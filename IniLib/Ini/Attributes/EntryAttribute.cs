/***************************************************************

•   File: EntryAttribute.cs

•   Description

    The EntryAttribute class associates a property with a specific
    configuration entry name in an INI or similar config file, 
    optionally supporting alternative entry names (aliases). This 
    attribute is intended to be used by methods such as ReadSettings 
    and WriteSettings to identify and process specific entries within 
    the file based on attribute data.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;

namespace System.Ini
{
    /// <summary>
    ///     Attribute that associates a property with a specific entry in the config resource.
    ///     Used by the ReadSettings and WriteSettings methods to identify and process individual file entries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [Serializable]
    public sealed class EntryAttribute : Attribute
    {
        private readonly string _entryName = null;
        private readonly string[] _entryAliases;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntryAttribute"/> class with a specified entry name.
        /// </summary>
        /// <param name="entryName">The primary name of the entry.</param>
        /// <param name="entryAliases">An optional list of alternative names (aliases) for the entry.</param>
        public EntryAttribute(string entryName, params string[] entryAliases)
        {
            _entryName = entryName;
            _entryAliases = entryAliases ?? Empty<string>.Array;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntryAttribute"/> class with the default entry name.
        /// </summary>
        public EntryAttribute()
        {
        }

        /// <summary>
        ///     Gets the primary name of the entry.
        /// </summary>
        public string Name => _entryName;

        /// <summary>
        ///     Gets the alternative names (aliases) of the entry.
        /// </summary>
        public string[] Aliases => (string[])_entryAliases.Clone();

        /// <inheritdoc />
        public override bool IsDefaultAttribute()
        {
            return string.IsNullOrEmpty(_entryName);
        }

        /// <inheritdoc />
        public override bool Match(object obj)
        {
            return obj is EntryAttribute attribute
                   && attribute.Name.Equals(this._entryName)
                   && attribute.Aliases.ArrayEquals(this.Aliases);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _entryName;
        }
    }
}
