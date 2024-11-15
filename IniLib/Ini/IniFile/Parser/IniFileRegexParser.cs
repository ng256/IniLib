using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Ini
{
    internal class IniFileRegexParser : IniFileParser
    {
        // The content of the INI file.
        private string _content;

        // A regular expression used to parse the INI file.
        private readonly Regex _regex;

        // String compare operation for hashed collections.
        private readonly StringComparer _comparer;

        // Indicates whether escape characters are allowed.
        private readonly bool _allowEscapeChars;

        // Indicates whether missing entries should be added.
        private readonly bool _addMissingEntries;

        // The character used to separate the key and value in an entry.
        private readonly char _delimiter = '=';

        // The line breaker used to separate lines in the INI file.
        private readonly string _lineBreaker = Environment.NewLine;

        // A flag indicating whether the matches have been cached.
        private readonly bool _cached = false;

        // The cached matches, if any.
        private IEnumerable<Match> _matches;

        // The matches, either cached or iterated over the content.
        internal IEnumerable<Match> Matches =>
            _matches ?? (_matches = _cached
                ? new MatchCollectionFiltered(_regex.Matches(Content), "section", "entry")
                : (IEnumerable<Match>)new MatchIterator(_regex, Content));

        // Returns the content of the ini file.
        public override string Content
        {
            get => _content ?? (_content = string.Empty);

            set
            {
                _content = value ?? string.Empty;

                // Update the matches based on the new content.
                _matches = _matches is MatchCollectionFiltered collection
                    ? collection.Update(_regex, _content, "section", "entry")
                    : (IEnumerable<Match>)new MatchIterator(_regex, _content);
            }
        }

        // Initializes a new instance of the ConfigurationManager class.
        public IniFileRegexParser(string content, IniFileSettings settings) : base(settings)
        {
            if (settings == null)
                settings = IniFileSettings.DefaultSettings;

            lock (settings)
            {
                _content = content ?? string.Empty;
                _regex = new Regex(settings.GetRegexPattern(), settings.RegexOptions);
                _allowEscapeChars = settings.AllowEscapeCharacters;
                _addMissingEntries = settings.AddMissingEntries;
                _delimiter = settings.EntrySeparatorCharacter == IniFileEntrySeparatorCharacter.Colon ? ':' : '=';
                _lineBreaker = settings.LineBreaker == LineBreakerStyle.Auto
                    ? _content.AutoDetectLineBreakerEx().GetString()
                    : settings.LineBreaker.GetString();
                _cached = settings.ParsingMethod < IniFileParsingMethod.QuickScan;
                _comparer = settings.Comparer;
            }
        }

        // Gets all the sections in the INI file.
        public override IEnumerable<string> GetSections()
        {
            // Determine the string comparison method.
            StringComparison comparison = Comparison;

            // Use a HashSet to store unique section names, using the specified comparison method.
            HashSet<string> sections = new HashSet<string>(_comparer);

            // Iterate through all matches.
            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    // If ignoring case, convert it to lower case.
                    string section = match.Groups["value"].Value.MayBeToLower(comparison);

                    sections.Add(section);
                }
            }

            // Return the collection of section names.
            return sections;
        }

        // Gets all the keys in a specific section.
        public override IEnumerable<string> GetKeys(string section)
        {
            // Determine the string comparison method.
            StringComparison comparison = Comparison;

            // Use a HashSet to store unique keys, using the specified comparison method.
            HashSet<string> keys = new HashSet<string>(_comparer);

            bool emptySection = section.IsNullOrEmpty();
            bool inSection = emptySection;

            // Iterate through all matches.
            foreach (Match match in Matches)
            {
                // Determine if we are in the specified section.
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }

                // If in the specified section and the match is an entry, extract the key.
                if (inSection && match.Groups["entry"].Success)
                {
                    string key = match.Groups["key"].Value.MayBeToLower(comparison);
                    keys.Add(key);
                }
            }

            // Return the collection of keys.
            return keys;
        }

        // Gets the value of a specific key in a specific section.
        public override string GetValue(string section, string key, string defaultValue = null)
        {
            StringComparison comparison = Comparison;
            string value = defaultValue;
            bool emptySection = section.IsNullOrEmpty();
            bool inSection = emptySection;

            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }

                // If in the specified section and the match is an entry, check the key.
                if (inSection && match.Groups["entry"].Success)
                {
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;

                    // Extract the value associated with the key.
                    value = match.Groups["value"].Value;

                    // Unescape the value if escape characters are allowed.
                    if (_allowEscapeChars) value = value.UnEscape();

                    return value;
                }
            }

            // If adding missing entries is allowed and a value is found, set the entry in the content.
            if (_addMissingEntries && !value.IsNullOrEmpty())
                SetValue(section, key, value);

            // Return the final value, which may be the default if no match was found.
            return value;
        }

        // Gets all the values in a specific section.
        public override IEnumerable<string> GetValues(string section)
        {
            StringComparison comparison = Comparison;
            List<string> values = new List<string>();
            bool emptySection = section.IsNullOrEmpty();
            bool inSection = emptySection;

            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }

                // If in the specified section and the match is an entry, extract the value.
                if (inSection && match.Groups["entry"].Success)
                {
                    string value = match.Groups["value"].Value;

                    // Unescape the value if escape characters are allowed.
                    if (_allowEscapeChars) value = value.UnEscape();

                    // Add the value to the list.
                    values.Add(value);
                }
            }

            return values;
        }

        // Gets all the values of a specific key in a specific section.
        public override IEnumerable<string> GetValues(string section, string key)
        {
            // If the key is null or empty, return all values from the section.
            if (key.IsNullOrEmpty()) return GetValues(section);

            StringComparison comparison = Comparison;
            List<string> values = new List<string>();
            bool emptySection = section.IsNullOrEmpty();
            bool inSection = emptySection;

            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }

                // If in the specified section and the match is an entry, check the key.
                if (inSection && match.Groups["entry"].Success)
                {
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;

                    string value = match.Groups["value"].Value;
                    if (_allowEscapeChars) value = value.UnEscape();

                    values.Add(value);
                }
            }

            // Return the list of values.
            return values;
        }

        // Sets the value of a specific key in a specific section.
        public override void SetValue(string section, string key, string value)
        {
            if (ReadOnly) return;

            StringComparison comparison = Comparison;
            bool emptySection = section.IsNullOrEmpty();
            bool expectedValue = !value.IsNullOrEmpty();
            bool inSection = emptySection;

            // Track the last match found.
            Match lastMatch = null;

            // Create a StringBuilder initialized with the current content.
            StringBuilder sb = new StringBuilder(_content);

            // Escape the value if allowed and necessary.
            if (_allowEscapeChars && expectedValue) value = value.ToEscape();

            // Iterate through all matches.
            foreach (Match match in Matches)
            {
                // Determine if we are in the specified section.
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }

                // If in the specified section and the match is an entry, update the value.
                if (inSection && match.Groups["entry"].Success)
                {
                    lastMatch = match;

                    // Continue if the key doesn't match.
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;

                    // Get the group representing the value.
                    Group group = match.Groups["value"];
                    int index = group.Index;
                    int length = group.Length;

                    // If null is passed, the value will just be removed.
                    if (expectedValue)
                    {
                        // Remove the old value.
                        sb.Remove(index, length);

                        // Insert the new value in its place.
                        sb.Insert(index, value);
                    }
                    else
                    {
                        // Remove all entry.
                        sb.Remove(match.Index, match.Length);
                    }

                    // Indicate the value has been set.
                    expectedValue = false;
                    break;
                }
            }

            // If the value is still expected, add it to the content.
            if (expectedValue)
            {
                int index = 0;

                // Determine where to insert the new key-value pair.
                if (lastMatch != null)
                {
                    index = lastMatch.Index + lastMatch.Length;
                }
                else if (!emptySection)
                {
                    // Append a new section header if necessary.
                    sb.Append(_lineBreaker);
                    sb.Append($"[{section}]{_lineBreaker}");
                    index = sb.Length;
                }

                // Construct the line with the key and value.
                string line = $"{key}{_delimiter}{value}";

                // Insert the line into the StringBuilder.
                sb.InsertLine(ref index, _lineBreaker, line);
            }

            // Update the content with the modified StringBuilder.
            Content = sb.ToString();
        }

        // Sets the values of a specific key in a specific section.
        public override void SetValues(string section, string key, params string[] values)
        {
            if (ReadOnly) return;

            StringComparison comparison = Comparison;
            bool emptySection = section.IsNullOrEmpty();
            bool inSection = emptySection;

            // Track the index of the current value being processed.
            int valueIndex = 0;

            // Offset to account for changes in length during replacements.
            int offset = 0;


            // Keep track of the last regex match found.
            Match lastMatch = null;  

            // Create a StringBuilder to modify the ini content.
            StringBuilder sb = new StringBuilder(_content);

            // Iterate through all matches.
            foreach (Match match in Matches)
            {
                // Break if all values have been processed.
                if (valueIndex == values.Length)
                    break;

                // Determine if we are in the specified section.
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }

                // If in the specified section and the match is an entry, update the value.
                if (inSection && match.Groups["entry"].Success)
                {
                    lastMatch = match;

                    // Check if the key matches.
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;

                    // Get the group representing the value.
                    Group group = match.Groups["value"];

                    // Get the new value to insert.
                    string newValue = values[valueIndex++] ?? string.Empty;
                    string oldValue = group.Value;

                    // Calculate the index considering previous modifications.
                    int index = group.Index + offset;
                    int length = group.Length;

                    // Remove the old value and insert the new one.
                    sb = sb.Remove(index, length);
                    if (_allowEscapeChars) newValue = newValue.ToEscape();
                    sb = sb.Insert(index, newValue);

                    // Update the offset for future replacements.
                    offset += newValue.Length - oldValue.Length;
                }
            }

            // If there are remaining values to insert.
            if (valueIndex < values.Length)
            {
                int index = 0;

                // Determine where to insert new key-value pairs.
                if (lastMatch != null)
                {
                    index = lastMatch.Index + lastMatch.Length;
                }
                else if (!emptySection)
                {
                    // Append a new section header if necessary.
                    sb = sb.Append(_lineBreaker);
                    sb = sb.Append($"[{section}]{_lineBreaker}");
                    index = sb.Length;
                }

                // Insert remaining values.
                while (valueIndex < values.Length)
                {
                    // Obtaining the next value.
                    string value = values[valueIndex++];
                    if (_allowEscapeChars) value = value.ToEscape();

                    // Insert the new key-value pair into the content.
                    string line = $"{key}{_delimiter}{value}";
                    sb = sb.InsertLine(ref index, _lineBreaker, line);
                }
            }

            // Update the content with the modified StringBuilder.
            Content = sb.ToString();
        }


        // Returns the content of the INI file as a string.
        public override string ToString()
        {
            return Content;
        }

        /// <summary>
        ///     Releases resources used by the parser.
        /// </summary>
        public override void Dispose()
        {
            _content = null;
            _matches = null;
        }
    }
}