namespace System
{
    internal static partial class InternalTools
    {
        internal static int GetDifference(this int a, int b)
        {
            if (a == b)
                return 0;

            if (a == 0 ^ b == 0)
                return 100;

            a = a < 0 ? ~a + 1 : a;
            b = b < 0 ? ~b + 1 : b;

            int diff = a > b
                ? (a - b) * 100 / a
                : (b - a) * 100 / b;

            return diff;
        }


    }
}