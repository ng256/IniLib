/***************************************************************

•   File: MatchCollectionFiltered.cs

•   Description

    MatchCollectionFiltered  is a  custom  collection class  for
    storing  and managing   regex  Match  objects.   It provides
    functionality to append, update, and filter matches based on
    specified group names.  The class ensures efficient internal
    memory   management and  supports iteration through embedded
    enumerators. It implements the ICollection<Match> interface,
    allowing for collection manipulations like adding, removing,
    and checking for match  existence. Additionally,  it handles
    capacity adjustments dynamically to   accommodate changes in
    the match collection size.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.InternalTools;

namespace System.Text.RegularExpressions
{
    // Represents a serializable collection of regex matches filtered by the specified group names.
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    //[DebuggerTypeProxy(typeof(MatchCollectionFiltered.Enumerator))]
    internal class MatchCollectionFiltered : ICollection<Match>
    {
        // Minimum capacity for the internal storage of matches.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const int MIN_CAPACITY = 16;

        // Array to store Match objects.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Match[] _items = new Match[MIN_CAPACITY];

        // Current size of the collection.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _size = 0;

        #region Constructor

        // Initializes a new instance with minimum capacity.
        public MatchCollectionFiltered()
        {
            EnsureCapacity(MIN_CAPACITY);
        }

        // Initializes a new instance with matches from an enumerable source.
        public MatchCollectionFiltered(IEnumerable<Match> collection, params string[] groups)
        {
            Append(collection, groups);
        }

        // Initializes a new instance with a non-generic enumerable source.
        public MatchCollectionFiltered(IEnumerable collection, params string[] groups)
        {
            Append(collection, groups);
        }

        // Initializes a new instance with a collection of Match objects.
        public MatchCollectionFiltered(ICollection<Match> collection, params string[] groups)
        {
            Append(collection, groups);
        }

        // Initializes a new instance with a non-generic collection.
        public MatchCollectionFiltered(ICollection collection, params string[] groups)
        {
            Append(collection, groups);
        }

        // Initializes a new instance with matches found by a regex in a content string.
        public MatchCollectionFiltered(Regex regex, string content, params string[] groups)
        {
            Append(regex.Matches(content), groups);
        }

        // Initializes a new instance with matches found in a substring starting at a specific index.
        public MatchCollectionFiltered(Regex regex, string content, int startAt, params string[] groups)
        {
            Append(regex.Matches(content, startAt), groups);
        }

        #endregion

        #region ICollection<Match> implementation

        // Ensures the internal array has sufficient capacity to store a given number of elements.
        private void EnsureCapacity(int capacity)
        {
            const uint maxSize = 2146435071U;

            // Set minimum capacity if less than zero.
            if (capacity < 0) capacity = MIN_CAPACITY; 

            // Return if the current capacity is sufficient.
            if (_items.Length >= capacity)
                return;

            // Calculate new size by doubling limited to the maximum array length.
            int newSize = _items.Length == 0 ? MIN_CAPACITY : _items.Length * 2;

            if ((uint)newSize > maxSize)
                newSize = (int)maxSize;

            // Ensure new size meets the required capacity.
            if (newSize < capacity)
                newSize = capacity;

            // Return if no resizing is needed.
            if (newSize == _items.Length)
                return; 

            // Resize the internal array and copy existing elements.
            Match[] destinationArray = new Match[newSize];
            if (_size > 0)
                Array.Copy(_items, 0, destinationArray, 0, _size);
            _items = destinationArray;
        }

        // Returns an enumerator that iterates through the collection.
        public IEnumerator<Match> GetEnumerator()
        {
            return new Enumerator(this);
        }

        // Returns a non-generic enumerator that iterates through the collection.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        // Adds a match to the collection.
        public void Add(Match item)
        {
            if (_size == _items.Length)
                EnsureCapacity(_size + 1); // Ensure capacity before adding new item.
            _items[_size++] = item; // Add the item and increment the size.
        }

        // Clears all matches from the collection.
        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size); // Clear the internal array.
                _size = 0; // Reset the size.
            }
        }

        // Determines whether the collection contains a specific match.
        public bool Contains(Match item)
        {
            return _items.Contains(item);
        }

        // Copies the elements of the collection to an array, starting at a particular array index.
        public void CopyTo(Match[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        // Removes a match from the collection at a specified index.
        private void RemoveAt(int index)
        {
            if (index < --_size) // Shift elements down and update the size.
                Array.Copy(_items, index + 1, _items, index, _size - index); 
            _items[_size] = null; // Clear the last element.
        }

        // Removes all matches from the collection at and after the specified index.
        private void RemoveFrom(int index)
        {
            // Clear the elements from the specified index to the end of the collection.
            Array.Clear(_items, index, _size - index);

            // Update the size of the collection.
            _size = index;
        }

        // Removes a specific match from the collection.
        public bool Remove(Match item)
        {
            int index = Array.IndexOf(_items, item, 0, _size); // Find the index of the item.

            // Return false if not found.
            if (index < 0) 
                return false; 

            RemoveAt(index); // Remove item at the found index.

            return true; // Return true if successfully removed.
        }

        // Removes all matches from the collection at and after the specified match.
        public bool RemoveFrom(Match item)
        {
            int index = Array.IndexOf(_items, item, 0, _size); // Find the index of the item.

            if (index < 0) 
                return false;

            RemoveFrom(index);

            return true;
        }

        // Gets the number of elements contained in the collection.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Count => _size;

        // Gets a value indicating whether the collection is read-only.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsReadOnly => _items.IsReadOnly;

        #endregion

        #region Append

        // Appends matches from an enumerable collection, optionally filtered by group names.
        public MatchCollectionFiltered Append(IEnumerable<Match> collection, params string[] groups)
        {
            // Throw an exception if the collection is null.
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            // If no groups specified, add all matches to a list and copy them to the internal array.
            if (groups == null || groups.Length == 0)
            {
                List<Match> list = new List<Match>();
                foreach (var match in collection) list.Add(match);
                EnsureCapacity(list.Count);
                list.CopyTo(_items);
            }
            else
            {
                // Filter matches by specified groups before adding.
                foreach (Match match in collection)
                {
                    foreach (string group in groups)
                        if (match.Groups[group].Success)
                            Add(match);
                }
            }

            return this;
        }

        // Appends matches from a non-generic enumerable collection.
        public MatchCollectionFiltered Append(IEnumerable collection, params string[] groups)
        {
            return Append(collection.OfType<Match>(), groups);
        }

        // Appends matches from a non-generic collection.
        public MatchCollectionFiltered Append(ICollection collection, params string[] groups)
        {
            EnsureCapacity(collection.Count); // Ensure the internal array can hold the new collection.
            return Append(collection.OfType<Match>(), groups);
        }

        // Appends matches from a generic collection.
        public MatchCollectionFiltered Append(ICollection<Match> collection, params string[] groups)
        {
            EnsureCapacity(collection.Count);
            return Append(collection.OfType<Match>(), groups);
        }

        #endregion

        #region Update

        // Clears the collection and appends new matches from a non-generic enumerable.
        public MatchCollectionFiltered Update(IEnumerable collection, params string[] groups)
        {
            Clear(); // Clear the current collection.
            Append(collection, groups);

            return this;
        }

        // Clears the collection and appends new matches from an enumerable of Match objects.
        public MatchCollectionFiltered Update(IEnumerable<Match> collection, params string[] groups)
        {
            Clear();
            Append(collection, groups);

            return this;
        }

        // Clears the collection, ensures capacity, and appends new matches from a non-generic collection.
        public MatchCollectionFiltered Update(ICollection collection, params string[] groups)
        {
            Clear();
            Append(collection, groups);

            return this;
        }

        // Updates the collection with new matches, ensuring capacity and appending them.
        public MatchCollectionFiltered Update(ICollection<Match> collection, params string[] groups)
        {
            Clear();
            Append(collection, groups);

            return this;
        }

        // Updates the collection with matches from a regular expression.
        public MatchCollectionFiltered Update(Regex regex, string content, params string[] groups)
        {
            Clear();
            Append(regex.Matches(content), groups); // Append new matches from the regex.

            return this;
        }

        // Updates the collection with matches from a regular expression, starting at a specific index.
        public MatchCollectionFiltered Update(Regex regex, string content, int startAt, params string[] groups)
        {
            Clear();
            Append(regex.Matches(content, startAt), groups);

            return this;
        }

        #endregion

        #region Embedded enumerator

        // Enumerator for iterating over the MatchCollectionFiltered.
        private class Enumerator : IEnumerator<Match>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private IEnumerable<Match> Matches => _items.OfType<Match>().ToArray();

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Match[] _items; // The collection of items to enumerate.

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private int _size; // The current match being enumerated.

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private int _index = 0; // The current match being enumerated.

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Match _current;

            // Initializes the enumerator with a MatchCollectionFiltered object.
            public Enumerator(MatchCollectionFiltered collection)
            {
                _items = collection._items;
                _size = collection._size;
            }

            // Disposes of the enumerator.
            public void Dispose()
            {
            }

            // Move position of the enumerator to the next element in the collection.
            public bool MoveNext()
            {
                switch (_index < _size)
                {
                    case true:
                        _current = _items[_index++]; // Set current to the next item.
                        return true; // Successfully moved to the next item.
                    default:
                        return false; // The end of the collection is reached.
                }
            }

            // Resets the enumerator to its initial position.
            public void Reset()
            {
                _index = 0; // Reset index to the beginning.
                _current = null; // Clear the current item.
            }

            // Gets the current element in the collection.
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public Match Current =>
                _current ?? throw new InvalidOperationException(GetResourceString("EnumNotStarted"));

            // Gets the current element in the collection (non-generic).
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object IEnumerator.Current => Current;
        }

        #endregion
    }
}