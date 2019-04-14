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

namespace JamLib.Server
{
    public class JamServer: IDisposable
    {
        private List<JamServerConnection> connections = new List<JamServerConnection>();
        private AutoResetEvent acceptCompleted = new AutoResetEvent(false);

        private X509Certificate serverCertificate;

        private bool alive;

        public void Start(int port, string certificate)
        {
            serverCertificate = X509Certificate.CreateFromCertFile(certificate);
            alive = true;

            Task.Run(() => { Listen(port); });
        }

        public void Dispose()
        {
            alive = false;
        }

        public void Listen(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (alive)
            {
                TcpClient client = listener.AcceptTcpClient();
                SslStream stream = new SslStream(client.GetStream(), false);

                stream.BeginAuthenticateAsServer(serverCertificate, AcceptCallback, stream);
                acceptCompleted.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult result)
        {
            SslStream stream = result.AsyncState as SslStream;
            stream.EndAuthenticateAsServer(result);

            JamServerConnection connection = new JamServerConnection(stream);
            connections.Add(connection);

            acceptCompleted.Set();
        }
    }
}
