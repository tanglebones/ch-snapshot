using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace CH.Snapshot.Test
{
    [TestFixture]
    public class SnapshotTestFixture
    {
        private static readonly TestDataEntry[] TestData =
            new[]
                {
                    new TestDataEntry("a", new TestData {Id = 1, Name = "atest"}),
                    new TestDataEntry("bw", new TestData {Id = 2, Name = "btest w"}),
                    new TestDataEntry("cqq", new TestData {Id = 3, Name = "ctest qq"}),
                    new TestDataEntry("dxx", new TestData {Id = 4, Name = "dtest xxx"}),
                    new TestDataEntry("ey", new TestData {Id = 5, Name = "etest hello"}),
                    new TestDataEntry("", new TestData {Id = 6, Name = "ftest world"}),
                };

        [Test]
        public void BasicBuild()
        {
            var table = new WriteTable<TestData>(new DataConverter<TestData>());
            var data = table.Resolve();
            Assert.IsNotNull(data);
            Assert.Greater(data.Length, 0);
        }

        [Test]
        public void BasicWorkflow()
        {
            var data = GetResolvedTestData();
            {
                ReadTable<TestData> table;
                using (var readOnlyByteArray = new ReadOnlyByteArray(data))
                {
                    table = new ReadTable<TestData>(new DataConverter<TestData>(), readOnlyByteArray);
                }
                CheckTableMatchesTestData(table);
            }
        }

        [Test]
        public void NestedTables()
        {
            var testTableByteArray = GetResolvedTestData();
            var nestTable = new WriteTable<byte[]>(new ByteArrayConverter());
            nestTable.Add("table1", testTableByteArray);
            nestTable.Add("table2", testTableByteArray);
            var data = nestTable.Resolve();
            using (var readOnlyByteArray = new ReadOnlyByteArray(data))
            {
                var table = new ReadTable<byte[]>(new ByteArrayConverter(), readOnlyByteArray);

                byte[] readTableBytes;
                Assert.IsTrue(table.TryGetValue("table1", out readTableBytes));
                Assert.IsTrue(readTableBytes.Where((b, i) => b != testTableByteArray[i]).Count() == 0);
                Assert.IsTrue(table.TryGetValue("table2", out readTableBytes));
                Assert.IsTrue(readTableBytes.Where((b, i) => b != testTableByteArray[i]).Count() == 0);
            }
        }

        private static byte[] GetResolvedTestData()
        {
            var table = new WriteTable<TestData>(new DataConverter<TestData>());
            foreach (var entry in TestData)
                table.Add(entry.Key, entry.Data);

            return table.Resolve();
        }

        [Test]
        public void MappedBasicWorkFlow()
        {
            var data = GetResolvedTestData();
            File.WriteAllBytes("test.data", data);
            try
            {
                using (var mappedData = new Mapper("test.data"))
                {
                    var table = new ReadTable<TestData>(new DataConverter<TestData>(), mappedData);
                    CheckTableMatchesTestData(table);
                }
            }
            finally
            {
                File.Delete("test.data");
            }
        }

        private static void CheckTableMatchesTestData(ReadTable<TestData> table)
        {
            foreach (var entry in TestData)
            {
                TestData value;
                Assert.IsTrue(table.TryGetValue(entry.Key, out value));
                Assert.AreEqual(value, entry.Data);
            }
        }
    }

    public struct TestDataEntry
    {
        public readonly string Key;
        public readonly TestData Data;

        public TestDataEntry(string key, TestData data)
        {
            Key = key;
            Data = data;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct TestData
    {
        public UInt64 Id;
        public string Name;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct TestNestedData
    {
        public byte[] Table;
    }
}