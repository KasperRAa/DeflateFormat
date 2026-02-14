namespace DeflateFormat.Codes
{
    internal class CompressedCode : Code
    {
        public int TotalLength { get; private set; }
        public int TotalDistance { get; private set; }

        public int LengthCode { get; private set; }
        public int DistanceCode { get; private set; }

        public int ExtraLength { get; private set; }
        public int ExtraDistance { get; private set; }

        public CompressedCode (int lengthCode, int extraLength, int distanceCode, int extraDistance)
        {
            LengthCode = lengthCode;
            ExtraLength = extraLength;
            TotalLength = lengthCode - 257 + 3 + extraLength;
            for (int i = 0; i < lengthCode - 257; i++) TotalLength += (1 << _lengthExtraBits[i]) - 1;

            DistanceCode = distanceCode;
            ExtraDistance = extraDistance;
            TotalDistance = distanceCode + extraDistance + 1;
            for (int i = 0; i < distanceCode; i++) TotalDistance += (1 << _distanceExtraBits[i]) - 1;
        }

        public CompressedCode(int length, int distance)
        {
            TotalLength = 3;
            LengthCode = 257;
            while (length - TotalLength > (1 << GetExtraBitsForLength(LengthCode)) - 1)
            {
                TotalLength += (1 << GetExtraBitsForLength(LengthCode));
                LengthCode++;
            }
            ExtraLength = length - TotalLength;

            TotalDistance = 1;
            DistanceCode = 0;
            while (distance - TotalDistance > (1 << GetExtraBitsForDistance(DistanceCode)) - 1)
            {
                TotalDistance += (1 << GetExtraBitsForDistance(DistanceCode));
                DistanceCode++;
            }
            ExtraDistance = distance - TotalDistance;
        }

        public static int GetExtraBitsForLength(int code) => _lengthExtraBits[code - 257];
        public int GetExtraBitsForLength() => GetExtraBitsForLength(LengthCode);

        public static int GetExtraBitsForDistance(int code) => _distanceExtraBits[code];
        public int GetExtraBitsForDistance() => GetExtraBitsForDistance(DistanceCode);


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
