/***************************************************************

•   File: Initializer.cs

•   Description

    The Initializer  class serves as   an abstract base  for all
    initialization  tools that   interact   with   data sources,
    allowing the reading and saving of settings. This class also
    provides  methods for  converting values between types based
    on the  cultural settings    and  specific  type converters.

    Derived classes must implement  specific methods for reading
    and writing property values to and from a data source.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static System.InternalTools;

namespace System.Ini
{
    /// <summary>
    ///     Abstract base class for initialization tools that interact with 
    ///     data sources and allow the reading and saving of settings for 
    ///     properties of various types.
    /// </summary>
    [Serializable]
    public abstract class Initializer : IDisposable
    {
        [NonSerialized]
        private readonly ConverterCache _converters;

        [NonSerialized]
        private readonly CultureInfo _culture;

        #region Properties

        /// <summary>
        ///     Gets the current culture used for formatting and parsing operations.
        /// </summary>
        protected CultureInfo Culture => _culture;

        /// <summary>
        ///     Gets the cache that contains converters for various types that may be used for converting values.
        /// </summary>
        internal ConverterCache Converters => _converters;

        #endregion

        #region Constructor

        private Initializer() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Initializer"/> class 
        ///     using the provided settings.
        /// </summary>
        /// <param name="settings">Settings used for initializing culture and type converters.</param>
        protected Initializer(InitializerSettings settings)
        {
            if(settings == null) 
                throw new ArgumentNullException(nameof(settings));

            // Select the appropriate converter cache based on the settings.
            _converters = settings.UseExtendedTypeConverters
                ? ConverterCache.Extended
                : ConverterCache.Default;

            // Set the culture based on the comparison settings.
            _culture = settings.Comparison.GetCultureInfo();
        }

        #endregion

        // Converts a string value to the specified type using the provided converter.
        // If the value cannot be converted, the default value will be returned.
        internal object ConvertFromString(Type type, string value, object defaultValue, TypeConverter converter)
        {
            // If no converter is provided, use the default converter for the specified type.
            if (converter == null)
                converter = _converters.Get(type);

            // Attempt to convert value to the desired type.
            if (value != null)
            {
                if (type == typeof(string))
                    return value;

                if (converter.TryConvertFromString(_culture, value, out object result))
                    return result;
            }

            // If the type matches the default value, return the default value.
            if (type.IsInstanceOfType(defaultValue))
                return defaultValue;

            // Otherwise, try converting the default value to the target type.
            converter.TryConvertTo(type, _culture, defaultValue, out defaultValue);
            return defaultValue;
        }

        // Converts an array of string values to an array of the specified element type using a type converter.
        internal Array ConvertFromStrings(Type elementType, string[] values, TypeConverter converter)
        {
            // Create an array of the specified element type to hold the converted values.
            int length = values.Length;
            Array array = Array.CreateInstance(elementType, length);

            // If no values are provided, return the empty array.
            if (length == 0)
                return array;

            if (converter == null)
                converter = _converters.Get(elementType);

            // Convert each value in the string array to the desired element type.
            for (int i = 0; i < length; i++)
            {
                string value = values[i];
                object elementValue = null;

                // Attempt to convert each string value.
                if (value != null)
                {
                    if (converter.TryConvertFromString(_culture, value, out object result))
                        elementValue = result;
                    else if (TryChangeType(elementType, _culture, value, out result))
                        elementValue = result;
                }

                // Set the value in the array, or the default value if conversion failed.
                array.SetValue(elementValue ?? elementType.GetDefaultValue(), i);
            }

            return array;
        }

        // Converts a value to its string representation using the provided type converter.
        internal string ConvertToString(object value, TypeConverter converter)
        {
            // If the value is a string, return it directly.
            if (value != null)
            {
                if (value is string str)
                    return str;

                if (converter == null)
                {
                    Type type = value.GetType();
                    converter = _converters.Get(type);
                }

                // Use the converter to convert the value to a string.
                if (converter.CanConvertTo(typeof(string)))
                    return converter.ConvertToString(null, _culture, value);
            }

            return null;
        }

        // Converts an array of values to an array of strings using the provided type converter.
        internal string[] ConvertToStrings(Array array, TypeConverter converter)
        {
            // Create a string array to hold the converted values.
            string[] values = new string[array.Length];

            // Iterate through each element in the array and convert it to a string.
            for (int i = 0; i < array.Length; i++)
            {
                object arrayValue = array.GetValue(i);

                if (arrayValue == null)
                    values[i] = null;
                else if ((converter ?? _converters.Get(arrayValue.GetType())).TryConvertToString(_culture, arrayValue, out string stringValue))
                    values[i] = stringValue;
                else if (TryChangeType(typeof(string), _culture, arrayValue, out object objectValue))
                    values[i] = (string)objectValue;
            }

            return values;
        }

        /// <summary>
        ///     Restores a property value from the data source.
        ///     Must be implemented by derived classes to provide the actual logic for reading.
        /// </summary>
        /// <param name="property">
        ///     The property to read the value for.
        /// </param>
        /// <param name="obj">
        ///     The object to which the property belongs.
        /// </param>
        protected abstract void RestorePropertyValue(PropertyInfo property, object obj = null);

        /// <summary>
        ///     Stores a property value to the data source.
        ///     Must be implemented by derived classes to provide the actual logic for writing.
        /// </summary>
        /// <param name="property">
        ///     The property to write the value for.
        /// </param>
        /// <param name="obj">
        ///     The object to which the property belongs.
        /// </param>
        protected abstract void StorePropertyValue(PropertyInfo property, object obj = null);

        /// <summary>
        ///     Reads settings for all types in the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///     The assembly that contains the types to read settings for.
        /// </param>
        public virtual void ImportSettings(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            // Get all types in the assembly and read settings for each type.
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                ImportSettings(type);
            }
        }

        /// <summary>
        ///     Reads settings for the specified type and all its nested types.
        /// </summary>
        /// <param name="type">
        ///     The type to read settings for.
        /// </param>
        public virtual void ImportSettings(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Read settings for all properties of the specified type.
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                RestorePropertyValue(property);
            }

            // Recursively read settings for nested types.
            foreach (Type nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                ImportSettings(nestedType);
            }
        }

        /// <summary>
        ///     Reads settings for the properties of a given object.
        /// </summary>
        /// <param name="obj">
        ///     The object to read settings for.
        /// </param>
        public virtual void ImportSettings(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            // Get the type of the object and read settings for its properties.
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (PropertyInfo property in properties)
            {
                /*object defaultValue = property.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() is
                    DefaultValueAttribute defaultValueAttribute
                    ? defaultValueAttribute.Value
                    : null;*/
                RestorePropertyValue(property, obj);
            }
        }

        /// <summary>
        ///     Reads the assembly settings, which was the entry point of the application.
        ///     This method may not work in some contexts, such as ASP.NET applications, 
        ///     since web applications can be loaded in host processes where there is not always a clear “entrance point”. 
        /// </summary>
        public virtual void ImportSettings()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if(assembly != null) ImportSettings(assembly);
        }

        /// <summary>
        ///     Writes settings for all types in the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///     The assembly that contains the types to write settings for.
        /// </param>
        public virtual void ExportSettings(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            // Get all types in the assembly and write settings for each type.
            foreach (Type type in assembly.GetTypes())
            {
                ExportSettings(type);
            }
        }

        /// <summary>
        ///     Writes settings for the specified type and all its nested types.
        /// </summary>
        /// <param name="type">
        ///     The type to write settings for.
        /// </param>
        public virtual void ExportSettings(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Write settings for all properties of the specified type.
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                StorePropertyValue(property);
            }

            // Recursively write settings for nested types.
            foreach (Type nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                ExportSettings(nestedType);
            }
        }

        /// <summary>
        ///     Writes the assembly settings, which was the entry point of the application.
        ///     This method may not work in some contexts, such as ASP.NET applications, 
        ///     since web applications can be loaded in host processes where there is not always a clear “entrance point”. 
        /// </summary>
        public virtual void ExportSettings()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null) ExportSettings(assembly);
        }

        /// <summary>
        ///     Disposes of the resources used by the Initializer.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Releases resources used by the Initializer.
        /// </summary>
        /// <param name="disposing">
        ///     True if disposing is requested, false otherwise.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Override to dispose custom resources, if necessary.
        }
    }
}