using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace CH.Snapshot
{
    public sealed class Mapper : IReadonlyData
    {
        private MemoryMappedFile _memoryMappedFile;
        private bool _disposed;
        private MemoryMappedViewAccessor _viewAccessor;

        public Mapper(string file)
        {
            _memoryMappedFile = MemoryMappedFile.CreateFromFile(file, FileMode.Open);
            _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, (new FileInfo(file)).Length, MemoryMappedFileAccess.Read);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                if (_viewAccessor != null)
                    _viewAccessor.Dispose();
                if (_memoryMappedFile != null)
                    _memoryMappedFile.Dispose();
            }
            _memoryMappedFile = null;
            _viewAccessor = null;
            _disposed = true;
        }

        public byte[] Read(ulong index, ulong length)
        {
            var temp = new byte[length];
            _viewAccessor.ReadArray((long) index, temp, 0, (int) length);
            return temp;
        }
    }
}