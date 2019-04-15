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

        public JamAccount Account { get; private set; }

        public JamServerConnection(SslStream stream, JamServer server)
        {
            this.Server = server;

            this.stream = stream;
            alive = true;
            
            Task.Run(() => Listen());
        }
        
        public void Dispose()
        {
            alive = false;
            stream.Close();
        }

        public void Login(JamPacket requestPacket)
        {
            LoginRequest request = new LoginRequest(requestPacket.Data);

            LoginResponse response;
            try
            {
                Account = JamAccount.Authenticate(request.Username, request.Password, Server.HashFactory);
                response = new LoginResponse()
                {
                    Result = LoginResponse.LoginResult.Good,
                    Account = Account
                };
            }
            catch (JamAccount.InvalidUsernameException)
            {
                response = new LoginResponse() { Result = LoginResponse.LoginResult.BadUsername };
            }
            catch (JamAccount.InvalidAccessCodeException)
            {
                response = new LoginResponse() { Result = LoginResponse.LoginResult.BadPassword };
            }

            JamPacket responsePacket = new JamPacket(Account.AccountID, Guid.Empty, LoginResponse.DATA_TYPE, response.GetBytes());
            Send(responsePacket);

            Server.AddConnection(this);
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
