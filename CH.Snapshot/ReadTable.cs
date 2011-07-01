using System;
using System.Linq;
using System.Runtime.InteropServices;

// TODO
//  - add support for nested tables that don't result in copying in nested table out as a byte array
//    - need to return an IReadonlyData<byte> instead of byte[]
//    - implementation of IReadonlyData<byte> need to support an offset into an inner IReadonlyData<byte>

namespace CH.Snapshot
{
    public class ReadTable<T>
    {
        private readonly IConverter<T> _converter;
        private readonly IReadonlyData _data;
        private readonly ulong _hashTableOffset;
        private readonly uint _hashTableCount;
        private readonly int _hashShift;
        private static readonly ulong SizeOfEntryHeader = (ulong) Marshal.SizeOf(typeof (EntryHeader));
        private static readonly ulong SizeOfOffsetEntry = (ulong) BitConverter.GetBytes((ulong) 0L).Length;

        public ReadTable(IConverter<T> converter, IReadonlyData data)
        {
            _converter = converter;
            _data = data;
            var sizeOfHeader = (ulong) Marshal.SizeOf(typeof (Header));
            var headerBytes = _data.Read(0, sizeOfHeader);
            var header = MarshalHelper.FromByteArray<Header>(headerBytes);

            if (header.Version != 0)
                throw new VersionNotUnderstoodException();

            _hashTableCount = header.HashTableCount;
            if ((_hashTableCount & (_hashTableCount - 1)) != 0)
                throw new InvalidHashTableCountInHeaderException();

            _hashTableOffset = sizeOfHeader;

            const int uintBitCount = 32;
            var bitCount = BitHelper.GetNextBitCountNeededToRepresent(_hashTableCount);
            _hashShift = (uintBitCount - bitCount);
        }

        public bool TryGetValue(string key, out T value)
        {
            value = _converter.Empty();
            if (_hashTableCount == 0) return false;

            var hashKey = HashHelper.StringToHashKey(key);
            var index = hashKey.Key >> _hashShift;

            var dataOffset = _hashTableOffset + SizeOfOffsetEntry*index;
            var offsetEntryBytes = _data.Read(dataOffset, SizeOfOffsetEntry*2);
            var offset = BitConverter.ToUInt64(offsetEntryBytes, 0);

            var endOffset = BitConverter.ToUInt64(offsetEntryBytes, (int) SizeOfOffsetEntry);
            while (offset < endOffset)
            {
                var entryHeaderBytes = _data.Read(offset, SizeOfEntryHeader);
                var entryHeader = MarshalHelper.FromByteArray<EntryHeader>(entryHeaderBytes);

                var entryKeyOffset = offset + SizeOfEntryHeader;
                var entryDataOffset = entryKeyOffset + entryHeader.KeySize;
                if (entryHeader.Hash == hashKey.Key)
                {
                    var entryKeyBytes = _data.Read(entryKeyOffset, entryHeader.KeySize);
                    if(!entryKeyBytes.Where((t, i) => t != hashKey.Value[i]).Any())
                    {
                        var data = _data.Read(entryDataOffset, entryHeader.DataSize);
                        value = _converter.AsT(data);
                        return true;
                    }
                }
                offset = entryDataOffset + entryHeader.DataSize;
            }
            return false;
        }
    }
}