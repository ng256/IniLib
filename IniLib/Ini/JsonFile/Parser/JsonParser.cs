/***************************************************************

•   File: JsonParser.cs

•   Description

    The JsonParser  class is  an   abstract class  that provides
    methods  for parsing JSON data. It  offers  functionality to
    navigate through JSON objects   and arrays, and  retrieve or
    modify  specific  values  based on    a path. This  class is
    designed  to be  extended for  further   customization, with
    methods for retrieving  values, setting values, and managing
    the structure of JSON data.


    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    /// Abstract class for parsing JSON data, providing methods to serialize,
    /// deserialize, and manipulate JSON structures.
    /// </summary>
    public abstract class JsonParser : TextFileParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParser"/> class using the provided settings.
        /// </summary>
        /// <param name="content">The JSON content to initialize the parser with.</param>
        /// <param name="settings">The settings that configure the behavior of the parser.</param>
        protected JsonParser(string content, JsonFileSettings settings) : base(content, settings)
        {
            if (settings == null) settings = JsonFileSettings.InternalDefaultSettings;
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

            return new JsonParserCached(content, settings);
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
        /// <returns>The value, or the default value if not found.</returns>
        public abstract object GetValue(string path, object defaultValue);

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
        /// Converts the JSON object to a string representation.
        /// </summary>
        /// <returns>The string representation of the JSON content.</returns>
        public override string ToString()
        {
            return Content;
        }
    }
}