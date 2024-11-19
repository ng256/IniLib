/***************************************************************

•   File: YamlFileSettings.cs

•   Description

    The YamlFileSettings class defines the settings for configuring
    the behavior of YAML parsers. These settings can be used to customize
    how YAML is parsed, serialized, and handled during operations like 
    retrieving or updating values.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///     This class defines the settings for the YAML parser.
    ///     It allows customization of comparison modes, pretty output,
    ///     and other parsing-related configurations.
    /// </summary>
    public sealed class YamlFileSettings : TextFileSettings
    {
        internal static YamlFileSettings InternalDefaultSettings = new YamlFileSettings();
        private bool _prettyOutput = true;

        /// <summary>
        ///     Flag to enable or disable pretty printing (indentation) of YAML output.
        /// </summary>
        public bool PrettyOutput
        {
            get => _prettyOutput;
            set => _prettyOutput = value;
        }

        /// <summary>
        ///		YAML file settings that are suitable for most tasks and are used by default.
        /// </summary>
        public static YamlFileSettings DefaultSettings
            => (YamlFileSettings)InternalDefaultSettings.Clone();

        /// <summary>
        ///		Initializes settings using the specified string comparison with invariant culture.
        /// </summary>
        public static YamlFileSettings InvariantCulture
            => new YamlFileSettings(StringComparison.InvariantCulture);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with invariant culture and ignore case.
        /// </summary>
        public static YamlFileSettings InvariantCultureIgnoreCase
            => new YamlFileSettings(StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with current culture.
        /// </summary>
        public static YamlFileSettings CurrentCulture
            => new YamlFileSettings(StringComparison.CurrentCulture);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with current culture and ignore case.
        /// </summary>
        public static YamlFileSettings CurrentCultureIgnoreCase
            => new YamlFileSettings(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlFileSettings"/> class.
        /// </summary>
        public YamlFileSettings() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlFileSettings"/> class.
        /// </summary>
        /// <param name="comparison">String comparison specifier.</param>
        public YamlFileSettings(StringComparison comparison) : base(comparison)
        {
        }

        /// <summary>
        /// Method to clone the current settings (deep copy).
        /// </summary>
        /// <returns>A new <see cref="YamlFileSettings"/> object with the same settings.</returns>
        public override object Clone()
        {
            return new YamlFileSettings
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
