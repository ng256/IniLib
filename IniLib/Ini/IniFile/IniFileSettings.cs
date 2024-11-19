/***************************************************************

•   File: IniFileSettings.cs

•   Description

    The IniFileSettings  class  is  a  set  of  parameters  that
    are used to configure the  process of parsing  (analysis) of
    ini files.

    It provides functionality for constructing regular expression 
    to parse a content.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Ini
{
    /// <summary>
    ///		Represent a settings for parsing and formatting the ini file.
    /// </summary>
    [Serializable]
    public sealed class IniFileSettings : TextFileSettings
    {
        private IniFileCommentCharacter _commentCharacter = IniFileCommentCharacter.SemicolonOrHash;
        private IniFileEntrySeparatorCharacter _entrySeparatorCharacter = IniFileEntrySeparatorCharacter.ColonOrEqual;
        private IniFileParsingMethod _parsingMethod = IniFileParsingMethod.PreserveOriginal;
        private bool _allowCommentsInEntries = true;
        private bool _addMissingEntries = false;
        private bool _allowMultiStringValues = false;

        #region Predefined common settings

        internal static IniFileSettings InternalDefaultSettings = new IniFileSettings();

        // Common settings with a fast scan analysis that allows hexadecimal values ​​and case-insensitive comparison.
        // These settings are used to pre-scan the INI file.
        internal static IniFileSettings InternalSettings =
            new IniFileSettings(IniFileParsingMethod.QuickScan, StringComparison.OrdinalIgnoreCase)
            {
                AllowEscapeCharacters = true
            };

        /// <summary>
        ///		INI file settings that are suitable for most tasks and are used by default.
        /// </summary>
        public static IniFileSettings DefaultSettings 
            => (IniFileSettings) InternalDefaultSettings.Clone();

        /// <summary>
        ///		Initializes settings using the specified string comparison with invariant culture.
        /// </summary>
        public static IniFileSettings InvariantCulture 
            => new IniFileSettings(StringComparison.InvariantCulture);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with invariant culture and ignore case.
        /// </summary>
        public static IniFileSettings InvariantCultureIgnoreCase 
            => new IniFileSettings(StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with current culture.
        /// </summary>
        public static IniFileSettings CurrentCulture 
            => new IniFileSettings(StringComparison.CurrentCulture);

        /// <summary>
        ///		Initializes settings using the specified string comparison
        ///		with current culture and ignore case.
        /// </summary>
        public static IniFileSettings CurrentCultureIgnoreCase 
            => new IniFileSettings(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        ///		Initializes settings using the specified parsing method
        ///		for quick scan.
        /// </summary>
        public static IniFileSettings QuickScan 
            => new IniFileSettings(IniFileParsingMethod.QuickScan);

        /// <summary>
        ///		Initializes settings using the specified parsing method
        ///		for reformatting the file.
        /// </summary>
        public static IniFileSettings ReformatFile 
            => new IniFileSettings(IniFileParsingMethod.ReformatFile);

        /// <summary>
        ///		Initializes settings using the specified parsing method
        ///		for preserving the original format.
        /// </summary>
        public static IniFileSettings PreserveOriginal 
            => new IniFileSettings(IniFileParsingMethod.PreserveOriginal);

        #endregion

        #region Properties

        /// <summary>
        ///		Comment symbol in the ini file.
        ///		Possible values ​​are semicolon, hash, or both characters.
        ///		The default is SemicolonAndHash.
        /// </summary>
        public IniFileCommentCharacter CommentCharacter
        {
            get => _commentCharacter;
            set => _commentCharacter = value;
        }

        /// <summary>
        ///		Separator between name and value in the ini file.
        ///		Possible values: colon, equal, or both characters.
        ///		The default is Equal.
        /// </summary>
        public IniFileEntrySeparatorCharacter EntrySeparatorCharacter
        {
            get => _entrySeparatorCharacter;
            set => _entrySeparatorCharacter = value;
        }

        /// <summary>
        ///		Gets or sets the format of the ini file writer.
        ///		The default value is IniFileWriterFormat.ReformatFile.
        /// </summary>
        public IniFileParsingMethod ParsingMethod
        {
            get => _parsingMethod;
            set => _parsingMethod = value;
        }

        /// <summary>
        ///		If true, a comment can be used in the parameter string.
        ///		If false, everything after the delimiter will be treated as the parameter value.
        /// </summary>
        public bool AllowCommentsInEntries
        {
            get => _allowCommentsInEntries;
            set => _allowCommentsInEntries = value;
        }


        /// <summary>
        ///		Add missing entries with default value when reading.
        /// </summary>
        public bool AddMissingEntries
        {
            get => _addMissingEntries;
            set => _addMissingEntries = value;
        }

        /// <summary>
        ///     Allows the use of multi-line values.
        /// </summary>
        public bool AllowMultiStringValues
        {
            get => _allowMultiStringValues;
            set => _allowMultiStringValues = value;
        }

        #endregion

        #region Constructor

        /// <summary>
        ///		Initializes default settings.
        /// </summary>
        public IniFileSettings() { }

        /// <summary>
        ///		Initializes settings using the specified string comparison.
        /// </summary>
        /// <param name="comparison">
        ///		String comparison specifier.
        /// </param>
        public IniFileSettings(StringComparison comparison) : base(comparison)
        {
        }

        /// <summary>
        ///		Initializes settings using the specified parsing method.
        /// </summary>
        /// <param name="parsingMethod">
        ///		Parsing method.
        /// </param>
        public IniFileSettings(IniFileParsingMethod parsingMethod)
        {
            ParsingMethod = parsingMethod;
        }

        /// <summary>
        ///		Initializes settings using the specified parsing method.
        /// </summary>
        ///		
        /// <param name="parsingMethod">
        ///		Parsing method.
        /// </param>
        ///		
        /// <param name="comparison">
        ///		String comparison specifier.
        /// </param>
        public IniFileSettings(IniFileParsingMethod parsingMethod, StringComparison comparison) : base(comparison)
        {
            ParsingMethod = parsingMethod;
        }

        #endregion

        #region Methods

        internal static IniFileSettings LoadFromContent(string content)
        {
            IniFileSettings settings = new IniFileSettings();
            if (content.IsNullOrEmpty()) return settings;
            using (IniFile iniFile = new IniFile(content, InternalSettings))
            {
                // Read various settings from the INI file with default values.
                settings.AddMissingEntries = iniFile.ReadBoolean(null, "add_missing");
                settings.ReadOnly = iniFile.ReadBoolean(null, "read_only");
                settings.AllowEscapeCharacters = iniFile.ReadBoolean(null, "allow_escape");
                settings.AllowCommentsInEntries = iniFile.ReadBoolean(null, "allow_inline_comments", true);
                settings.LineBreaker = iniFile[null, "new_line", @"\r\n"].UnEscape().AutoDetectLineBreaker();

                // Binary data encoding style.
                try
                {
                    string bytesEncoding = iniFile[null, "bytes_encoding", "hex"].ToLower(CultureInfo.InvariantCulture);
                    switch (bytesEncoding)
                    {
                        case "base64":
                            settings.BytesEncoding = BytesEncoding.Base64;
                            break;
                        case "base32":
                            settings.BytesEncoding = BytesEncoding.Base32;
                            break;
                        case "hex":
                        case "base16":
                        case "hexadecimal":
                            settings.BytesEncoding = BytesEncoding.Hexadecimal;
                            break;
                        case "oct":
                        case "base8":
                        case "octal":
                            settings.BytesEncoding = BytesEncoding.Octal;
                            break;
                        case "bin":
                        case "base2":
                        case "binary":
                            settings.BytesEncoding = BytesEncoding.Binary;
                            break;
                        default:
                            // Parse a custom encoding value.
                            settings.BytesEncoding =
                                (BytesEncoding)Enum.Parse(typeof(BytesEncoding), bytesEncoding);
                            break;
                    }
                }
                catch
                {
                    // Fallback to hexadecimal encoding on error.
                    settings.BytesEncoding = BytesEncoding.Hexadecimal;
                }

                // Attempt to read and set the Comparison setting.
                try
                {
                    settings.Comparison = iniFile.Read(null, "str_compare", StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    // Default to InvariantCultureIgnoreCase on error.
                    settings.Comparison = StringComparison.InvariantCultureIgnoreCase;
                }

                // Using a comment character.
                try
                {
                    string commentChar = iniFile[null, "comment_char", "semicolon"].ToLower(CultureInfo.InvariantCulture);
                    switch (commentChar)
                    {
                        case ";":
                        case "semicolon":
                            settings.CommentCharacter = IniFileCommentCharacter.Semicolon;
                            break;
                        case "#":
                        case "hash":
                            settings.CommentCharacter = IniFileCommentCharacter.Hash;
                            break;
                        case ";#":
                        case "#;":
                        case "both":
                            settings.CommentCharacter = IniFileCommentCharacter.SemicolonOrHash;
                            break;
                        default:
                            // Parse a custom comment character.
                            settings.CommentCharacter =
                                (IniFileCommentCharacter)Enum.Parse(typeof(IniFileCommentCharacter), commentChar);
                            break;
                    }
                }
                catch
                {
                    // Allow both by default.
                    settings.CommentCharacter = IniFileCommentCharacter.SemicolonOrHash;
                }

                // Attempt to read and set the EntryDelimiter setting.
                try
                {
                    string entryDelimiter = iniFile[null, "entry_delimiter", "equals"].ToLower();
                    switch (entryDelimiter)
                    {
                        case "=":
                        case "equals":
                            settings.EntrySeparatorCharacter = IniFileEntrySeparatorCharacter.Equal;
                            break;
                        case ":":
                        case "colon":
                            settings.EntrySeparatorCharacter = IniFileEntrySeparatorCharacter.Colon;
                            break;
                        case "=:":
                        case ":=":
                            settings.EntrySeparatorCharacter = IniFileEntrySeparatorCharacter.ColonOrEqual;
                            break;
                        default:
                            // Parse a custom entry separator character.
                            settings.EntrySeparatorCharacter =
                                (IniFileEntrySeparatorCharacter)Enum.Parse(typeof(IniFileEntrySeparatorCharacter),
                                    entryDelimiter);
                            break;
                    }
                }
                catch
                {
                    // Allow both by default.
                    settings.EntrySeparatorCharacter = IniFileEntrySeparatorCharacter.ColonOrEqual;
                }
            }
            return settings;
        }


        // Detects settings from an INI file if they are explicitly specified.
        internal static IniFileSettings LoadFromFile(string fileName)
        {
            string content = File.Exists(fileName) ? File.ReadAllText(fileName) : string.Empty;
            IniFileSettings settings = LoadFromContent(content);

            // Return the populated settings object.
            return settings;
        }

        internal static IniFileSettings Load()
        {
            string fileName = IniFile.GetIniFileName();
            return LoadFromFile(fileName);
        }

        // Create a regular expression object.
        internal Regex CreateRegex()
        {
            return new Regex(GetRegexPattern(), RegexOptions);
        }

        // Generates a regular expression pattern that can match various ini file formats.
        internal string GetRegexPattern()
        {
            StringBuilder sb = new StringBuilder(350);

            // Start building the regular expression pattern.
            sb.Append("(?=\\S)" +
                      "(?<text>" +
                      "(?<comment>" +
                      "(?<open>");

            // Add the comment open character.
            switch (CommentCharacter & IniFileCommentCharacter.SemicolonOrHash)
            {
                case IniFileCommentCharacter.Semicolon:
                    sb.Append(";");
                    break;
                case IniFileCommentCharacter.Hash:
                    sb.Append("#");
                    break;
                default:
                    sb.Append("[;#]");
                    break;
            }

            // Continue building the pattern for comments.
            sb.Append("+)(?:[^\\S\\r\\n]*)" +
                      "(?<value>.+))|" +

            // Building the section pattern.
                      "(?<section>" +
                      "(?<open>\\[)" +
                      "(?:[^\\S\\r\\n]*)" +
                      "(?<value>[^\\]]*\\S+)" +
                      "(?:[^\\S\\r\\n]*)" +
                      "(?<close>\\]))|" +

            // Building the entry pattern.
                      "(?<entry>" +
                      "(?<key>[^");

            // Add the entry separator character.
            switch (EntrySeparatorCharacter & IniFileEntrySeparatorCharacter.ColonOrEqual)
            {
                case IniFileEntrySeparatorCharacter.Colon:
                    sb.Append(":");
                    break;
                case IniFileEntrySeparatorCharacter.Equal:
                    sb.Append("=");
                    break;
                default:
                    sb.Append(":=");
                    break;
            }

            // Continue building the pattern for entries.
            sb.Append("\\r\\n\\[\\]]*\\S)" +
                      "(?:[^\\S\\r\\n]*)" +
                      "(?<delimiter>");

            // Add the entry separator character based on the EntrySeparatorCharacter property
            switch (EntrySeparatorCharacter & IniFileEntrySeparatorCharacter.ColonOrEqual)
            {
                case IniFileEntrySeparatorCharacter.Colon:
                    sb.Append(":");
                    break;
                case IniFileEntrySeparatorCharacter.Equal:
                    sb.Append("=");
                    break;
                default:
                    sb.Append(":|=");
                    break;
            }

            // Continue building the pattern for entries
            sb.Append(")" +
                      "(?:[^\\S\\r\\n]*)" +
                      "(?<value>[^");

            // If comments in the entry is allowed.
            if (AllowCommentsInEntries)
            {
                switch (CommentCharacter & IniFileCommentCharacter.SemicolonOrHash)
                {
                    case IniFileCommentCharacter.Semicolon:
                        sb.Append(";");
                        break;
                    case IniFileCommentCharacter.Hash:
                        sb.Append("#");
                        break;
                    default:
                        sb.Append(";#");
                        break;
                }
            }

            if (AllowMultiStringValues)
            {
                // Continue building the pattern for undefined content.
                sb.Append("\\r\\n]*))|" +
                          "(?<entry>(?<value>.+))" +
                          ")" +
                          "(?<=\\S)|");  
            }
            else
            {
                // Continue building the pattern for entries content.
                sb.Append("\\r\\n]*))|" +
                          "(?<undefined>.+)" +
                          ")" +
                          "(?<=\\S)|");
            }

            // Building the white spaces pattern.
            sb.Append("(?<linebreaker>\\r\\n|\\n)|" +
                      "(?<whitespace>[^\\S\\r\\n]+)");

            // Return the generated regular expression pattern
            return sb.ToString();
        }

        /// <summary>
        ///		Creates new instance of <see cref="IniFileSettings"/>
        ///		and copies the values of all the properties from the current instance to the new instance.
        /// </summary>
        /// <returns>
        ///		A deep copy of current <see cref="IniFileSettings"/> instance.
        /// </returns>
        public override object Clone()
        {
            // Create a new instance of IniFileSettings
            IniFileSettings clonedSettings = new IniFileSettings
            {
                // Copy the properties of the current instance to the new instance
                CommentCharacter = this.CommentCharacter,
                EntrySeparatorCharacter = this.EntrySeparatorCharacter,
                ParsingMethod = this.ParsingMethod,
                Comparison = this.Comparison,
                AllowCommentsInEntries = this.AllowCommentsInEntries,
                AllowEscapeCharacters = this.AllowEscapeCharacters,
                AddMissingEntries = this.AddMissingEntries,
                ReadOnly = this.ReadOnly,
                UseExtendedTypeConverters = this.UseExtendedTypeConverters
            };

            return clonedSettings;
        }

        #endregion
    }
}