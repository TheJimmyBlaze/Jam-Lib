using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct PingResponse
    {
        public const int DATA_TYPE = 6;

        public DateTime PingTimeUtc { get; set; }
        public DateTime PongTimeUtc { get; set; }

        public PingResponse(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<PingResponse>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}
