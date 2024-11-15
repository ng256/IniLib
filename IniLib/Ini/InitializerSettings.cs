﻿/***************************************************************

•   File: IniFileSettings.cs

•   Description

    The InitializerSettings class is  a base set  of  parameters  
    that are used to configure the initializer.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///     Represents base settings that are used to configure the initializer.
    /// </summary>
    public abstract class InitializerSettings : ICloneable
    {
        private StringComparison _comparison = StringComparison.InvariantCultureIgnoreCase;
        private bool _allowEscapeCharacters = false;
        private bool _useExtendedTypeConverters = true;
        private PropertyFilter _propertyFilter = PropertyFilter.AllProperties;

        /// <summary>
        ///     Initialize a new instance of the <see cref="InitializerSettings"/> class.
        /// </summary>
        protected InitializerSettings()
        {
        }

        /// <summary>
        ///		Specifies the culture, case, and sort rules
        ///     to be used by <see cref="IniFileParser"/> object.
        /// </summary>
        public StringComparison Comparison
        {
            get => _comparison;
            set => _comparison = value;
        }

        /// <summary>
        ///		Use escaped characters in parameter values.
        /// </summary>
        public bool AllowEscapeCharacters
        {
            get => _allowEscapeCharacters;
            set => _allowEscapeCharacters = value;
        }

        /// <summary>
        ///	    Enables the use of improved type converters.
        ///	    They help you work efficiently with different representations of objects,
        ///	    such as numbers, references, character encodings, regional settings, and more.
        /// </summary>
        public bool UseExtendedTypeConverters
        {
            get => _useExtendedTypeConverters;
            set => _useExtendedTypeConverters = value;
        }

        /// <summary>
        ///     Defines modes for selecting which properties of a target object should be processed.
        /// </summary>
        public PropertyFilter PropertyFilter
        {
            get => _propertyFilter;
            set => _propertyFilter = value;
        }

        /// <summary>
        ///		Specifies a string comparison operation
        ///     based on <see cref="Comparison"/> property
        ///     to be used by <see cref="IniFileParser"/> object.
        /// </summary>
        public StringComparer Comparer => Comparison.GetComparer();

        /// <inheritdoc/>
        public abstract object Clone();
    }
}
