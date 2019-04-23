using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Client
{
    internal static class InternalClientInterpreter
    {
        internal static void Interpret(JamClient client, JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case PingRequest.DATA_TYPE:
                    client.Ping(packet);
                    break;
                default:
                    JamClient.MessageReceivedEventArgs args = new JamClient.MessageReceivedEventArgs()
                    {
                        Packet = packet
                    };
                    client.OnMessageReceived(args);
                    break;
            }
        }
    }
}
