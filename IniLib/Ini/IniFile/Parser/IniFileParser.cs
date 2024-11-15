/***************************************************************

•   File: IniFileParser.cs

•   Description

    The IniFileParser  class is an   abstract class that  is the
    basis   for working  with   INI  files.  It   implements the
    IIniFileReader  and IIniFileWriter  interfaces, allowing you
    to read  and  write  data  to ini files.

    The class contains methods   for saving the contents  of the
    current IniFileParser instance  to a specified file, as well
    as a static method for loading the contents from a specified
    file  and  creating  an   IniFileParser   instance.

    The class methods allow you to perform various operations on
    data in INI  files,  such as getting sections, keys, default
    values,  specific  values, as  well  as setting values ​​and
    their lists.

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;

namespace System.Ini
{
    /// <summary>
    ///     Represents an abstract class for parsing and modifying INI files.
    /// </summary>
    public abstract class IniFileParser : IDisposable
    {
        // Specifies the string comparison setting for sections and keys.
        private readonly StringComparison _comparison;

        // Specifies whether the INI file is read-only.
        private readonly bool _readOnly;

        #region Properties

        /// <summary>
        ///     Gets the <see cref="StringComparison"/> mode used for comparing sections and keys.
        /// </summary>
        public StringComparison Comparison => _comparison;

        /// <summary>
        ///     Gets a value indicating whether the INI file is read-only.
        /// </summary>
        public bool ReadOnly => _readOnly;

        /// <summary>
        ///     Gets or sets the content of the INI file as a single string.
        /// </summary>
        public abstract string Content { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the IniFileParser class with specified settings.
        /// </summary>
        /// <param name="settings">
        ///     The settings to configure the parser, including comparison and read-only mode.
        /// </param>

        protected IniFileParser(IniFileSettings settings)
        {
            _comparison = settings.Comparison;
            _readOnly = settings.ReadOnly;
        }


        internal IniFileParser Create(string content, IniFileSettings settings)
        {
            IniFileParsingMethod method = settings.ParsingMethod;
            IniFileParser parser = ((int)method % 2 == 0)
                ? (IniFileParser) new IniFileRegexParser(content, settings)
                : new IniFileDictionary(content, settings);

            return parser;
        }


        #endregion

        #region Methods

        /// <summary>
        ///     Retrieves a collection of all section names in the INI file.
        /// </summary>
        /// <returns>
        ///     An enumerable list of section names as strings.
        /// </returns>
        public abstract IEnumerable<string> GetSections();

        /// <summary>
        ///     Retrieves all keys within a specified section of the INI file.
        /// </summary>
        /// <param name="section">
        ///     The section from which to retrieve keys.
        /// </param>
        /// <returns>
        ///     An enumerable list of keys in the specified section.
        /// </returns>
        public abstract IEnumerable<string> GetKeys(string section);

        /// <summary>
        ///     Retrieves the value of a specified key within a section or returns a default value if not found.
        /// </summary>
        /// <param name="section">
        ///     The section containing the key.
        /// </param>
        /// <param name="key">
        ///     The key for which to retrieve the value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value to return if the key is not found.
        /// </param>
        /// <returns>
        ///     The value of the specified key or the default value.
        /// </returns>
        public abstract string GetValue(string section, string key, string defaultValue = null);

        /// <summary>
        ///     Retrieves all values within a specified section.
        /// </summary>
        /// <param name="section">
        ///     The section from which to retrieve values.
        /// </param>
        /// <returns>
        ///     An enumerable list of values in the specified section.
        /// </returns>
        public abstract IEnumerable<string> GetValues(string section);

        /// <summary>
        ///     Retrieves all values associated with a specified key within a section.
        /// </summary>
        /// <param name="section">
        ///     The section containing the key.
        /// </param>
        /// <param name="key">
        ///     The key for which to retrieve associated values.
        /// </param>
        /// <returns>
        ///     An enumerable list of values associated with the specified key.
        /// </returns>
        public abstract IEnumerable<string> GetValues(string section, string key);

        /// <summary>
        ///     Sets the value of a specified key in a section. If the value is null, removes the entry.
        /// </summary>
        /// <param name="section">
        ///     The section containing the key.
        /// </param>
        /// <param name="key">
        ///     The key to set or remove.
        /// </param>
        /// <param name="value">
        ///     The value to assign to the key, or null to remove the entry.
        /// </param>
        public abstract void SetValue(string section, string key, string value);

        /// <summary>
        ///     Sets multiple values for a specified key in a section.
        /// </summary>
        /// <param name="section">
        ///     The section containing the key.
        /// </param>
        /// <param name="key">
        ///     The key for which to set values.
        /// </param>
        /// <param name="values">
        ///     The values to assign to the key.
        /// </param>
        public abstract void SetValues(string section, string key, params string[] values);

        /// <summary>
        ///     Returns the content of the INI file as a string.
        /// </summary>
        /// <returns>
        ///     The INI file content.
        /// </returns>
        public override string ToString()
        {
            return Content;
        }

        /// <summary>
        ///     Performs tasks associated with removing, freeing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        #endregion
    }
}
