using JamLib.Database;
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
        #region Event Handlers
        public class MessageReceivedEventArgs : EventArgs
        {
            public JamServerConnection ServerConnection { get; set; }
            public JamPacket Packet { get; set; }
        }

        public class ConnectionEventArgs: EventArgs
        {
            public TcpClient Client { get; set; }
        }

        public class IdentifiedConnectionEventArgs: EventArgs
        {
            public TcpClient Client { get; set; }
            public Account Account { get; set; }
        }

        public EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public EventHandler<MessageReceivedEventArgs> UndeliveredMessageReceivedEvent;

        public EventHandler<ConnectionEventArgs> ClientConnectedEvent;
        public EventHandler<ConnectionEventArgs> ClientDisconnectedEvent;

        public EventHandler<IdentifiedConnectionEventArgs> ClientIdentifiedEvent;
        public EventHandler<ConnectionEventArgs> ClientInvalidUsernameEvent;
        public EventHandler<ConnectionEventArgs> ClientInvalidPasswordEvent;

        public EventHandler<IdentifiedConnectionEventArgs> ClientConnectedElsewhereEvent;

        public void OnMessageReceived(MessageReceivedEventArgs args)
        {
            MessageReceivedEvent?.Invoke(this, args);
        }

        public void OnUndelieveredMessageReceived(MessageReceivedEventArgs args)
        {
            UndeliveredMessageReceivedEvent?.Invoke(this, args);
        }

        public void OnClientConnected(ConnectionEventArgs args)
        {
            ClientConnectedEvent?.Invoke(this, args);
        }

        public void OnClientDisconnected(ConnectionEventArgs args)
        {
            ClientDisconnectedEvent?.Invoke(this, args);
        }

        public void OnClientIdentified(IdentifiedConnectionEventArgs args)
        {
            ClientIdentifiedEvent?.Invoke(this, args);
        }

        public void OnClientInvalidUsername(ConnectionEventArgs args)
        {
            ClientInvalidUsernameEvent?.Invoke(this, args);
        }

        public void OnClientInvalidPassword(ConnectionEventArgs args)
        {
            ClientInvalidPasswordEvent?.Invoke(this, args);
        }

        public void OnClientConnectedElsewhere(IdentifiedConnectionEventArgs args)
        {
            ClientConnectedElsewhereEvent?.Invoke(this, args);
        }
        #endregion

        private struct ConnectState
        {
            public TcpClient Client { get; set; }
            public SslStream Stream { get; set; }
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
                SslStream stream = new SslStream(client.GetStream(), false);

                ConnectState state = new ConnectState()
                {
                    Client = client,
                    Stream = stream
                };

                stream.BeginAuthenticateAsServer(serverCertificate, AcceptCallback, state);
                acceptCompleted.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            ConnectState state = (ConnectState)result.AsyncState;
            state.Stream.EndAuthenticateAsServer(result);

            JamServerConnection connection = new JamServerConnection(state.Client, state.Stream, this);

            acceptCompleted.Set();
        }
    }
}
