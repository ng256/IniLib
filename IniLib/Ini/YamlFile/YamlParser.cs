/***************************************************************

•   File: YamlParser.cs

•   Description

    The   YamlParser class  provides    functionality to  parse,
    retrieve, and modify  data   from YAML content.   This class
    extends   the TextFileParser  and supports advanced features
    like  handling escape   characters,    pretty   output,  and
    regex-based        token    parsing    for   efficient  YAML
    deserialization  and serialization.

    It  also  provides    methods   for accessing   and updating
    hierarchical YAML structures using paths.


•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Ini
{
    /// <summary>
    ///     Represents a parser for YAML content, supporting advanced 
    ///     features like hierarchical access, token parsing, and value modification.
    /// </summary>
    public class YamlParser : TextFileParser
    {
        private readonly Regex _tokenRegex;
        private object _yamlData;
        private readonly bool _prettyOutput;

        /// <summary>
        ///     Initializes a new instance of the <see cref="YamlParser"/> class.
        /// </summary>
        /// <param name="content">The YAML content to parse.</param>
        /// <param name="settings">The settings used to configure the YAML parser.</param>

        public YamlParser(string content, YamlFileSettings settings) 
            : base(content, settings ?? YamlFileSettings.InternalDefaultSettings)
        {
            if (settings == null)
                settings = YamlFileSettings.InternalDefaultSettings;

            _prettyOutput = settings.PrettyOutput;
            _tokenRegex = new Regex(
                @"(?:#\s*(?<comment>.*?)\s*[\r\n]+)|(?<indent>^[^\S\r\n]+)|(?<value>(?<bool>true)|(?<bool>false)|(?<null>null)|""(?<string>[^""\\]*(?:\\.[^""\\]*)*)""|(?<number>-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?))|(?:(?<key>[^\s:#]+)\s*(?=:))|(?<syntax>(?<value_sep>:)|(?<item>-))|(?<whitespace>[^\S\r\n]+)|(?<newline>[\r\n]+)|(?<unknown>[^-:\r\n]+)",
                settings.RegexOptions);
            Content = content;
        }
        
        /// <summary>
        ///     Gets or sets the YAML content. When setting the content, the YAML is deserialized
        ///     and stored in the cached root object. When getting the content, it is serialized.
        /// </summary>
        public string Content
        {
            // Serialize the cached root object back into a YAML string.
            get => SerializeYaml(_yamlData);

            // Deserialize the YAML string and cache the resulting object.
            set => _yamlData = DeserializeYaml(value.IsNullOrWhiteSpace() ? string.Empty : value);
        }

        /// <summary>
        ///     Retrieves the value from the YAML content based on the provided path.
        /// </summary>
        /// <param name="path">The path to the value in the YAML structure.</param>
        /// <param name="defaultValue">The default value to return if the path is not found.</param>
        /// <returns>The value as a string, or the default value if not found.</returns>
        public string GetValue(string path, string defaultValue)
        {
            var keys = path.Split('.');
            var result = GetValueRecursive(_yamlData, keys, 0);
            return result?.ToString() ?? defaultValue;
        }

        /// <summary>
        ///     Retrieves an array of values from the YAML content based on the provided path.
        /// </summary>
        /// <param name="path">The path to the values in the YAML structure.</param>
        /// <returns>An array of values as strings, or an empty array if not found.</returns>
        public string[] GetValues(string path)
        {
            var keys = path.Split('.');
            var result = GetValueRecursive(_yamlData, keys, 0) as IEnumerable<object>;
            return result?.Select(o => o.ToString()).ToArray() ?? new string[0];
        }

        /// <summary>
        ///     Sets the value in the YAML content at the specified path.
        /// </summary>
        /// <param name="path">The path to the value to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(string path, string value)
        {
            if (ReadOnly) return;
            if (value != null && AllowEscapeChars) 
                value = value.ToEscape();

            var keys = path.Split('.');
            SetValueRecursive(_yamlData, keys, 0, value);
        }

        /// <summary>
        ///     Sets multiple values in the YAML content at the specified path.
        /// </summary>
        /// <param name="path">The path to the values to set.</param>
        /// <param name="values">The values to set.</param>
        public void SetValues(string path, params string[] values)
        {
            if (ReadOnly) return;
            var keys = path.Split('.');
            SetValuesRecursive(_yamlData, keys, 0, values);
        }

        // Deserializes a YAML string into an object.
        private object DeserializeYaml(string yaml)
        {
            var matches = _tokenRegex.Matches(yaml);
            var tokens = new Queue<Match>(matches.Count);
            foreach (Match match in matches)
            {
                tokens.Enqueue(match);
            }
            
            return ParseObject(tokens);
        }

        // Serializes an object into a YAML string recursively.
        private string SerializeYaml(object obj, int indentLevel = 0)
        {
            if (obj is Dictionary<string, object> dict)
            {
                return string.Join(Environment.NewLine,
                    dict.Select(kv => new string(' ', indentLevel) + $"{kv.Key}: {SerializeYaml(kv.Value, indentLevel + 2)}"));
            }
            if (obj is List<object> list)
            {
                return string.Join(Environment.NewLine,
                    list.Select(item => new string(' ', indentLevel) + $"- {SerializeYaml(item, indentLevel + 2)}"));
            }
            return obj?.ToString() ?? "null";
        }

        // Recursively retrieves the value from the YAML structure at the specified path.
        private object GetValueRecursive(object current, string[] keys, int index)
        {
            if (index >= keys.Length) return current;
            if (current is Dictionary<string, object> dict && dict.TryGetValue(keys[index], out var next))
            {
                return GetValueRecursive(next, keys, index + 1);
            }
            return null;
        }

        // Recursively sets the value in the YAML structure at the specified path.
        private void SetValueRecursive(object current, string[] keys, int index, string value)
        {
            if (index == keys.Length - 1)
            {
                if (current is Dictionary<string, object> d1)
                {
                    d1[keys[index]] = value;
                }
                return;
            }
            if (current is Dictionary<string, object> d2)
            {
                if (!d2.TryGetValue(keys[index], out var next) || next is not Dictionary<string, object> )
                {
                    next = new Dictionary<string, object>();
                    d2[keys[index]] = next;
                }
                SetValueRecursive(next, keys, index + 1, value);
            }
        }

        // Recursively sets multiple values in the YAML structure at the specified path.
        private void SetValuesRecursive(object current, string[] keys, int index, string[] values)
        {
            if (index == keys.Length - 1)
            {
                if (current is Dictionary<string, object> d1)
                {
                    d1[keys[index]] = values.ToList();
                }
                return;
            }
            if (current is Dictionary<string, object> d2)
            {
                if (!d2.TryGetValue(keys[index], out var next) || next is not Dictionary<string, object>)
                {
                    next = new Dictionary<string, object>();
                    d2[keys[index]] = next;
                }
                SetValuesRecursive(next, keys, index + 1, values);
            }
        }

        // Parses a single value from the token queue.
        private object ParseValue(Queue<Match> tokens)
        {
            if (!tokens.TryDequeue(out var token)) return null;

            if (token.Groups["bool"].Success) return bool.Parse(token.Value);
            if (token.Groups["null"].Success) return null;
            if (token.Groups["number"].Success) return double.TryParse(token.Value, out var number) ? number : (object)token.Value;
            if (token.Groups["string"].Success) return token.Groups["string"].Value;
            if (token.Groups["key"].Success) return token.Groups["key"].Value;

            return token.Value;
        }

        // Parses a YAML object from the token queue.
        private Dictionary<string, object> ParseObject(Queue<Match> tokens)
        {
            var result = new Dictionary<string, object>(Comparer);

            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                if (token.Groups["key"].Success)
                {
                    var key = token.Groups["key"].Value;
                    if (tokens.Count > 0 && tokens.Peek().Groups["value_sep"].Success)
                    {
                        tokens.Dequeue(); // Skip ':'
                        result[key] = ParseValue(tokens);
                    }
                }
            }

            return result;
        }

        // Parses a YAML list from the token queue.
        private List<object> ParseList(Queue<Match> tokens)
        {
            var result = new List<object>();

            while (tokens.Count > 0 && tokens.Peek().Groups["item"].Success)
            {
                tokens.Dequeue(); // Skip '-'
                result.Add(ParseValue(tokens));
            }

            return result;
        }
    }
}
