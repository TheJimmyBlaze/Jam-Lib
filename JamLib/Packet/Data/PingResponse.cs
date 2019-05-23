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

        private readonly ISerializer serializer;

        public PingResponse(DateTime pingTimeUtc, DateTime pongTimeUtc, ISerializer serializer)
        {
            PingTimeUtc = pingTimeUtc;
            PongTimeUtc = pongTimeUtc;
            this.serializer = serializer;
        }

        public PingResponse(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<PingResponse>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
