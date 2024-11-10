/***************************************************************

•   File: Collections.cs

•   Description

    This code fragment is an implementation of extension methods
    for working  with collections and sequences in C#. Extension
    methods  allow you to add    new  methods to  existing types
    without      changing         their       source       code.
    The  methods is implemented for various types of collections
    and  sequences: arrays, ICollection  and IEnumerable.

    IsNullEmpty  - Tests whether  a  collection  or  sequence is
    empty or null.
    ForEach - Performs a specified  action  on each element of a
    collection or sequence. The method takes as an  argument the
    action that should be performed for each element.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    internal static partial class InternalTools
    {
        // Returns the maximum possible length to retrieve elements from an array, starting at a specified position.
        internal static int GetMaxCount<T>(this T[] array, int startIndex, int count)
        {
            return Math.Min(count, array.Length - startIndex);
        }

        // Returns the maximum possible length to retrieve elements from an array, starting at a specified position.
        internal static int GetMaxCount<T>(this T[] array, int startIndex)
        {
            return array.Length - startIndex;
        }

        // An empty buffer.
        internal static readonly string[] EmptyStrings = Empty<string>.Array;
        internal static readonly byte[] EmptyBytes = Empty<byte>.Array;
        internal static readonly object[] EmptyObjects = Empty<object>.Array;

        // Checks if the given array is null or empty.
        internal static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }

        // Checks if the given collection is null or empty.
        internal static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }

        // Checks if the given IEnumerable is null or empty.
        internal static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            return enumerable == null || !enumerable.GetEnumerator().MoveNext();
        }

        // Checks if the given array is null or empty.
        internal static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        // Performs the specified action on each element of the given collection.
        internal static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        // Performs the specified action on each element of the given enumerable.
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        // Performs the specified action on each element of the given array.
        internal static void ForEach(this Array array, Action<object> action)
        {
            if (array.IsNullOrEmpty() || action == null) return;

            for (int i = 0; i < array.Length; ++i)
            {
                action(array.GetValue(i));
            }
        }

        // Performs the specified action on each element of the given collection.
        internal static void ForEach(this ICollection collection, Action<object> action)
        {
            if (collection.IsNullOrEmpty() || action == null) return;

            foreach (object item in collection)
            {
                action.Invoke(item);
            }
        }

        // Performs the specified action on each element of the given list.
        internal static void ForEach(this IList list, Action<object> action)
        {
            if (list.IsNullOrEmpty() || action == null) return;

            for (int i = 0; i < list.Count; ++i)
            {
                action(list[i]);
            }
        }

        // Performs the specified action on each element of the given list.
        internal static void ForEach(this IEnumerable enumerable, Action<object> action)
        {
            if (enumerable == null || action == null) return;

            switch (enumerable)
            {
                case Array array:
                    ForEach(array, action);
                    break;
                case IList list:
                    ForEach(list, action);
                    break;
                case ICollection collection:
                    ForEach(collection, action);
                    break;
                default:
                    foreach (object item in enumerable) action.Invoke(item);
                    break;
            }
        }

        // Performs the specified action on each element of the given array.
        internal static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array.IsNullOrEmpty() || action == null) return;

            for (int i = 0; i < array.Length; i++)
            {
                T item = array[i];
                action.Invoke(item);
            }
        }

        // Performs the specified action on each element of the given list.
        internal static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            if (list.IsNullOrEmpty() || action == null) return;

            for (int i = 0; i < list.Count; ++i)
            {
                action(list[i]);
            }
        }

        // Performs the specified action on each element of the given collection.
        internal static void ForEach<T>(this ICollection<T> collection, Action<T> action)
        {
            if (collection.IsNullOrEmpty() || action == null) return;

            foreach (T item in collection)
            {
                action.Invoke(item);
            }
        }

        // Performs the specified action on each element of the given enumerable.
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null || action == null) return;

            switch (enumerable)
            {
                case T[] array:
                    ForEach(array, action);
                    break;
                case IList<T> list:
                    ForEach(list, action);
                    break;
                case ICollection<T> collection:
                    ForEach(collection, action);
                    break;
                default:
                    foreach (T item in enumerable) action.Invoke(item);
                    break;
            }
        }

        // Checks if the two given arrays are equal.
        internal static bool ArrayEquals<T>(this T[] array1, T[] array2)
        {
            if (array1 == null && array2 == null || array1 == array2)
            {
                return true;
            }

            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].Equals(array2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Checks if the two given arrays are equal.
        internal static bool ArrayEquals(this Array array1, Array array2)
        {
            if (array1 == null && array2 == null || array1 == array2)
            {
                return true;
            }

            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                {
                    return false;
                }
            }

            return true;
        }


    }
}
