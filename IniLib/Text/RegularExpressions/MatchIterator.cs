/***************************************************************

•   File: MatchIterator.cs

•   Description

    The MatchEnumerator  class is   an enumerator  that allows
    iterating  over the matches of  a  regular  expression. It
    implements the IEnumerable   interface, which  provide the
    necessary functionality for enumerating over a matches.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.InternalTools;

namespace System.Text.RegularExpressions
{
    // Provide an enumerator that allows iterating over the matches of a regular expression.
    [Serializable]
    [DebuggerDisplay("{Content}")]
    internal class MatchIterator : IEnumerable<Match>
    {
        // The first match found by the regular expression
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Match _first;

        // Returns the content of the first match.
        public string Content => (string) _first.GetPrivateField("_text");

        #region Constructor

        public MatchIterator(Match match)
        {
            // Ensure that the first match is not null
            _first = match ?? throw new ArgumentNullException(nameof(match));
        }

        public MatchIterator(Regex regex, string content)
        {
            Update(regex, content);
        }

        public MatchIterator(Regex regex, string content, int startAt)
        {
            Update(regex, content, startAt);
        }

        #endregion

        #region Update

        public MatchIterator Update(Match match)
        {
            _first = match ?? throw new ArgumentNullException(nameof(match));

            return this;
        }

        public MatchIterator Update(Regex regex, string content)
        {
            _first = regex?.Match(content ?? string.Empty)
                     ?? throw new ArgumentNullException(nameof(regex));

            return this;
        }

        public MatchIterator Update(Regex regex, string content, int startAt)
        {
            if (startAt < content.Length)
                throw new ArgumentOutOfRangeException(nameof(startAt), startAt,
                    GetResourceString("ArgumentOutOfRange_IndexString"));

            _first = regex?.Match(content ?? string.Empty, startAt)
                     ?? throw new ArgumentNullException(nameof(regex));

            return this;
        }

        #endregion

        #region IEnumerable<Match> implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_first);
        }

        public IEnumerator<Match> GetEnumerator()
        {
            return new Enumerator(_first);
        }

        #endregion

        #region Enumerator

        private class Enumerator : IEnumerator<Match>
        {
            // The current match in the enumeration
            private Match _current;

            // The first match found by the regular expression
            private readonly Match _first;

            // Initializes a new instance of the MatchEnumerator class with the specified first match.
            public Enumerator(Match match)
            {
                _first = match ?? throw new ArgumentNullException(nameof(match));
            }

            // Advances the enumerator to the next element of the collection.
            public bool MoveNext()
            {
                // If the current match is successful, move to the next match
                switch (_current)
                {
                    case Match match when match.Success:
                        _current = match.NextMatch();
                        break;

                    // If the current match is null, set it to the first match
                    case null:
                        _current = _first;
                        break;
                }

                return _current.Success;
            }

            // Sets the enumerator to its initial position, which is before the first element in the collection.
            public void Reset()
            {
                _current = null;
            }

            // Gets the current element in the collection.
            public Match Current => _current ?? throw new InvalidOperationException(GetResourceString("EnumNotStarted"));

            // Gets the current element in the collection.
            object IEnumerator.Current => _current ?? throw new InvalidOperationException(GetResourceString("EnumNotStarted"));

            /// <summary>Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.</summary>
            public void Dispose()
            {
            }
        }

            #endregion

    }
}