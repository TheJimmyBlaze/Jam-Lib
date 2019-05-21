using JamLib.Domain;
using JamLib.Domain.Serialization;
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

        private readonly ISerializer serializer;

        public PlainTextImperative(string text, ISerializer serializer)
        {
            Text = text;
            this.serializer = serializer;
        }

        public PlainTextImperative(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<PlainTextImperative>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
