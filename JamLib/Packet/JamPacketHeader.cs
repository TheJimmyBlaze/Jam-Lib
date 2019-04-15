using JamLib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet
{
    public struct JamPacketHeader
    {
        public Guid Receipient { get; set; }
        public Guid Sender { get; set; }
        public DateTime SendTimeUTC { get; set; }

        public int DataType { get; set; }
        public int DataLength { get; set; }

        public JamPacketHeader(byte[] rawBytes)
        {
            this = StructMarshal.GetStructFromBytes<JamPacketHeader>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return StructMarshal.GetBytesFromStruct(this);
        }
    }
}
