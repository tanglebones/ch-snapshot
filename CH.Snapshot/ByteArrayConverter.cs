namespace CH.Snapshot
{
    public class ByteArrayConverter : IConverter<byte[]>
    {
        public byte[] AsByteArray(byte[] value)
        {
            return value;
        }

        public byte[] AsT(byte[] byteArray)
        {
            return byteArray;
        }

        public byte[] Empty()
        {
            return new byte[0];
        }
    }
}