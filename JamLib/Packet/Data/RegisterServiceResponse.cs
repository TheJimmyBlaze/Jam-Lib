using JamLib.Domain.Serialization;
using JamLib.Packet.DataRegisty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct RegisterServiceResponse
    {
        public const int DATA_TYPE = 16;
        public const string DATA_SIGNITURE = "RegisterServiceResponse";

        public List<DataType> RegisteredDataTypes { get; set; }

        private readonly ISerializer serializer;

        public RegisterServiceResponse(List<DataType> registeredDataTypes, ISerializer serializer)
        {
            RegisteredDataTypes = registeredDataTypes;
            this.serializer = serializer;
        }

        public RegisterServiceResponse(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<RegisterServiceResponse>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
