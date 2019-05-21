using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain.Serialization
{
    public interface ISerializer
    {
        StructType GetStructFromBytes<StructType>(byte[] rawBytes);

        byte[] GetBytesFromStruct<StructType>(StructType structType);
    }
}
