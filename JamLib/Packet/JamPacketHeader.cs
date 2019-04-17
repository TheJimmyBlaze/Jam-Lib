using JamLib.Domain.Serialization;
using System;

namespace JamLib.Packet
{
    public struct JamPacketHeader
    {
        public Guid Receipient;
        public Guid Sender;
        public DateTime SendTimeUTC;

        public int DataType;
        public int DataLength;

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
