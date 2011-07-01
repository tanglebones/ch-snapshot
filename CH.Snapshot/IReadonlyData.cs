using System;

namespace CH.Snapshot
{
    public interface IReadonlyData : IDisposable
    {
        byte[] Read(ulong index, ulong length);
    }
}