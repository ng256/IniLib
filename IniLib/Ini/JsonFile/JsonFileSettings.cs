/***************************************************************

•   File: JsonFileSettings.cs

•   Description

    The JsonFileSettings class defines the settings for configuring
    the behavior of JSON parsers. These settings can be used to customize
    how JSON is parsed, serialized, and handled during operations like 
    retrieving or updating values.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///     This class defines the settings for the JSON parser.
    ///     It allows customization of comparison modes, pretty output,
    ///     and other parsing-related configurations.
    /// </summary>
    public class JsonFileSettings : InitializerSettings
    {
        internal static JsonFileSettings InternalDefaultSettings = new JsonFileSettings();
        private LineBreakerStyle _lineBreaker = LineBreakerStyle.Auto;
        private StringComparison _stringComparison = StringComparison.InvariantCultureIgnoreCase;
        private bool _prettyOutput = true;
        private bool _cached = false;

        // The default string comparison method to use
        public StringComparison StringComparison
        {
            get => _stringComparison;
            set => _stringComparison = value;
        }

        /// <summary>
        ///     Flag to enable or disable pretty printing (indentation) of JSON output.
        /// </summary>
        public bool PrettyOutput
        {
            get => _prettyOutput;
            set => _prettyOutput = value;
        }

        /// <summary>
        ///     Flag to allow caching of the parsed JSON data.
        /// </summary>
        public bool Cached
        {
            get => _cached;
            set => _cached = value;
        } // By default, caching is disabled

        /// <summary>
        ///		The newline characters to be used in the JSON file.
        /// </summary>
        public LineBreakerStyle LineBreaker
        {
            get => _lineBreaker;
            set => _lineBreaker = value;
        }

        // Constructor to initialize with default settings.
        public JsonFileSettings() : base()
        {

        }

        /// <summary>
        /// Method to clone the current settings (deep copy).
        /// </summary>
        /// <returns>A new JsonParserSettings object with the same settings.</returns>
        public override object Clone()
        {
            return new JsonFileSettings
            {
                StringComparison = this.StringComparison,
                PrettyOutput = this.PrettyOutput,
                Cached = this.Cached
            };
        }
    }
}
