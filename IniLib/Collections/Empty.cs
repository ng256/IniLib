/***************************************************************

•   File: Empty.cs

•   Description

    Represents an empty collections.

***************************************************************/

namespace System.Collections.Generic
{
    internal static class Empty<T>
    {
        internal static readonly T[] Array = new T[0];
        internal static readonly IEnumerable<T> Enumerable = Array;
    }
}
