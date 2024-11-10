namespace System.Ini
{
    /// <summary>
    /// Attribute that associates a class or property with a specific section in the INI file.
    /// Used by the ReadSettings and WriteSettings methods to identify and process sections.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [Serializable]
    public sealed class SectionAttribute : Attribute
    {
        private readonly string _sectionName = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionAttribute"/> class with a specified section name.
        /// </summary>
        /// <param name="sectionName">The name of the INI section.</param>
        public SectionAttribute(string sectionName)
        {
            _sectionName = sectionName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionAttribute"/> class with the default section name.
        /// </summary>
        public SectionAttribute()
        {
        }

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        public string Name
        {
            get => _sectionName;
        }

        /// <inheritdoc />
        public override bool IsDefaultAttribute()
        {
            return string.IsNullOrEmpty(_sectionName);
        }

        /// <inheritdoc />
        public override bool Match(object obj)
        {
            return obj is SectionAttribute attribute && attribute.Name.Equals(_sectionName);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _sectionName;
        }
    }

    /// <summary>
    /// Attribute that associates a property with a specific entry in the config resource.
    /// Used by the ReadSettings and WriteSettings methods to identify and process individual file entries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [Serializable]
    public sealed class IniEntryAttribute : Attribute
    {
        private readonly string _entryName = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniEntryAttribute"/> class with a specified entry name.
        /// </summary>
        /// <param name="entryName">The name of the INI entry.</param>
        public IniEntryAttribute(string entryName)
        {
            _entryName = entryName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniEntryAttribute"/> class with the default entry name.
        /// </summary>
        public IniEntryAttribute()
        {
        }

        /// <summary>
        /// Gets the name of the INI entry.
        /// </summary>
        public string Name => _entryName;

        /// <inheritdoc />
        public override bool IsDefaultAttribute()
        {
            return string.IsNullOrEmpty(_entryName);
        }

        /// <inheritdoc />
        public override bool Match(object obj)
        {
            return obj is IniEntryAttribute attribute && attribute.Name.Equals(_entryName);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _entryName;
        }

    }
}
