using System.Runtime.InteropServices;

namespace CH.Snapshot
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct EntryHeader
    {
        public uint Hash;
        public uint KeySize;
        public uint DataSize;
    }
}