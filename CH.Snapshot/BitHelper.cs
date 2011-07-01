namespace CH.Snapshot
{
    public static class BitHelper
    {
        public static int GetNextBitCountNeededToRepresent(uint count)
        {
            var pow2 = 0;
            if (count == 0) return 0;
            --count;
            while (count > 0)
            {
                ++pow2;
                count >>= 1;
            }
            return pow2;
        }
    }
}