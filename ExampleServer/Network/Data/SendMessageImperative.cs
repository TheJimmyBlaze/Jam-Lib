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

        public SendMessageImperative(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<SendMessageImperative>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}
