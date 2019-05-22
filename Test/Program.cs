using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        public struct Test
        {
            public Guid Guid { get; set; }
        }

        static void Main(string[] args)
        {
            Test test = new Test()
            {
                Guid = Guid.NewGuid()
            };

            byte[] bytes = StructMarshal.GetBytesFromStruct(test);
        }
    }
}
