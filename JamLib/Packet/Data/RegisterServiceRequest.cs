using JamLib.Domain.Serialization;
using JamLib.Packet.DataRegisty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct RegisterServiceRequest
    {
        public const int DATA_TYPE = 15;
        public const string DATA_SIGNITURE = "RegisterServiceRequest";

        public List<DataType> ServiceDataTypes { get; set; }

        private readonly ISerializer serializer;

        public RegisterServiceRequest(List<DataType> serviceDataTypes, ISerializer serializer)
        {
            ServiceDataTypes = serviceDataTypes;
            this.serializer = serializer;
        }

        public RegisterServiceRequest(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<RegisterServiceRequest>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
