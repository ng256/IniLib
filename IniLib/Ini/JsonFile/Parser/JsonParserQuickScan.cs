/***************************************************************

•   File: JsonParserQuickScan.cs

•   Description

    The JsonParserQuickScan class provides a simplified approach to parsing,
    getting, and setting values in JSON content. It uses a direct deserialization
    and serialization process each time data is accessed or modified, and it does
    not cache the parsed object, which allows for quick processing of smaller
    datasets but may be less efficient for larger or frequently accessed JSON data.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Ini
{
    /// <summary>
    /// This class extends JsonParser and performs direct deserialization
    /// and serialization on each access without caching the parsed object.
    /// </summary>
    [DebuggerDisplay("{Content}")]
    public class JsonParserQuickScan : JsonParser
    {
        // The raw JSON string content.
        private string _content;  

        // Indicates whether escape characters are allowed.
        private readonly bool _allowEscapeChars;

        /// <summary>
        /// Initializes a new instance of the JsonParserQuickScan class with the specified content and settings.
        /// </summary>
        /// <param name="content">The JSON content to initialize the parser with.</param>
        /// <param name="settings">The settings that configure the behavior of the parser.</param>
        public JsonParserQuickScan(string content, JsonFileSettings settings = null)
            : base(content, settings)
        {
            if(settings == null) 
                settings = JsonFileSettings.InternalDefaultSettings;

            Content = content;  // Ensure valid JSON content

            _allowEscapeChars = settings.AllowEscapeCharacters;

        }

        /// <summary>
        /// Initializes a new instance of the JsonParserQuickScan class with default content and settings.
        /// </summary>
        public JsonParserQuickScan()
            : this("{}", JsonFileSettings.InternalDefaultSettings)
        {
        }

        /// <summary>
        /// Gets or sets the raw JSON content. When setting the content, the JSON is deserialized,
        /// and when getting the content, it is returned as a string.
        /// </summary>
        public sealed override string Content
        {
            // Return the raw JSON content.
            get => _content;

            // Set the raw JSON content.
            set => _content = value.IsNullOrWhiteSpace() ? "{}" : value;
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
                var root = ParseJson(Content);  // Parse the content into an object.
                var keys = path.Split('.');  // Split the path into individual keys.
                var value = GetValueRecursive(root, keys, 0);  // Get the value recursively based on the keys.

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
                var root = ParseJson(Content);  // Parse the content into an object
                var keys = path.Split('.');  // Split the path into individual keys
                var value = GetValueRecursive(root, keys, 0);  // Get the value recursively

                if (value is List<object> list)
                {
                    var result = new List<string>();
                    foreach (var item in list)
                    {
                        string strItem = SerializeJson(item);

                        if (_allowEscapeChars && strItem != null)
                            strItem = strItem.UnEscape();

                        result.Add(strItem);  // Serialize each item in the list to a string
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
                
                var root = ParseJson(Content);  // Parse the content into an object
                var keys = path.Split('.');  // Split the path into individual keys
                SetValueRecursive(root, keys, 0, value);  // Set the value recursively based on the keys
                Content = SerializeJson(root);  // Serialize the updated object back to JSON and store it
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
                values = (string[])values.Clone();

                for (int i = 0; i < values.Length; i++)
                {
                    if (_allowEscapeChars && values[i] != null)
                        values[i] = values[i].ToEscape();
                }

                var root = ParseJson(Content);  // Parse the content into an object
                var keys = path.Split('.');  // Split the path into individual keys
                SetValuesRecursive(root, keys, 0, values);  // Set the values recursively based on the keys
                Content = SerializeJson(root);  // Serialize the updated object back to JSON and store it
            }
            catch
            {
                // Quietly fail in case of an error
            }
        }
    }
}
