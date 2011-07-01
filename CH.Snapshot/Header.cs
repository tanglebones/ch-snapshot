using System.Runtime.InteropServices;

namespace CH.Snapshot
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct Header
    {
        public uint Version;
        public uint HashTableCount;
        public uint EntryCount;
    }
}