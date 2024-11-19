/***************************************************************

•   File: TextFileParser.cs

•   Description

    The TextFileParser class serves as an abstract base for 
    parsing settings files in a text-based format. It provides 
    core properties and methods to support the parsing process, 
    including configuration for escape characters, string 
    comparison, line-breaking styles, and read-only restrictions. 

    Derived classes must implement specific parsing logic 
    tailored to the particular text file format.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/
namespace System.Ini
{
    /// <summary>
    ///     Provides a base class for parsing text-based configuration files.
    /// </summary>
    public abstract class TextFileParser
    {
        // Indicates whether escape characters are allowed.
        private readonly bool _allowEscapeChars;

        // Comparison method used when searching for the text data.
        private readonly StringComparison _comparison;

        // String comparer to create string collections.
        private readonly StringComparer _comparer;

        // The line breaker used to separate lines in the text file.
        private readonly string _lineBreaker;

        // Specifies whether the text file is read-only.
        private readonly bool _readOnly;

        /// <summary>
        ///     Prevents direct instantiation of the <see cref="TextFileParser"/> class.
        /// </summary>
        private TextFileParser() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextFileParser"/> class 
        ///     using the provided content and settings.
        /// </summary>
        /// <param name="content">The content of the text file to be parsed.</param>
        /// <param name="settings">
        ///     An instance of <see cref="TextFileSettings"/> that specifies
        ///     configuration options for parsing.
        /// </param>
        protected TextFileParser(string content, TextFileSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _comparison = settings.Comparison;
            _comparer = settings.Comparer;
            _readOnly = settings.ReadOnly;
            _allowEscapeChars = settings.AllowEscapeCharacters;
            _lineBreaker = (settings.LineBreaker == LineBreakerStyle.Auto
                ? content.AutoDetectLineBreakerEx()
                : settings.LineBreaker).GetString();
        }

        /// <summary>
        ///     Gets a value indicating whether escape characters are allowed in the text file.
        /// </summary>
        protected bool AllowEscapeChars => _allowEscapeChars;

        /// <summary>
        ///     Gets the string comparison method used for searching text data.
        /// </summary>
        protected StringComparison Comparison => _comparison;

        /// <summary>
        ///     Gets the string comparer used for creating string collections.
        /// </summary>
        protected StringComparer Comparer => _comparer;

        /// <summary>
        ///     Gets the newline character sequence used to separate lines in the text file.
        /// </summary>
        protected string LineBreaker => _lineBreaker;

        /// <summary>
        ///     Gets a value indicating whether the text file is read-only.
        /// </summary>
        protected bool ReadOnly => _readOnly;
    }
}
