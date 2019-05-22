using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.DataRegisty
{
    public class DataTypeRegistry
    {
        private readonly List<DataType> registeredDataTypes = new List<DataType>();

        public DataType Register(DataType dataType)
        {
            if (GetByData(dataType) != null)
                throw new InvalidOperationException(string.Format("A data type with the signiture: {0}.{1} has already been registered.", dataType.AppSigniture, dataType.DataSigniture));

            dataType.ID = Guid.NewGuid();
            registeredDataTypes.Add(dataType);

            return dataType;
        }

        public List<DataType> BulkRegister(List<DataType> dataTypes)
        {
            List<DataType> registeredDataTypes = new List<DataType>();
            foreach (DataType dataType in dataTypes)
            {
                registeredDataTypes.Add(Register(dataType));
            }

            return registeredDataTypes;
        }

        public List<DataType> GetByApp(string appSigniture)
        {
            return registeredDataTypes.Where(x => x.AppSigniture == appSigniture).ToList();
        }

        public DataType GetByData(DataType dataType)
        {
            return registeredDataTypes.SingleOrDefault(x => x == dataType);
        }
    }
}
