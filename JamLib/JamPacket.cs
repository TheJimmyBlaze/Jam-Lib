using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib
{
    public class JamPacket
    {
        private struct ReceiveState
        {
            public SslStream Stream { get; set; }
            public JamPacket Packet { get; set; }
            public byte[] HeaderBytes { get; set; }
        }

        private AutoResetEvent sendCompleted = new AutoResetEvent(false);
        private static AutoResetEvent receiveCompleted = new AutoResetEvent(false);

        public JamPacketHeader Header { get; set; }
        public byte[] Data { get; set; }
        public bool ContainsData { get; set; } = false;

        public JamPacket() { }

        public JamPacket(Guid recipient, Guid sender, bool encrypted, byte[] data)
        {
            Header = new JamPacketHeader()
            {
                Receipient = recipient,
                Sender = sender,
                SendTimeUTC = DateTime.UtcNow,
                DataEncrypted = encrypted,
                DataLength = data.Length
            };

            Data = data;
            ContainsData = true;
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(Data);
        }

        public int Send(SslStream stream)
        {
            byte[] headerBytes = Header.GetBytes();
            byte[] sendBytes = new byte[headerBytes.Length + Data.Length];

            headerBytes.CopyTo(sendBytes, 0);
            Data.CopyTo(sendBytes, headerBytes.Length);

            stream.BeginWrite(sendBytes, 0, sendBytes.Length, SendCallback, stream);
            sendCompleted.WaitOne();

            return sendBytes.Length;
        }

        private void SendCallback(IAsyncResult result)
        {
            SslStream stream = result.AsyncState as SslStream;
            stream.EndWrite(result);
            stream.Flush();

            sendCompleted.Set();
        }

        public static JamPacket Receive(SslStream stream)
        {
            ReceiveState state = new ReceiveState()
            {
                Stream = stream,
                Packet = new JamPacket(),
                HeaderBytes = new byte[Marshal.SizeOf(typeof(JamPacketHeader))]
            };
            stream.BeginRead(state.HeaderBytes, 0, state.HeaderBytes.Length, ReceiveHeaderCallback, state);
            receiveCompleted.WaitOne();

            state.Packet.ContainsData = true;
            return state.Packet;
        }

        private static void ReceiveHeaderCallback(IAsyncResult result)
        {
            ReceiveState state = (ReceiveState)result.AsyncState;
            int bytesRead = state.Stream.EndRead(result);
            int bytesRequired = Marshal.SizeOf(state.Packet.Header.GetType());

            if (bytesRead != bytesRequired)
                throw new InvalidOperationException(string.Format("Did not read enought bytes to build a valid Header.\nRequire: {0}b, Read: {0}b.", bytesRequired, bytesRead));

            state.Packet.Header = new JamPacketHeader(state.HeaderBytes);
            state.Packet.Data = new byte[state.Packet.Header.DataLength];
            state.Stream.BeginRead(state.Packet.Data, 0, state.Packet.Header.DataLength, ReceiveDataCallback, state);
        }

        private static void ReceiveDataCallback(IAsyncResult result)
        {
            ReceiveState state = (ReceiveState)result.AsyncState;
            int bytesRead = state.Stream.EndRead(result);
            int bytesRequired = state.Packet.Header.DataLength;

            if (bytesRead != bytesRequired)
                throw new InvalidOperationException(string.Format("Did not read enough bytes to build fill buffer defined in Header.\nRequired: {0}b, Read: {0}b.", bytesRequired, bytesRead));

            receiveCompleted.Set();
        }
    }
}
