using JamLib.Domain.Serialization;
using JamLib.Packet.DataRegisty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct RegisterDataTypesResponse
    {
        public const int DATA_TYPE = 16;

        public List<DataType> RegisteredDataTypes { get; set; }

        private readonly ISerializer serializer;

        public RegisterDataTypesResponse(List<DataType> registeredDataTypes, ISerializer serializer)
        {
            RegisteredDataTypes = registeredDataTypes;
            this.serializer = serializer;
        }

        public RegisterDataTypesResponse(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<RegisterDataTypesResponse>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
