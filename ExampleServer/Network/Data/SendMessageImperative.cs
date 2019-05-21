using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network.Data
{
    public struct SendMessageImperative
    {
        public const int DATA_TYPE = 110;

        public string Message { get; set; }

        private readonly ISerializer serializer;

        public SendMessageImperative(string message, ISerializer serializer)
        {
            Message = message;
            this.serializer = serializer;
        }

        public SendMessageImperative(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<SendMessageImperative>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
