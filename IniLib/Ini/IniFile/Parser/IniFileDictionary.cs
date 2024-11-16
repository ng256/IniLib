/***************************************************************

•   File: IniFileDictionary.cs

•   Description

    IniFileDictionary is  a class that  is designed to work with
    data in the INI file format. It allows you to read and write
    data from a file or stream, and provides methods for getting
    values ​​by key and section.

    IniFileDictionary converts the contents of an ini  file into
    a data structure that is a  dictionary of sections and their
    parameters.  This allows you to easily  manipulate  the data
    and access values ​​by keys.

    Loses original formatting when    saving:  When  writing the
    contents  of the dictionary  back to a file  or  stream, the
    class  does  not   preserve   the original  file formatting.
    Instead,  it writes data  in  a  standard INI   file format.

    The IniFileDictionary class can be useful  for various tasks
    related  to handling INI  data, such as  reading application
    settings, saving  system  configuration, or  exchanging data
    between applications.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using static System.InternalTools;

namespace System.Ini
{
    internal sealed class IniFileDictionary : IniFileParser
    {
        private string _content = null;

        private readonly SortedDictionary<string, NameValueCollection> _sections = new SortedDictionary<string, NameValueCollection>();
        private readonly Regex _regex;
        private readonly bool _allowEscapeChars;
        private readonly bool _addMissingEntries;
        private readonly char _delimiter = '=';
        private readonly string _lineBreaker = Environment.NewLine;
        private readonly StringComparer _comparer;

        // Gets or sets the contents of the ini file as a string.
        public override string Content
        {
            /*Generates a string representation of the INI file content.
             It iterates through the dictionary of sections and their parameters, and builds a string with the appropriate 
            formatting (section names, key-value pairs, and blank lines between sections).*/
            get
            {
                if (_content != null) return _content;

                StringBuilder sb = new StringBuilder();

                // Iterate through the dictionary of sections and parameters.
                foreach (KeyValuePair<string, NameValueCollection> pair in _sections)
                {
                    string section = pair.Key;
                    NameValueCollection entries = pair.Value;
                    int entriesCount = entries.Count;

                    // Add the section name, if it's not null or empty and there are entries.
                    if (!section.IsNullOrEmpty() && entriesCount > 0)
                        sb.AppendLine($"[{section}]", _lineBreaker);

                    // Add all the entries in the current section.
                    for (int entryIndex = 0; entryIndex < entriesCount; entryIndex++)
                    {
                        string key = entries.GetKey(entryIndex);
                        if (entries.GetValues(entryIndex) is string[] values)
                        {
                            // Handle multiple values for the same key.
                            for (int index = 0; index < values.Length; index++)
                            {
                                string value = values[index];

                                // Escape the value if necessary.
                                if (_allowEscapeChars)
                                    value = value.ToEscape();

                                sb.AppendLine($"{key}{_delimiter}{value}", _lineBreaker);
                            }
                        }
                    }
                    sb.Append(_lineBreaker); // Add a blank line between sections.
                }

                return _content = sb.ToString(); // Return the final string representation of the ini file content.
            }

            set // Parses the content and converts it into a data structure that is a dictionary of sections and their parameters.
            {
                // Clear the existing sections and entries.
                if (_sections.Count > 0)
                    foreach (KeyValuePair<string, NameValueCollection> pair in _sections) 
                        pair.Value.Clear();

                // There is no need to parse an empty string.
                if (value.IsNullOrEmpty())
                    return;

                StringComparison comparison = Comparison;

                // Get the entries for the empty section (global entries).
                NameValueCollection entries = GetEntries(string.Empty);

                // Use a regular expression to parse the content.
                for (Match match = _regex.Match(value ?? string.Empty); match.Success; match = match.NextMatch())
                {
                    if (match.Groups["section"].Success)
                    {
                        // Found a section, get the section name and set the entries collection.
                        string section = match.Groups["value"].Value.MayBeToLower(comparison);
                        entries = GetEntries(section);
                        continue;
                    }

                    if (match.Groups["entry"].Success)
                    {
                        // Found an entry, get the key and value.
                        string key = match.Groups["key"].Value.MayBeToLower(comparison);
                        string val = match.Groups["value"].Value;

                        // Unescape the value if necessary.
                        if (_allowEscapeChars)
                            val = val.UnEscape(CustomEscapeCharacters);

                        // Add the entry to the current entries collection.
                        entries.Add(key, val);
                    }
                }
            }
        }

        // Initializes an instance of the class using the specified settings,
        // which include string comparison options, escape character handling,
        // adding missing entries, and other options.
        public IniFileDictionary(string content, IniFileSettings settings) : base(settings)
        {
            if (settings == null)
                settings = IniFileSettings.InternalDefaultSettings;

            _comparer = settings.Comparer;
            _sections = new SortedDictionary<string, NameValueCollection>(_sections, _comparer);
            _allowEscapeChars = settings.AllowEscapeCharacters;
            _addMissingEntries = settings.AddMissingEntries;
            _delimiter = settings.EntrySeparatorCharacter == IniFileEntrySeparatorCharacter.Colon ? ':' : '=';
            _regex = new Regex(settings.GetRegexPattern(), settings.RegexOptions);
            _lineBreaker = settings.LineBreaker == LineBreakerStyle.Auto
                ? content.AutoDetectLineBreaker().GetString()
                : settings.LineBreaker.GetString();
            Content = content;
        }

        // Returns a NameValueCollection for the specified section.
        private NameValueCollection GetEntries(string section)
        {
            if (section == null) section = string.Empty;

            // If the section does not exist, it is created and an empty collection is returned.
            if (!_sections.TryGetValue(section, out NameValueCollection entries))
                _sections.Add(section, entries = new NameValueCollection(Comparison.GetComparer()));

            return entries;
        }

        // Returns a list of all sections contained in the dictionary.
        public override IEnumerable<string> GetSections()
        {
            return _sections.Keys;
        }

        // Returns a list of keys for the given section.
        public override IEnumerable<string> GetKeys(string section)
        {
            return GetEntries(section)?.AllKeys ?? new string[0];
        }

        // Returns the value of the parameter for the given key and section.
        // If the parameter is not found, the default value is returned.
        public override string GetValue(string section, string key, string defaultValue = null)
        {
            string value = defaultValue;

            if (GetEntries(section) is NameValueCollection entries)
            {
                // If the parameter collection exists, then the presence of a parameter with the specified key is checked.
                if (entries.GetValues(key) is string[] values && values.Length > 0)
                {
                    value = values[0];
                    //if (_allowEscapeChars) value = value.UnEscape(CustomEscapeCharacters);
                    return value;
                }

                // Adds a parameter to a collection if a default value is specified.
                if (_addMissingEntries && !value.IsNullOrEmpty())
                    entries.Add(key, value);
            }

            return value;
        }

        // Returns all values ​​for a given section.
        // If the section does not exist, an empty list is returned.
        public override IEnumerable<string> GetValues(string section)
        {
            List<string> values = new List<string>();

            // If a section with the specified name section exists.
            if (GetEntries(section) is NameValueCollection entries)
            {
                for (int valueIndex = 0; valueIndex < entries.Count; valueIndex++)
                {
                    string value = entries[valueIndex];
                    //if (_allowEscapeChars) value = value.UnEscape(CustomEscapeCharacters);
                    values.Add(value);
                }
            }

            return values;
        }

        // Returns all values ​​for a given section and key.
        // If the section does not exist, an empty list is returned.
        public override IEnumerable<string> GetValues(string section, string key)
        {
            // If the key is empty, return all the values in the section.
            if (key.IsNullOrEmpty()) return GetValues(section);

            // If a section with the specified name section exists.
            if (GetEntries(section) is NameValueCollection entries)
            {
                // If section contains values with given key.
                if (entries.GetValues(key) is string[] values && values.Length > 0)
                {
                    // If escape characters are allowed, they are processed.
                    // if (_allowEscapeChars)
                    // {
                    //     for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
                    //     {
                    //         string value = values[valueIndex];
                    //         values[valueIndex] = value.UnEscape(CustomEscapeCharacters);
                    //     }
                    // }

                    return values;
                }
            }

            return new string[0];
        }

        // Sets the value of the parameter for the given section and key.
        // If value is null remove the key.
        public override void SetValue(string section, string key, string value)
        {
            _content = null;

            if (GetEntries(section) is NameValueCollection entries)
            {
                if (value.IsNullOrEmpty())
                    entries.Remove(key);
                else
                {
                    // Escape the value if necessary.
                    if (_allowEscapeChars)
                        value = value.ToEscape();

                    entries.Set(key, value);
                }
            }
        }

        // Sets parameter values ​​for the given section by the given keys.
        // Values ​​can be passed as an array of strings or as individual arguments.
        public override void SetValues(string section, string key, params string[] values)
        {
            _content = null;

            if (GetEntries(section) is NameValueCollection entries)
            {
                if (values.Length == 0)
                    entries.Remove(key);
                else if (values.Length == 1)
                    entries.Set(key, values[0]);
                else
                    for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
                    {
                        string value = values[valueIndex];

                        if (_allowEscapeChars)
                            value = value.ToEscape();

                        entries.Add(key, value);
                    }
            }
        }

        // Returns a string representation of the contents of the ini file.
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
            _sections.Clear();
        }
    }
}