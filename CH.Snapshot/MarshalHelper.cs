using System.Runtime.InteropServices;

namespace CH.Snapshot
{
    public static class MarshalHelper
    {
        public static byte[] ToByteArray<TX>(TX obj)
        {
            var inBufferSize = Marshal.SizeOf(typeof (TX));
            var inBuffer = Marshal.AllocHGlobal(inBufferSize);
            var bytes = new byte[inBufferSize];
            try
            {
                Marshal.StructureToPtr(obj, inBuffer, false);
                Marshal.Copy(inBuffer, bytes, 0, inBufferSize);
            }
            finally
            {
                Marshal.FreeHGlobal(inBuffer);
            }
            return bytes;
        }

        public static TX FromByteArray<TX>(byte[] array)
        {
            var objectType = typeof (TX);
            var inBufferSize = Marshal.SizeOf(objectType);
            var inBuffer = Marshal.AllocHGlobal(inBufferSize);
            try
            {
                Marshal.Copy(array, 0, inBuffer, inBufferSize);
                return (TX) Marshal.PtrToStructure(inBuffer, objectType);
            }
            finally
            {
                Marshal.FreeHGlobal(inBuffer);
            }
        }
    }
}