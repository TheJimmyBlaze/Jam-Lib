using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Client
{
    public class JamClient: IDisposable
    {
        private AutoResetEvent connectionCompleted = new AutoResetEvent(false);

        private SslStream stream;
        private bool alive;

        public void Connect(string ip, int port, int timeout)
        {
            TcpClient client = new TcpClient(ip, port);
            SslStream inProgressStream = new SslStream(client.GetStream(), false, ValidateCertificate);

            inProgressStream.BeginAuthenticateAsClient(ip, ConnectCallback, inProgressStream);
            connectionCompleted.WaitOne(timeout);

            if (stream != null && stream.IsAuthenticated)
            {
                alive = true;
                Task.Run(() => Listen());
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            stream = result.AsyncState as SslStream;
            stream.EndAuthenticateAsClient(result);
            connectionCompleted.Set();
        }

        protected virtual bool ValidateCertificate(object sender, X509Certificate serverCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //This basic method simply returns true, method should be overriden if certificate validation implementation is required.
            return true;
        }

        public void Dispose()
        {
            alive = false;
            stream.Close();
        }

        public int Send(JamPacket packet)
        {
            int sentBytes = 0;

            Task.Run(() => { sentBytes = packet.Send(stream); });

            while (sentBytes == 0)
                Thread.Sleep(50);

            return sentBytes;
        }

        public void Listen()
        {
            while (alive)
            {
                JamPacket packet = JamPacket.Receive(stream);
                Console.WriteLine(packet);
            }
        }
    }
}
