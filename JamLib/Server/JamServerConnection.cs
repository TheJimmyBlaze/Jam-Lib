using JamLib.Database;
using JamLib.Domain;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
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

        public InternalServerInterpreter InternalInterpreter;

        public Account Account { get; private set; }

        public JamServerConnection(SslStream stream, JamServer server)
        {
            const int DISCONNECT_POLL_FREQUENCY = 500;

            InternalInterpreter = new InternalServerInterpreter(this);
            Server = server;

            this.stream = stream;
            alive = true;

            Task.Run(() => PollConnection(DISCONNECT_POLL_FREQUENCY));
            Task.Run(() => Listen());
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

        public int Send(JamPacket packet)
        {
            int sentBytes = 0;

            sentBytes = packet.Send(stream);

            while (sentBytes == 0)
                Thread.Sleep(50);

            return sentBytes;
        }

        private void Listen()
        {
            while (alive)
            {
                JamPacket packet = JamPacket.Receive(stream);
                Server.Router.Route(packet, this);
            }
        }

        private void PollConnection(int pollFrequency)
        {
            while (alive)
            {
                Thread.Sleep(pollFrequency);
                
                try
                {
                    PingRequest pingRequest = new PingRequest()
                    {
                        PingTimeUtc = DateTime.UtcNow
                    };

                    JamPacket pingPacket = new JamPacket(Guid.Empty, Guid.Empty, PingRequest.DATA_TYPE, pingRequest.GetBytes());
                    Send(pingPacket);
                }
                catch (System.IO.IOException)
                {
                    Dispose();
                }
            }
        }
    }
}
