using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.Ini
{
    internal class IniFileConcurrentRegexParser : IniFileParser
    {
        private string _content;
        private readonly Regex _regex;
        private readonly StringComparer _comparer;
        private readonly bool _allowEscapeChars;
        private readonly bool _addMissingEntries;
        private readonly char _delimiter = '=';
        private readonly string _lineBreaker = Environment.NewLine;
        private readonly bool _cached = false;
        private IEnumerable<Match> _matches;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);  // Semaphore to block access to object.

        internal IEnumerable<Match> Matches
        {
            get
            {

                var matches = _matches;
                if (matches != null)
                {
                    return matches;
                }

                // Waiting for update...
                _semaphore.Wait();
                try
                {
                    string content = Content;
                    return _matches = _cached
                        ? new MatchCollectionFiltered(_regex.Matches(content), "section", "entry")
                        : (IEnumerable<Match>)new MatchIterator(_regex, content);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public override string Content
        {
            get
            {
                _semaphore.Wait();
                try
                {
                    return _content ?? (_content = string.Empty);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            set
            {
                _semaphore.Wait();
                try
                {
                    _content = value ?? string.Empty;
                    _matches = _matches is MatchCollectionFiltered collection
                        ? collection.Update(_regex, _content, "section", "entry")
                        : (IEnumerable<Match>)new MatchIterator(_regex, _content);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private void Update(object state)
        {
            Content = state as string;
        }

        public IniFileConcurrentRegexParser(string content, IniFileSettings settings) : base(settings)
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

        public override IEnumerable<string> GetSections()
        {
            StringComparison comparison = Comparison;
            HashSet<string> sections = new HashSet<string>(_comparer);
            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    string section = match.Groups["value"].Value.MayBeToLower(comparison);

                    sections.Add(section);
                }
            }
            return sections;
        }

        public override IEnumerable<string> GetKeys(string section)
        {
            StringComparison comparison = Comparison;
            HashSet<string> keys = new HashSet<string>(_comparer);

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
                if (inSection && match.Groups["entry"].Success)
                {
                    string key = match.Groups["key"].Value.MayBeToLower(comparison);
                    keys.Add(key);
                }
            }
            return keys;
        }

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
                if (inSection && match.Groups["entry"].Success)
                {
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;
                    value = match.Groups["value"].Value;
                    if (_allowEscapeChars) value = value.UnEscape();

                    return value;
                }
            }
            if (_addMissingEntries && !value.IsNullOrEmpty())
                SetValue(section, key, value);
            return value;
        }

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
                if (inSection && match.Groups["entry"].Success)
                {
                    string value = match.Groups["value"].Value;
                    if (_allowEscapeChars) value = value.UnEscape();
                    values.Add(value);
                }
            }

            return values;
        }

        public override IEnumerable<string> GetValues(string section, string key)
        {
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
                if (inSection && match.Groups["entry"].Success)
                {
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;

                    string value = match.Groups["value"].Value;
                    if (_allowEscapeChars) value = value.UnEscape();

                    values.Add(value);
                }
            }
            return values;
        }

        public override void SetValue(string section, string key, string value)
        {
            if (ReadOnly) return;

            StringComparison comparison = Comparison;
            bool emptySection = section.IsNullOrEmpty();
            bool expectedValue = !value.IsNullOrEmpty();
            bool inSection = emptySection;
            Match lastMatch = null;
            StringBuilder sb = new StringBuilder(_content);
            if (_allowEscapeChars && expectedValue) value = value.ToEscape();
            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }
                if (inSection && match.Groups["entry"].Success)
                {
                    lastMatch = match;
                    if (!match.Groups["key"].Value.Equals(key, comparison))
                        continue;
                    Group group = match.Groups["value"];
                    int index = group.Index;
                    int length = group.Length;
                    if (expectedValue)
                    {
                        sb.Remove(index, length);
                        sb.Insert(index, value);
                    }
                    else
                    {
                        sb.Remove(match.Index, match.Length);
                    }
                    expectedValue = false;
                    break;
                }
            }
            if (expectedValue)
            {
                int index = 0;
                if (lastMatch != null)
                {
                    index = lastMatch.Index + lastMatch.Length;
                }
                else if (!emptySection)
                {
                    sb.Append(_lineBreaker);
                    sb.Append($"[{section}]{_lineBreaker}");
                    index = sb.Length;
                }
                string line = $"{key}{_delimiter}{value}";
                sb.InsertLine(ref index, _lineBreaker, line);
            }

            // Update Content asynchronously.
            string content = sb.ToString();
            ThreadPool.QueueUserWorkItem(Update, content);
        }

        public override void SetValues(string section, string key, params string[] values)
        {
            if (ReadOnly) return;

            StringComparison comparison = Comparison;
            bool emptySection = section.IsNullOrEmpty();
            bool inSection = emptySection;
            Match lastMatch = null;
            StringBuilder sb = new StringBuilder(_content);
            bool keyMatchFound = false;

            foreach (Match match in Matches)
            {
                if (match.Groups["section"].Success)
                {
                    inSection = match.Groups["value"].Value.Equals(section, comparison);
                    if (emptySection) break;
                    continue;
                }
                if (inSection && match.Groups["entry"].Success)
                {
                    lastMatch = match;
                    if (match.Groups["key"].Value.Equals(key, comparison))
                    {
                        keyMatchFound = true;
                        break;
                    }
                }
            }

            if (keyMatchFound)
            {
                sb.Remove(lastMatch.Index, lastMatch.Length);
                foreach (string value in values)
                {
                    sb.Insert(lastMatch.Index, $"{key}{_delimiter}{value}{_lineBreaker}");
                }
            }
            else
            {
                sb.AppendLine($"[{section}]");
                foreach (string value in values)
                {
                    sb.AppendLine($"{key}{_delimiter}{value}");
                }
            }

            // Update Content asynchronously.
            string content = sb.ToString();
            ThreadPool.QueueUserWorkItem(Update, content);
        }

        public override void Dispose()
        {
            _content = null;
            _matches = null;
        }
    }
}
