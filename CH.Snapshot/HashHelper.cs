using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CH.Snapshot
{
    public static class HashHelper
    {
        public static KeyValuePair<uint, byte[]> StringToHashKey(string key)
        {
            var b = Encoding.UTF8.GetBytes(key);
            var h = Crc32.Crc.Crc32(b);
            return new KeyValuePair<uint, byte[]>(h, b);
        }

        public class CompareKey : IEqualityComparer<KeyValuePair<uint, byte[]>>, IComparer<KeyValuePair<uint, byte[]>>
        {
            public int Compare(KeyValuePair<uint, byte[]> x, KeyValuePair<uint, byte[]> y)
            {
                return x.Key < y.Key ? -1 : x.Key > y.Key ? 1 : 0;
            }

            public bool Equals(KeyValuePair<uint, byte[]> x, KeyValuePair<uint, byte[]> y)
            {
                if (x.Key != y.Key) return false;
                if (x.Value.Length != y.Value.Length) return false;
                return !x.Value.Where((t, i) => t != y.Value[i]).Any();
            }

            public int GetHashCode(KeyValuePair<uint, byte[]> obj)
            {
                return (int) obj.Key;
            }
        }
    }
}