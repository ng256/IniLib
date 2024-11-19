/***************************************************************

•   File: JsonParserCached.cs

•   Description

    The JsonParserCached  class extends  the JsonParser class to
    provide cached deserialization of  JSON content.  This means
    that  the JSON content is  parsed  only once, and subsequent
    access to values is  faster. The class provides  methods for
    getting  and  setting  values  based  on a path,  along with
    handling  of  both  single  and  multiple values.

    Loses original formatting when    saving:  When  writing the
    contents  of the dictionary  back to a file  or  stream, the
    class   does  not     preserve the original file formatting.
    Instead,  it writes data in  a  standard JSON   file format.

    The JsonParserCached  class can be useful  for various tasks
    related  to handling JSON data, such as  reading application
    settings, saving  system  configuration, or  exchanging data
    between applications.

    The  class utilizes  regular expressions for tokenizing JSON
    input  and provides  mechanisms    for pretty-printing  JSON
    output.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System.Ini
{
    // This class extends JsonParser and adds caching capabilities
    // to improve performance by parsing the JSON content only once.
    [DebuggerDisplay("{Content}")]
    internal class JsonParserCached : JsonParser
    {
        // Regular expression used to tokenize the JSON input.
        private readonly Regex _tokenRegex;

        // Cached root object representing the deserialized JSON content.
        private object _jsonData;  

        // Flag indicating whether the output should be pretty-printed.
        private readonly bool _prettyOutput;

        // Initializes a new instance of the JsonParserCached class with the specified content and settings.
        public JsonParserCached(string content, JsonFileSettings settings = null)
            : base(content, settings ?? JsonFileSettings.InternalDefaultSettings)
        {
            if (settings == null)
                settings = JsonFileSettings.InternalDefaultSettings;

            _tokenRegex = new Regex(
                "(?:/\\*(?<comment>.*)\\*/" +
                "|//(?<comment>.*)[\\r\\n]+)" +
                "|(?<value>(?<bool>true)" +
                "|(?<bool>false)" +
                "|(?<null>null)" +
                "|\"(?<string>[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"" +
                "|(?<number>-?(?:0|[1-9][0-9]*)(?:\\.[0-9]+)?(?:[eE][+-]?[0-9]+)?))|(?<syntax>(?<value_sep>:)" +
                "|(?<array_open>\\[)|(?<item_sep>,)|(?<array_close>\\])" +
                "|(?<object_open>{)|(?<object_close>}))" +
                "|(?<whitespace>[^\\S\\r\\n]+)" +
                "|(?<newline>[\\r\\n]+)" +
                "|(?<unknown>[^{}\\[\\],:]+)",
                settings.RegexOptions);
            _prettyOutput = settings.PrettyOutput;
            Content = content; // Ensure valid JSON content
        }

        // Initializes a new instance of the JsonParserCached class with empty content and default settings.
        public JsonParserCached() : this("{}", JsonFileSettings.InternalDefaultSettings)
        {
        }

        // Gets or sets the JSON content. When setting the content, the JSON is deserialized
        // and stored in the cached root object. When getting the content, it is serialized.
        public sealed override string Content
        {
            // Serialize the cached root object back into a JSON string.
            get => SerializeJson(_jsonData); 

            // Deserialize the JSON string and cache the resulting object.
            set => _jsonData = DeserializeJson(value.IsNullOrWhiteSpace() ? "{}" : value); 
        }

        // Gets the value at the specified path in the JSON content.
        public override object GetValue(string path, object defaultValue)
        {
            try
            {
                var keys = path.Split('/', '\\');  // Split the path into individual keys
                var value = GetValueRecursive(_jsonData, keys, 0);  // Get the value recursively based on the keys

                switch (value)
                {
                    case Dictionary<string, object> _:
                    case List<string> _:
                        return null;
                    case string str:
                        if (AllowEscapeChars)
                            str = str.UnEscape();
                        return str;
                    case IConvertible conv:
                        return conv;
                    default:
                        return defaultValue;
                }
            }
            catch
            {
                return defaultValue;  // In case of an error, return the default value
            }
        }

        // Gets an array of values at the specified path in the JSON content.
        public override string[] GetValues(string path)
        {
            try
            {
                var keys = path.Split('/', '\\');  // Split the path into individual keys
                var value = GetValueRecursive(_jsonData, keys, 0);  // Get the value recursively
                var allowEscapeChars = AllowEscapeChars;

                if (value is List<object> list)
                {
                    var result = new List<string>();
                    foreach (var item in list)
                    {
                        string strItem = SerializeJson(item);

                        if (allowEscapeChars && strItem != null)
                            strItem = strItem.UnEscape();

                        result.Add(SerializeJson(strItem));  // Serialize each item in the list to a string
                    }
                    return result.ToArray();  // Return the array of serialized values
                }

                return new string[0];  // Return an empty array if no values found
            }
            catch
            {
                return new string[0];  // In case of an error, return an empty array
            }
        }

        // Sets the value at the specified path in the JSON content.
        public override void SetValue(string path, string value)
        {
            if (ReadOnly) return;

            try
            {
                if (AllowEscapeChars && value != null)
                    value = value.ToEscape();

                var keys = path.Split('/', '\\');  // Split the path into individual keys.
                SetValueRecursive(_jsonData, keys, 0, value);  // Set the value recursively based on the keys.
            }
            catch
            {
                // Quietly fail in case of an error.
            }
        }

        // Sets multiple values at the specified path in the JSON content.
        public override void SetValues(string path, params string[] values)
        {
            if (ReadOnly) return;

            try
            {
                if (AllowEscapeChars)
                {
                    values = (string[])values.Clone();
                    for (int i = 0; i < values.Length; i++)
                        values[i] = values[i]?.ToEscape();
                }

                var keys = path.Split('/', '\\');  // Split the path into individual keys
                SetValuesRecursive(_jsonData, keys, 0, values);  // Set the values recursively based on the keys
            }
            catch
            {
                // Quietly fail in case of an error
            }
        }

        // Deserializes a JSON string into an object.
        private object DeserializeJson(string json)
        {
            return ParseJson(json);
        }

        // Serializes an object into a JSON string.
        private string SerializeJson(object obj, int indentLevel = 0)
        {
            string indent = new string(' ', indentLevel * 2);  // 2 spaces per indent level
            string lineBreaker = LineBreaker;

            if (obj is Dictionary<string, object> dictionary)
            {
                var items = new List<string>();
                foreach (var kvp in dictionary)
                {
                    items.Add($"\"{kvp.Key}\": {SerializeJson(kvp.Value, indentLevel + 1)}");
                }
                return _prettyOutput
                    ? $"{{{lineBreaker}{indent}  {string.Join($",{lineBreaker}" + indent + "  ", items)}{lineBreaker}{indent}}}"
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
                    ? $"[{lineBreaker}{indent}  {string.Join($",{lineBreaker}" + indent + "  ", items)}{lineBreaker}{indent}]"
                    : $"[{string.Join(",", items)}]";
            }

            if (obj is string str) return $"\"{str}\"";
            if (obj is bool b) return b ? "true" : "false";
            if (obj is null) return "null";

            return obj.ToString();
        }

        // Recursively retrieves the value from the JSON structure at the specified path.
        private object GetValueRecursive(object current, string[] keys, int index)
        {
            if (index >= keys.Length) return current;

            if (current is Dictionary<string, object> obj)
            {
                var key = keys[index];
                if (obj.TryGetValue(key, out var value))
                    return GetValueRecursive(value, keys, index + 1);
            }

            return null; // Return null if path doesn't exist
        }

        // Recursively sets the value in the JSON structure at the specified path.
        private void SetValueRecursive(object current, string[] keys, int index, string value)
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
                        obj[key] = new Dictionary<string, object>(Comparer);
                    }
                    SetValueRecursive(obj[key], keys, index + 1, value);
                }
            }
        }

        // Recursively sets multiple values in the JSON structure at the specified path.
        private void SetValuesRecursive(object current, string[] keys, int index, string[] values)
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
                    if (obj.TryGetValue(key, out object o) && !(o is Dictionary<string, object>))
                    {
                        obj[key] = new Dictionary<string, object>(Comparer);
                    }

                    SetValuesRecursive(obj[key], keys, index + 1, values);
                }
            }
        }

        // Parses a JSON string into an object.
        protected object ParseJson(string json)
        {
            var tokens = new Queue<Match>();
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            var matches = _tokenRegex.Matches(json);
            var matchCollection = new MatchCollectionFiltered(matches, "value", "syntax");
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
            if (!tokens.TryDequeue(out var token)) return null;

            if (token.Groups["null"].Success) return null;
            if (token.Groups["object_open"].Success) return ParseObject(tokens);
            if (token.Groups["array_open"].Success) return ParseArray(tokens);
            if (token.Groups["string"].Success) return token.Groups["string"].Value;
            if (token.Groups["bool"].Success) return bool.Parse(token.Groups["bool"].Value);
            if (token.Groups["number"].Success) return token.Groups["number"].Value.ToNumber();

            return null;
        }

        // Parses a JSON object from the token queue.
        private Dictionary<string, object> ParseObject(Queue<Match> tokens)
        {
            var obj = new Dictionary<string, object>(Comparer);

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

                if (tokens.Peek().Groups["item_sep"].Success) tokens.Dequeue();
            }

            return list;
        }
    }
}
