using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer
{
    public class ChatServerInterpreter : IJamPacketInterpreter
    {
        public void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case PlainTextImperative.DATA_TYPE:
                    WritePlainTextToConsole(packet);
                    break;
            }
        }

        private void WritePlainTextToConsole(JamPacket packet)
        {
            PlainTextImperative plainText = new PlainTextImperative(packet.Data);
            Console.WriteLine("[{0}]: {1}", packet.Header.Sender, plainText.Text);
        }
    }
}
