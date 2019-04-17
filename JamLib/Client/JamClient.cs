using JamLib.Domain;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Client
{
    public class JamClient: IDisposable
    {
        private AutoResetEvent connectionCompleted = new AutoResetEvent(false);

        private SslStream stream;
        private bool alive;

        public readonly IJamPacketInterpreter Interperter;

        public JamAccountFactory Account;

        public bool IsConnected
        {
            get { return stream.CanWrite && stream.CanRead; }
        }

        public JamClient(IJamPacketInterpreter interpreter)
        {
            Interperter = interpreter;
        }
        
        protected virtual bool ValidateCertificate(object sender, X509Certificate serverCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //This basic method simply returns true, method should be overriden if certificate validation implementation is required.
            return true;
        }

        public void Connect(string ip, int port, int timeout)
        {
            try
            {
                TcpClient client = new TcpClient(ip, port);
                SslStream inProgressStream = new SslStream(client.GetStream(), false, ValidateCertificate);
                
                inProgressStream.BeginAuthenticateAsClient(ip, null, SslProtocols.Default, false, ConnectCallback, inProgressStream);
                connectionCompleted.WaitOne(timeout);

                if (stream != null && stream.IsAuthenticated)
                {
                    alive = true;
                    Task.Run(() => Listen());
                }
            }
            catch (SocketException) { }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            stream = result.AsyncState as SslStream;
            stream.EndAuthenticateAsClient(result);
            connectionCompleted.Set();
        }

        public void Dispose()
        {
            alive = false;
            stream.Close();
        }

        public void Login(string username, string password)
        {
            LoginRequest loginRequest = new LoginRequest()
            {
                Username = username,
                Password = password
            };

            JamPacket packet = new JamPacket(Guid.Empty, Guid.Empty, LoginRequest.DATA_TYPE, loginRequest.GetBytes());
            Send(packet);
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
                Interperter.Interpret(packet);
            }
        }
    }
}
