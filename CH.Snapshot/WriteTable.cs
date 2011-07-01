using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CH.Snapshot
{
    public class WriteTable<T>
    {
        private readonly IConverter<T> _converter;

        private readonly IDictionary<KeyValuePair<uint, byte[]>, byte[]> _data =
            new Dictionary<KeyValuePair<uint, byte[]>, byte[]>(new HashHelper.CompareKey());

        public WriteTable(IConverter<T> converter)
        {
            _converter = converter;
        }

        public void Add(string key, T value)
        {
            _data.Add(HashHelper.StringToHashKey(key), _converter.AsByteArray(value));
        }

        public byte[] Resolve()
        {
            var count = (uint) _data.Count;

            ulong dataSize = 0;

            const uint maxBitCount = 26;
            var bitCount = BitHelper.GetNextBitCountNeededToRepresent(count);
            if (bitCount >= maxBitCount) throw new Exception("Too many entries in table.");

            ++bitCount;

            var hashCount = count == 0 ? 0 : (uint) (1 << bitCount);

            var sizeOfHeader = (ulong) Marshal.SizeOf(typeof (Header));
            var header =
                new Header
                    {
                        Version = 0,
                        EntryCount = count,
                        HashTableCount = hashCount,
                    };

            var sizeOfOffsetEntry = (ulong) BitConverter.GetBytes((ulong) 0L).Length;
            var sizeOfEntryHeader = (ulong) Marshal.SizeOf(typeof (EntryHeader));
            var hashTableSize = (hashCount + 1)*sizeOfOffsetEntry;
            // add an extra entry to store the end of entries offset
            var entryOffset = sizeOfHeader + hashTableSize;
            dataSize = entryOffset;
            ulong[] hashOffsetTable = null;
            SortedSet<KeyValuePair<uint, byte[]>> hashSet = null;
            if (hashCount > 0)
            {
                const int uintBitCount = 32;
                var hashShift = (uintBitCount - bitCount);

                hashSet = new SortedSet<KeyValuePair<uint, byte[]>>(_data.Keys, new HashHelper.CompareKey());
                hashOffsetTable = new ulong[hashCount + 1];
                for (var i = 0; i < hashCount; ++i) hashOffsetTable[i] = ulong.MaxValue;
                var e = hashSet.GetEnumerator();

                var currentOffset = entryOffset;
                e.MoveNext();
                var prev = e.Current.Key >> hashShift;
                hashOffsetTable[prev] = currentOffset;
                currentOffset += sizeOfEntryHeader + (ulong) e.Current.Value.Length +
                                 (ulong) _data[e.Current].Length;
                while (e.MoveNext())
                {
                    var curr = e.Current.Key >> hashShift;
                    if (curr != prev)
                    {
                        hashOffsetTable[curr] = currentOffset;
                        prev = curr;
                    }
                    currentOffset += sizeOfEntryHeader + (ulong) e.Current.Value.Length +
                                     (ulong) _data[e.Current].Length;
                }
                dataSize = currentOffset;
                hashOffsetTable[hashCount] = dataSize;
                for (var i = hashCount; i <= hashCount; --i)
                {
                    if (hashOffsetTable[i] == ulong.MaxValue)
                        hashOffsetTable[i] = hashOffsetTable[i + 1];
                }
            }

            var result = new byte[dataSize];

            {
                var currentOffset = 0UL;
                Array.Copy(MarshalHelper.ToByteArray(header), 0L, result, (long) currentOffset, (long) sizeOfHeader);
                currentOffset += sizeOfHeader;

                if (hashOffsetTable != null)
                {
                    foreach (var offset in hashOffsetTable)
                    {
                        Array.Copy(BitConverter.GetBytes(offset), 0L, result, (long) currentOffset,
                                   (long) sizeOfOffsetEntry);
                        currentOffset += sizeOfOffsetEntry;
                    }
                }
                if (hashSet!=null)
                    foreach (var hashKey in hashSet)
                    {
                        var entry = _data[hashKey];
                        var entryHeader =
                            new EntryHeader
                                {
                                    DataSize = (uint) entry.Length,
                                    Hash = hashKey.Key,
                                    KeySize = (uint) hashKey.Value.Length
                                };
                        Array.Copy(MarshalHelper.ToByteArray(entryHeader), 0L, result, (long) currentOffset,
                                   (long) sizeOfEntryHeader);
                        currentOffset += sizeOfEntryHeader;
                        Array.Copy(hashKey.Value, 0L, result, (long) currentOffset, entryHeader.KeySize);
                        currentOffset += entryHeader.KeySize;
                        Array.Copy(entry, 0L, result, (long) currentOffset, entryHeader.DataSize);
                        currentOffset += entryHeader.DataSize;
                    }
            }
            return result;
        }
    }
}