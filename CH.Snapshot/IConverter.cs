namespace CH.Snapshot
{
    public interface IConverter<T>
    {
        byte[] AsByteArray(T value);
        T AsT(byte[] byteArray);
        T Empty();
    }
}