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

        public InternalInterpreter InternalInterpreter;

        public Account Account { get; private set; }

        public JamServerConnection(SslStream stream, JamServer server)
        {
            InternalInterpreter = new InternalInterpreter(this);
            Server = server;

            this.stream = stream;
            alive = true;
            
            Task.Run(() => Listen());
        }
        
        public void Dispose()
        {
            alive = false;
            stream.Close();
        }

        public void Login(JamPacket loginPacket)
        {
            if (loginPacket.Header.DataType != LoginRequest.DATA_TYPE)
                return;

            LoginRequest request = new LoginRequest(loginPacket.Data);

            LoginResponse response;
            try
            {
                Account = JamAccountFactory.Authenticate(request.Username, request.Password, Server.HashFactory);

                response = new LoginResponse()
                {
                    Result = LoginResponse.LoginResult.Good,
                    Account = Account
                };
            }
            catch (JamAccountFactory.InvalidUsernameException)
            {
                response = new LoginResponse() { Result = LoginResponse.LoginResult.BadUsername };
            }
            catch (JamAccountFactory.InvalidAccessCodeException)
            {
                response = new LoginResponse() { Result = LoginResponse.LoginResult.BadPassword };
            }

            JamPacket responsePacket = new JamPacket(Guid.Empty, Guid.Empty, LoginResponse.DATA_TYPE, response.GetBytes());
            Send(responsePacket);

            Server.AddConnection(this);
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

            if (Account != null)
                Server.DeleteConnection(Account.AccountID);
        }

        public int Send(JamPacket packet)
        {
            int sentBytes = 0;

            Task.Run(() => { sentBytes = packet.Send(stream); });

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
    }
}
