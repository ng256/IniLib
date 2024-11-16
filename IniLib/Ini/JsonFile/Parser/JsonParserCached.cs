/***************************************************************

•   File: JsonParserCached.cs

•   Description

    The JsonParserCached class extends the JsonParser class to provide 
    cached deserialization of JSON content. This means that the JSON
    content is parsed only once, and subsequent access to values is faster.
    The class provides methods for getting and setting values based on a 
    path, along with handling of both single and multiple values.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Ini
{
    /// <summary>
    /// This class extends JsonParser and adds caching capabilities to improve
    /// performance by parsing the JSON content only once.
    /// </summary>
    [DebuggerDisplay("{Content}")]
    public class JsonParserCached : JsonParser
    {
        private object _root;  // Cached root object representing the deserialized JSON content

        // Indicates whether escape characters are allowed.
        private readonly bool _allowEscapeChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParserCached"/> class with the specified content and settings.
        /// </summary>
        /// <param name="content">The JSON content to initialize the parser with.</param>
        /// <param name="settings">The settings that configure the behavior of the parser.</param>
        public JsonParserCached(string content, JsonFileSettings settings = null)
            : base(content, settings)
        {
            if (settings == null)
                settings = JsonFileSettings.InternalDefaultSettings;

            Content = content;  // Ensure valid JSON content

            _allowEscapeChars = settings.AllowEscapeCharacters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParserCached"/> class with default settings.
        /// </summary>
        public JsonParserCached() : this("{}", JsonFileSettings.InternalDefaultSettings)
        {
        }

        /// <summary>
        /// Gets or sets the JSON content. When setting the content, the JSON is deserialized
        /// and stored in the cached root object. When getting the content, it is serialized.
        /// </summary>
        public sealed override string Content
        {
            // Serialize the cached root object back into a JSON string.
            get => SerializeJson(_root); 

            // Deserialize the JSON string and cache the resulting object.
            set => _root = DeserializeJson(value.IsNullOrWhiteSpace() ? "{}" : value); 
        }

        /// <summary>
        /// Gets the value at the specified path in the JSON content.
        /// </summary>
        /// <param name="path">The path to the value in the JSON structure.</param>
        /// <param name="defaultValue">The default value to return if the path is not found.</param>
        /// <returns>The value as a string, or the default value if not found.</returns>
        public override string GetValue(string path, string defaultValue)
        {
            try
            {
                var keys = path.Split('.');  // Split the path into individual keys
                var value = GetValueRecursive(_root, keys, 0);  // Get the value recursively based on the keys


                // Return the value or default if not found.
                string result = value?.ToString() ?? defaultValue;

                if (_allowEscapeChars && result != null)
                    result = result.UnEscape();

                return result;  
            }
            catch
            {
                return defaultValue;  // In case of an error, return the default value
            }
        }

        /// <summary>
        /// Gets an array of values at the specified path in the JSON content.
        /// </summary>
        /// <param name="path">The path to the values in the JSON structure.</param>
        /// <returns>An array of values as strings, or an empty array if not found.</returns>
        public override string[] GetValues(string path)
        {
            try
            {
                var keys = path.Split('.');  // Split the path into individual keys
                var value = GetValueRecursive(_root, keys, 0);  // Get the value recursively

                if (value is List<object> list)
                {
                    var result = new List<string>();
                    foreach (var item in list)
                    {
                        string strItem = SerializeJson(item);

                        if (_allowEscapeChars && strItem != null)
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

        /// <summary>
        /// Sets the value at the specified path in the JSON content.
        /// </summary>
        /// <param name="path">The path to the value in the JSON structure.</param>
        /// <param name="value">The value to set at the specified path.</param>
        public override void SetValue(string path, string value)
        {
            try
            {
                if (_allowEscapeChars && value != null)
                    value = value.ToEscape();

                var keys = path.Split('.');  // Split the path into individual keys
                SetValueRecursive(_root, keys, 0, value);  // Set the value recursively based on the keys
            }
            catch
            {
                // Quietly fail in case of an error
            }
        }

        /// <summary>
        /// Sets multiple values at the specified path in the JSON content.
        /// </summary>
        /// <param name="path">The path to the values in the JSON structure.</param>
        /// <param name="values">The values to set at the specified path.</param>
        public override void SetValues(string path, params string[] values)
        {
            try
            {
                values = (string[]) values.Clone();

                for (int i = 0; i < values.Length; i++)
                {
                    if (_allowEscapeChars && values[i] != null)
                        values[i] = values[i].ToEscape();
                }

                var keys = path.Split('.');  // Split the path into individual keys
                SetValuesRecursive(_root, keys, 0, values);  // Set the values recursively based on the keys
            }
            catch
            {
                // Quietly fail in case of an error
            }
        }
    }
}
