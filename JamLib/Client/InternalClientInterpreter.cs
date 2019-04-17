using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Client
{
    public class InternalClientInterpreter : IJamPacketInterpreter
    {
        private readonly JamClient client;

        public InternalClientInterpreter(JamClient client)
        {
            this.client = client;
        }

        public void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case PingRequest.DATA_TYPE:
                    client.Ping(packet);
                    break;
                default:
                    client.Interperter.Interpret(packet);
                    break;
            }
        }
    }
}
