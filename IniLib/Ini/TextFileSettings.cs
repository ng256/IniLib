/***************************************************************

•   File: TextFileSettings.cs

•   Description

    The TextFileSettings class provides a base implementation 
    for handling settings stored in text-based files. This class 
    extends InitializerSettings, adding properties for configuring 
    newline characters and specifying read-only access. 

    Derived classes can utilize these features for fine-tuning 
    file handling, including support for various line-breaking 
    styles and the prevention of modifications to the file content.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///     Provides base functionality for handling text-based settings files.
    /// </summary>
    public abstract class TextFileSettings : InitializerSettings
    {
        private LineBreakerStyle _lineBreaker = LineBreakerStyle.Auto;
        private bool _readOnly = false;

        /// <summary>
        ///     Gets or sets the newline characters to be used in the text file.
        /// </summary>
        public LineBreakerStyle LineBreaker
        {
            get => _lineBreaker;
            set => _lineBreaker = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether writing values 
        ///     to the text file is permitted.
        /// </summary>
        public bool ReadOnly
        {
            get => _readOnly;
            set => _readOnly = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextFileSettings"/> class.
        /// </summary>
        protected TextFileSettings()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextFileSettings"/> class
        ///     with a specified string comparison option.
        /// </summary>
        /// <param name="comparison">
        ///     String comparison specifier to determine the behavior of key comparisons.
        /// </param>
        protected TextFileSettings(StringComparison comparison) : base(comparison)
        {
        }
    }
}
