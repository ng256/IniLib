/***************************************************************

•   File: JsonParser.cs

•   Description

    The JsonParser class is an abstract class that provides
    methods for parsing JSON data. It offers functionality to
    serialize and deserialize JSON, navigate through JSON objects
    and arrays, and retrieve or modify specific values based on a
    path. This class is designed to be extended for further
    customization, with methods for retrieving values, setting
    values, and managing the structure of JSON data.

    The class utilizes regular expressions for tokenizing JSON input
    and provides mechanisms for pretty-printing JSON output.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Ini
{
    /// <summary>
    /// Abstract class for parsing JSON data, providing methods to serialize,
    /// deserialize, and manipulate JSON structures.
    /// </summary>
    public abstract class JsonParser
    {
        // Comparison method used when searching for keys in JSON objects.
        private readonly StringComparison _comparison;

        // Flag indicating whether the output should be pretty-printed.
        private readonly bool _prettyOutput;

        // The line breaker used to separate lines in the JSON file.
        private readonly string _lineBreaker;

        // Regular expression used to tokenize the JSON input.
        private readonly Regex _tokenRegex;

        private readonly StringComparer _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParser"/> class using the provided settings.
        /// </summary>
        /// <param name="content">The JSON content to initialize the parser with.</param>
        /// <param name="settings">The settings that configure the behavior of the parser.</param>
        protected JsonParser(string content, JsonFileSettings settings)
        {
            if (settings == null) settings = JsonFileSettings.InternalDefaultSettings;

            StringComparison comparison = settings.StringComparison;
            _comparison = comparison;
            _prettyOutput = settings.PrettyOutput;
            _tokenRegex = new Regex(
                @"(?<value>(?<bool>true)|(?<bool>false)|(?<null>null)|""(?<string>[^""\\]*(?:\\.[^""\\]*)*)""|(?<number>-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?))|(?<value_sep>:)|(?<array_open>\[)|(?<array_sep>,)|(?<array_close>\])|(?<object_open>{)|(?<object_close>})|(?<whitespace>\s)",
                comparison.GetRegexOptions(RegexOptions.Compiled));
            _lineBreaker = settings.LineBreaker == LineBreakerStyle.Auto
                ? content.AutoDetectLineBreakerEx().GetString()
                : settings.LineBreaker.GetString();
            _comparer = comparison.GetComparer();
        }

        /// <summary>
        /// Creates a JsonParser instance based on the provided content and settings.
        /// </summary>
        /// <param name="content">The JSON content to initialize the parser with.</param>
        /// <param name="settings">The settings that configure the behavior of the parser.</param>
        /// <returns>A JsonParser instance (either JsonParserQuickScan or JsonParserCached).</returns>
        public static JsonParser Create(string content, JsonFileSettings settings = null)
        {
            if(settings == null) settings = JsonFileSettings.InternalDefaultSettings;

            return settings.Cached
                ? (JsonParser)new JsonParserCached(content, settings)
                : new JsonParserQuickScan(content, settings);
        }

        /// <summary>
        /// Gets or sets the JSON content.
        /// </summary>
        public abstract string Content { get; set; }

        /// <summary>
        /// Retrieves the value from the JSON content based on the provided path.
        /// </summary>
        /// <param name="path">The path to the value in the JSON structure.</param>
        /// <param name="defaultValue">The default value to return if the path is not found.</param>
        /// <returns>The value as a string, or the default value if not found.</returns>
        public abstract string GetValue(string path, string defaultValue);

        /// <summary>
        /// Retrieves an array of values from the JSON content based on the provided path.
        /// </summary>
        /// <param name="path">The path to the values in the JSON structure.</param>
        /// <returns>An array of values as strings, or an empty array if not found.</returns>
        public abstract string[] GetValues(string path);

        /// <summary>
        /// Sets the value in the JSON content at the specified path.
        /// </summary>
        /// <param name="path">The path to the value to set.</param>
        /// <param name="value">The value to set.</param>
        public abstract void SetValue(string path, string value);

        /// <summary>
        /// Sets multiple values in the JSON content at the specified path.
        /// </summary>
        /// <param name="path">The path to the values to set.</param>
        /// <param name="values">The values to set.</param>
        public abstract void SetValues(string path, params string[] values);

        /// <summary>
        /// Deserializes a JSON string into an object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        protected object DeserializeJson(string json)
        {
            return ParseJson(json);
        }

        /// <summary>
        /// Serializes an object into a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="indentLevel">The indentation level for pretty-printing.</param>
        /// <returns>The serialized JSON string.</returns>
        protected string SerializeJson(object obj, int indentLevel = 0)
        {
            string indent = new string(' ', indentLevel * 2);  // 2 spaces per indent level

            if (obj is Dictionary<string, object> dictionary)
            {
                var items = new List<string>();
                foreach (var kvp in dictionary)
                {
                    items.Add($"\"{kvp.Key}\": {SerializeJson(kvp.Value, indentLevel + 1)}");
                }
                return _prettyOutput 
                    ? $"{{{_lineBreaker}{indent}  {string.Join($",{_lineBreaker}" + indent + "  ", items)}{_lineBreaker}{indent}}}" 
                    : $"{{{string.Join(",", items)}}}";
            }

            if (obj is List<object> list)
            {
                var items = new List<string>();
                foreach (var item in list)
                {
                    items.Add(SerializeJson(item, indentLevel + 1));
                }
                return _prettyOutput 
                    ? $"[{_lineBreaker}{indent}  {string.Join($",{_lineBreaker}" + indent + "  ", items)}{_lineBreaker}{indent}]" 
                    : $"[{string.Join(",", items)}]";
            }

            if (obj is string str) return $"\"{str}\"";
            if (obj is bool b) return b ? "true" : "false";
            if (obj is null) return "null";

            return obj.ToString();
        }

        /// <summary>
        /// Recursively retrieves the value from the JSON structure at the specified path.
        /// </summary>
        /// <param name="current">The current JSON object or array.</param>
        /// <param name="keys">The array of keys representing the path.</param>
        /// <param name="index">The current index in the path.</param>
        /// <returns>The value at the specified path, or null if not found.</returns>
        protected object GetValueRecursive(object current, string[] keys, int index)
        {
            if (index >= keys.Length) return current;

            if (current is Dictionary<string, object> obj)
            {
                var key = keys[index];
                if(obj.TryGetValue(key, out var value)) 
                    return GetValueRecursive(value, keys, index + 1);
            }

            return null; // Return null if path doesn't exist
        }

        /// <summary>
        /// Recursively sets the value in the JSON structure at the specified path.
        /// </summary>
        /// <param name="current">The current JSON object or array.</param>
        /// <param name="keys">The array of keys representing the path.</param>
        /// <param name="index">The current index in the path.</param>
        /// <param name="value">The value to set.</param>
        protected void SetValueRecursive(object current, string[] keys, int index, string value)
        {
            if (index >= keys.Length) return;

            if (current is Dictionary<string, object> obj)
            {
                var key = keys[index];
                if (index == keys.Length - 1)
                {
                    obj[key] = ParseJson(value);
                }
                else
                {
                    if (!obj.ContainsKey(key) || !(obj[key] is Dictionary<string, object>))
                    {
                        obj[key] = new Dictionary<string, object>(_comparer);
                    }
                    SetValueRecursive(obj[key], keys, index + 1, value);
                }
            }
        }

        /// <summary>
        /// Recursively sets multiple values in the JSON structure at the specified path.
        /// </summary>
        /// <param name="current">The current JSON object or array.</param>
        /// <param name="keys">The array of keys representing the path.</param>
        /// <param name="index">The current index in the path.</param>
        /// <param name="values">The values to set.</param>
        protected void SetValuesRecursive(object current, string[] keys, int index, string[] values)
        {
            if (index >= keys.Length) return;

            if (current is Dictionary<string, object> obj)
            {
                var key = keys[index];
                if (index == keys.Length - 1)
                {
                    var array = new List<object>();
                    foreach (var value in values)
                    {
                        array.Add(ParseJson(value));
                    }
                    obj[key] = array;
                }
                else
                {
                    if (!obj.ContainsKey(key) || !(obj[key] is Dictionary<string, object>))
                    {
                        obj[key] = new Dictionary<string, object>(_comparer);
                    }
                    SetValuesRecursive(obj[key], keys, index + 1, values);
                }
            }
        }

        /// <summary>
        /// Parses a JSON string into an object.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>The parsed object.</returns>
        protected object ParseJson(string json)
        {
            var tokens = new Queue<Match>();
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            var matches = _tokenRegex.Matches(json);
            foreach (Match match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    tokens.Enqueue(match);
                }
            }

            return ParseValue(tokens);
        }

        // Parses a single value from the token queue.
        private object ParseValue(Queue<Match> tokens)
        {
            if (tokens.Count == 0) return null;

            


            var token = tokens.Dequeue();


            if (token.Groups["null"].Success) return null;
            if (token.Groups["object_open"].Success) return ParseObject(tokens);
            if (token.Groups["array_open"].Success) return ParseArray(tokens);
            if (token.Groups["string"].Success) return token.Groups["string"].Value;
            if (token.Groups["bool"].Success)
            {
                if (bool.TryParse(token.Groups["bool"].Value, out var b)) return b;
                return b;
            }

            if (token.Groups["number"].Success)
            {
                string number = token.Groups["number"].Value;
                if (int.TryParse(number, out var i)) return i;
                if (double.TryParse(number, out var d)) return d;
            }

            return null;
        }

        // Parses a JSON object from the token queue.
        private Dictionary<string, object> ParseObject(Queue<Match> tokens)
        {
            var obj = new Dictionary<string, object>(_comparer);

            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                if (token.Groups["object_close"].Success) break;

                if (token.Groups["string"].Success)
                {
                    var key = token.Groups["string"].Value;
                    if (!tokens.Dequeue().Groups["value_sep"].Success) return obj; // Invalid syntax
                    obj[key] = ParseValue(tokens);
                }
            }

            return obj;
        }

        // Parses a JSON array from the token queue.
        private List<object> ParseArray(Queue<Match> tokens)
        {
            var list = new List<object>();

            while (tokens.Count > 0)
            {
                var token = tokens.Peek();
                if (token.Groups["array_close"].Success)
                {
                    tokens.Dequeue();
                    break;
                }

                list.Add(ParseValue(tokens));

                if (tokens.Peek().Groups["array_sep"].Success) tokens.Dequeue();
            }

            return list;
        }

        /// <summary>
        /// Converts the JSON object to a string representation.
        /// </summary>
        /// <returns>The string representation of the JSON content.</returns>
        public override string ToString()
        {
            return Content;
        }
    }
}