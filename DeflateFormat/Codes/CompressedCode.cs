namespace DeflateFormat.Codes
{
    internal class CompressedCode : Code
    {
        public int Length;
        public int Distance;

        public CompressedCode (int code, int extraLength, int distance, int extraDistance)
        {
            Length = code - 257 + 3 + extraLength;
            for (int i = 0; i < code - 257; i++) Length += (1 << _lengthExtraBits[i]) - 1;

            Distance = distance + extraDistance + 1;
            for (int i = 0; i < distance; i++) Distance += (1 << _distanceExtraBits[i]) - 1;
        }

        public static int GetExtraBitsForLength(int code) => _lengthExtraBits[code - 257];
        public static int GetExtraBitsForDistance(int code) => _distanceExtraBits[code];

        private static int[] _lengthExtraBits =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
            1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
            4, 4, 4, 4, 5, 5, 5, 5, 0
        };
        private static int[] _distanceExtraBits =
        {
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
            4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
            9, 9, 10, 10, 11, 11, 12, 12, 13, 13
        };
    }
}
