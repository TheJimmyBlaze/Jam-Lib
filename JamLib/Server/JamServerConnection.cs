using JamLib.Database;
using JamLib.Domain;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class JamServerConnection: IDisposable
    {
        public readonly JamServer Server;

        private readonly SslStream stream;
        private bool alive;

        private readonly ConcurrentQueue<JamPacket> packetSendQueue = new ConcurrentQueue<JamPacket>();

        public Account Account { get; private set; }

        public JamServerConnection(SslStream stream, JamServer server)
        {
            const int DISCONNECT_POLL_FREQUENCY = 500;

            Server = server;

            this.stream = stream;
            alive = true;

            Task.Run(() => Listen());

            Task.Run(() => SendPacketsFromQueue());
            Task.Run(() => PollConnection(DISCONNECT_POLL_FREQUENCY));
        }
        
        public void Dispose()
        {
            alive = false;
            stream.Close();
            
            if (Account != null)
                Server.DeleteConnection(Account.AccountID);
        }

        public void Login(JamPacket loginPacket)
        {
            if (loginPacket.Header.DataType != LoginRequest.DATA_TYPE)
                return;

            LoginRequest request = new LoginRequest(loginPacket.Data);

            LoginResponse response;
            try
            {
                Account = AccountFactory.Authenticate(request.Username, request.Password, Server.HashFactory);

                response = new LoginResponse()
                {
                    Result = LoginResponse.LoginResult.Good,
                    Account = Account
                };

                Server.AddConnection(this);
            }
            catch (AccountFactory.InvalidUsernameException)
            {
                response = new LoginResponse() { Result = LoginResponse.LoginResult.BadUsername };
            }
            catch (AccountFactory.InvalidAccessCodeException)
            {
                response = new LoginResponse() { Result = LoginResponse.LoginResult.BadPassword };
            }

            JamPacket responsePacket = new JamPacket(Guid.Empty, Guid.Empty, LoginResponse.DATA_TYPE, response.GetBytes());
            Send(responsePacket);
        }

        public void Ping(JamPacket pingPacket)
        {
            if (pingPacket.Header.DataType != PingRequest.DATA_TYPE)
                return;

            PingRequest request = new PingRequest(pingPacket.Data);

            PingResponse response = new PingResponse()
            {
                PingTimeUtc = request.PingTimeUtc,
                PongTimeUtc = DateTime.UtcNow
            };

            JamPacket responsePacket = new JamPacket(Guid.Empty, Guid.Empty, PingResponse.DATA_TYPE, response.GetBytes());
            Send(responsePacket);
        }

        public void Logout()
        {
            Dispose();
        }

        public void Send(JamPacket packet)
        {
            packetSendQueue.Enqueue(packet);
        }

        private void SendPacketsFromQueue()
        {
            while (alive)
            {
                Thread.Sleep(50);

                if (packetSendQueue.Count > 0 && stream.CanWrite)
                {
                    try
                    {
                        if (packetSendQueue.TryDequeue(out JamPacket sendPacket))
                            sendPacket.Send(stream);
                    }
                    catch (IOException)
                    {
                        Dispose();
                    }
                }
            }
        }

        private void Listen()
        {
            while (alive)
            {
                JamPacket packet = JamPacket.Receive(stream);
                JamPacketRouter.Route(this, packet);
            }
        }

        private void PollConnection(int pollFrequency)
        {
            while (alive)
            {
                Thread.Sleep(pollFrequency);
                
                PingRequest pingRequest = new PingRequest()
                {
                    PingTimeUtc = DateTime.UtcNow
                };

                JamPacket pingPacket = new JamPacket(Guid.Empty, Guid.Empty, PingRequest.DATA_TYPE, pingRequest.GetBytes());
                Send(pingPacket);
            }
        }
    }
}
