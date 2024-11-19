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
    public sealed class JsonFileSettings : TextFileSettings
    {
        internal static JsonFileSettings InternalDefaultSettings = new JsonFileSettings();
        private bool _prettyOutput = true;

        /// <summary>
        ///		JSON file settings that are suitable for most tasks and are used by default.
        /// </summary>
        public static JsonFileSettings DefaultSettings
            => (JsonFileSettings)InternalDefaultSettings.Clone();

        /// <summary>
        ///		Initializes settings using the specified string comparison with invariant culture.
        /// </summary>
        public static JsonFileSettings InvariantCulture
            => new JsonFileSettings(StringComparison.InvariantCulture);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with invariant culture and ignore case.
        /// </summary>
        public static JsonFileSettings InvariantCultureIgnoreCase
            => new JsonFileSettings(StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with current culture.
        /// </summary>
        public static JsonFileSettings CurrentCulture
            => new JsonFileSettings(StringComparison.CurrentCulture);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with current culture and ignore case.
        /// </summary>
        public static JsonFileSettings CurrentCultureIgnoreCase
            => new JsonFileSettings(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        ///     Flag to enable or disable pretty printing (indentation) of JSON output.
        /// </summary>
        public bool PrettyOutput
        {
            get => _prettyOutput;
            set => _prettyOutput = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonFileSettings"/> class.
        /// </summary>
        public JsonFileSettings() : base()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonFileSettings"/> class.
        /// </summary>
        /// <param name="comparison">String comparison specifier.</param>
        public JsonFileSettings(StringComparison comparison) : base(comparison)
        {
        }

        /// <summary>
        ///     Method to clone the current settings (deep copy).
        /// </summary>
        /// <returns>A new <see cref="JsonFileSettings"/> object with the same settings.</returns>
        public override object Clone()
        {
            return new 
            {
                PrettyOutput = this.PrettyOutput,
                LineBreaker = this.LineBreaker,
                AllowEscapeCharacters = this.AllowEscapeCharacters,
                PropertyFilter = this.PropertyFilter,
                Comparison = this.Comparison,
                UseExtendedTypeConverters = this.UseExtendedTypeConverters,
                ReadOnly = this.ReadOnly
            };
        }
    }
}
