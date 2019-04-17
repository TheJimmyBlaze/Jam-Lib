using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class InternalInterpreter : IJamPacketInterpreter
    {
        private readonly JamServerConnection client;

        public InternalInterpreter(JamServerConnection client)
        {
            this.client = client;
        }

        public void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case LoginRequest.DATA_TYPE:
                    client.Login(packet);
                    break;
                case PingRequest.DATA_TYPE:
                    client.Ping(packet);
                    break;
                case PlainTextImperative.DATA_TYPE:
                    //TODO: do something?
                    break;
                default:
                    client.Server.Interpreter.Interpret(packet);
                    break;
            }
        }
    }
}
