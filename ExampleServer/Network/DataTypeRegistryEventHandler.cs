using JamLib.Packet.DataRegisty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network
{
    internal static class DataTypeRegistryEventHandler
    {
        internal static void OnDataTypeRegistered(object sender, DataTypeRegistry.RegisterEventArgs e)
        {
            Console.WriteLine("Registered DataType: {0}.{1}", e.RegisteredDataType.AppSigniture, e.RegisteredDataType.DataSigniture);
        }

        internal static void OnDataTypeDeregistered(object sender, DataTypeRegistry.DeregisterEventArgs e)
        {
            Console.WriteLine("Deregistered {0} DataTypes from: {1}", e.Count, e.AppSigniture);
        }
    }
}
