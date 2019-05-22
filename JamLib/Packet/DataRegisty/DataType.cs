using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.DataRegisty
{
    public class DataType
    {
        public string AppSigniture { get; set; }
        public string DataSigniture { get; set; }

        public Guid ID { get; set; }

        public DataType(string appSigniture, string dataSigniture)
        {
            AppSigniture = appSigniture;
            DataSigniture = dataSigniture;
        }

        public override bool Equals(object obj)
        {
            if (obj is DataType dataType)
            {
                if (dataType.AppSigniture == AppSigniture &&
                    dataType.DataSigniture == DataSigniture)
                    return true;
            }
            return false;
        }
    }
}
