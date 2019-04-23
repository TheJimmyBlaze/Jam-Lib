using JamLib.Domain.Cryptography;
using JamLib.Packet;
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
        public EventHandler<MessageReceivedEventArgs> MessageReceived;
        public EventHandler<MessageReceivedEventArgs> UndeliveredMessageReceived;
        public class MessageReceivedEventArgs : EventArgs
        {
            public JamServerConnection ServerConnection { get; set; }
            public JamPacket Packet { get; set; }
        }
        
        private readonly List<JamServerConnection> connections = new List<JamServerConnection>();
        private readonly AutoResetEvent acceptCompleted = new AutoResetEvent(false);

        private X509Certificate serverCertificate;
        public readonly IHashFactory HashFactory;

        private bool alive;

        public JamServer(IHashFactory hashFactory)
        {
            HashFactory = hashFactory;
        }

        public JamServerConnection GetConnection(Guid accountID)
        {
            return connections.SingleOrDefault(x => x.Account.AccountID == accountID);
        }

        public void AddConnection(JamServerConnection connection)
        {
            connections.Add(connection);
        }

        public void DeleteConnection(Guid accountID)
        {
            connections.Remove(GetConnection(accountID));
        }

        public void Start(int port, string certificate, string certificatePassword)
        {
            serverCertificate = new X509Certificate2(certificate, certificatePassword, X509KeyStorageFlags.Exportable);
            alive = true;

            Task.Run(() => { Listen(port); });
        }

        public void Dispose()
        {
            alive = false;
        }

        private void Listen(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (alive)
            {
                TcpClient client = listener.AcceptTcpClient();
                SslStream inProgressStream = new SslStream(client.GetStream(), false);

                inProgressStream.BeginAuthenticateAsServer(serverCertificate, AcceptCallback, inProgressStream);
                acceptCompleted.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            SslStream stream = result.AsyncState as SslStream;
            stream.EndAuthenticateAsServer(result);

            JamServerConnection connection = new JamServerConnection(stream, this);

            acceptCompleted.Set();
        }

        public void OnMessageReceived(MessageReceivedEventArgs args)
        {
            MessageReceived?.Invoke(this, args);
        }

        public void OnUndelieveredMessageReceived(MessageReceivedEventArgs args)
        {
            UndeliveredMessageReceived?.Invoke(this, args);
        }
    }
}
