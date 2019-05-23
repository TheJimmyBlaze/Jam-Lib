using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct PingRequest
    {
        public const int DATA_TYPE = 5;
        public const string DATA_SIGNITURE = "PingRequest";

        public DateTime PingTimeUtc { get; set; }

        private readonly ISerializer serializer;

        public PingRequest(DateTime pingTimeUtc, ISerializer serializer)
        {
            PingTimeUtc = pingTimeUtc;
            this.serializer = serializer;
        }
        
        public PingRequest(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<PingRequest>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
