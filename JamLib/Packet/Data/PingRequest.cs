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

        public DateTime PingTimeUtc { get; set; }
        
        public PingRequest(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<PingRequest>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}
