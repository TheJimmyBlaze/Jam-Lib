using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain.Cryptography
{
    public interface IHashFactory
    {
        byte[] BuildHash(string value);

        bool ValidateString(string value, byte[] storableHash);
    }
}
