using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.DataRegisty
{
    public class DataTypeRegistry
    {
        #region Event Handlers
        public class RegisterEventArgs: EventArgs
        {
            public DataType RegisteredDataType { get; set; }
        }

        public class DeregisterEventArgs: EventArgs
        {
            public string AppSigniture { get; set; }
            public int Count { get; set; }
        }

        public EventHandler<RegisterEventArgs> DataTypeRegisteredEvent;
        public EventHandler<DeregisterEventArgs> DataTypesDeregisteredEvent;

        public void OnDataTypeRegistered(RegisterEventArgs e)
        {
            DataTypeRegisteredEvent?.Invoke(this, e);
        }
        public void OnDataTypeDeregistered(DeregisterEventArgs e)
        {
            DataTypesDeregisteredEvent?.Invoke(this, e);
        }
        #endregion

        private readonly List<DataType> registeredDataTypes = new List<DataType>();

        public DataTypeRegistry() { }

        public DataTypeRegistry(List<DataType> registeredDataTypes)
        {
            this.registeredDataTypes = registeredDataTypes;
        }

        public DataType Register(DataType dataType)
        {
            if (GetByData(dataType.AppSigniture, dataType.DataSigniture) != null)
                throw new InvalidOperationException(string.Format("A data type with the signiture: {0}.{1} has already been registered.", dataType.AppSigniture, dataType.DataSigniture));

            dataType.ID = Guid.NewGuid();
            registeredDataTypes.Add(dataType);

            OnDataTypeRegistered(new RegisterEventArgs() { RegisteredDataType = dataType });
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

        public void Deregister(string appSigniture)
        {
            int numRemoved = registeredDataTypes.RemoveAll(x => x.AppSigniture == appSigniture);
            OnDataTypeDeregistered(new DeregisterEventArgs() { AppSigniture = appSigniture, Count = numRemoved });
        }

        public List<DataType> GetByApp(string appSigniture)
        {
            return registeredDataTypes.Where(x => x.AppSigniture == appSigniture).ToList();
        }

        public DataType GetByData(string appSigniture, string dataSigniture)
        {
            return registeredDataTypes.SingleOrDefault(x => x.AppSigniture == appSigniture && x.DataSigniture == dataSigniture);
        }
    }
}
