using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JamLib
{
    public struct JamPacketHeader
    {
        public Guid Receipient { get; set; }
        public Guid Sender { get; set; }
        public DateTime SendTimeUTC { get; set; }

        public bool DataEncrypted { get; set; }
        public int DataLength { get; set; }

        public JamPacketHeader(byte[] rawBytes)
        {
            this = new JamPacketHeader();
            int size = Marshal.SizeOf(GetType());

            IntPtr pointer = Marshal.AllocHGlobal(size);
            Marshal.Copy(rawBytes, 0, pointer, size);
            this = (JamPacketHeader)Marshal.PtrToStructure(pointer, GetType());
            Marshal.FreeHGlobal(pointer);
        }

        public byte[] GetBytes()
        {
            int size = Marshal.SizeOf(GetType());
            byte[] bytes = new byte[size];

            IntPtr pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, pointer, true);
            Marshal.Copy(pointer, bytes, 0, size);
            Marshal.FreeHGlobal(pointer);

            return bytes;
        }
    }
}
