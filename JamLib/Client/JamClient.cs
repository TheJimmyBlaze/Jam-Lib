using JamLib.Database;
using JamLib.Domain;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        #region Event Handlers
        public class MessageReceivedEventArgs: EventArgs
        {
            public JamPacket Packet { get; set; }
        }

        public EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public EventHandler ClientDisposedEvent;

        public void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceivedEvent?.Invoke(this, e);
        }

        public void OnClientDisposed(EventArgs e)
        {
            ClientDisposedEvent?.Invoke(this, e);
        }
        #endregion

        private readonly AutoResetEvent connectionCompleted = new AutoResetEvent(false);

        private SslStream stream;
        private bool alive;

        private readonly ConcurrentQueue<JamPacket> packetSendQueue = new ConcurrentQueue<JamPacket>();

        public Account Account { get; set; }

        public bool IsConnected
        {
            get { return stream != null && stream.CanWrite && stream.CanRead; }
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
                    Task.Run(() => SendPacketsFromQueue());
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
            stream?.Close();
            OnClientDisposed(null);
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

            Guid accountID = Guid.Empty;
            if (Account != null)
                accountID = Account.AccountID;

            JamPacket responsePacket = new JamPacket(pingPacket.Header.Sender, accountID, PingResponse.DATA_TYPE, response.GetBytes());
            Send(responsePacket);
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
                InternalClientInterpreter.Interpret(this, packet);
            }
        }
    }
}
