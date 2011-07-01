using System;

namespace CH.Snapshot
{
    public sealed class ReadOnlyByteArray : IReadonlyData
    {
        private readonly byte[] _data;

        public ReadOnlyByteArray(byte[] data)
        {
            _data = data;
        }

        public void Dispose()
        {
        }

        public byte[] Read(ulong index, ulong length)
        {
            var temp = new byte[length];
            Array.Copy(_data, (long) index, temp, 0, (long) length);
            return temp;
        }
    }
}