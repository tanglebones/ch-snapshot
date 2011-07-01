namespace CH.Snapshot
{
    public class DataConverter<T> : IConverter<T> where T : new()
    {
        public byte[] AsByteArray(T value)
        {
            return MarshalHelper.ToByteArray(value);
        }

        public T AsT(byte[] byteArray)
        {
            return MarshalHelper.FromByteArray<T>(byteArray);
        }

        public T Empty()
        {
            return new T();
        }
    }
}