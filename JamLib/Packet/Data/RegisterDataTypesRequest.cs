using JamLib.Domain.Serialization;
using JamLib.Packet.DataRegisty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct RegisterDataTypesRequest
    {
        public const int DATA_TYPE = 15;

        public List<DataType> DataTypes { get; set; }

        private readonly ISerializer serializer;

        public RegisterDataTypesRequest(List<DataType> dataTypes, ISerializer serializer)
        {
            DataTypes = dataTypes;
            this.serializer = serializer;
        }

        public RegisterDataTypesRequest(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<RegisterDataTypesRequest>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}
