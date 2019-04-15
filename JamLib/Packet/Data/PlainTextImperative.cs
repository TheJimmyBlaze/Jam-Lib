using JamLib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct PlainTextImperative
    {
        public const int DATA_TYPE = 1;

        public string Text { get; set; }

        public PlainTextImperative(byte[] rawBytes)
        {
            this = StructMarshal.GetStructFromBytes<PlainTextImperative>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return StructMarshal.GetBytesFromStruct(this);
        }
    }
}
