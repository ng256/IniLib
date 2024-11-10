/*****************************************************************

    File: Convert.cs

•   Description:

•   Represents a set of static methods  and properties designed
    to perform various operations on data. These methods can be
    useful when working with different  data  types  and  their
    transformation. They can  be used to simplify code and make
    it more efficient.

•   Copyright

    © Pavel Bashkardin, 2022

*****************************************************************/

using System.ComponentModel;
using System.Globalization;
using System.Ini;
using System.Linq;
using System.Security;

namespace System
{
    internal static partial class InternalTools
    {
        /************************************************************************************************
         * Methods that converts a value of one type to another type.
         * The method takes a value, a target type, a type converter, and a culture.
         * It returns the converted value or null (or default value) if conversion is not possible.
         ************************************************************************************************/

        // Maintains a cache of type converters for improved performance
        internal static ConverterCache Converters = new ConverterCache()
        {
            { typeof(int), new Int32Converter() }
        };

        // Casts the given object to the specified type T.
        // If the source is null, it returns the default value of T.
        // If the source is already of type T, it returns the source.
        // Otherwise, it throws an ArgumentException.
        internal static T CastTo<T>(this object source, string name = null)
        {
            switch (source)
            {
                case null:
                    return default(T) == null // Check if the type T is nullable.
                        ? default(T)
                        : throw new ArgumentNullException(name, GetResourceString("Arg_NullReferenceException"));
                case T dest:
                    return dest;
                default:
                    throw new ArgumentException(GetResourceString("Arg_WrongType", source, typeof(T)), name);
            }
        }

        // Retrieves the type converter for the specified type.
        internal static TypeConverter GetConverter(this Type type)
        {
            return Converters.Get(type);
        }

        // Converts the value using the specified type converter and culture.
        internal static object ConvertFrom(this TypeConverter converter, CultureInfo culture, object value)
        {
            return converter.ConvertFrom(context: (ITypeDescriptorContext)null, culture, value);
        }

        // Converts the value to the specified destination type using the type converter and culture.
        internal static object ConvertTo(this TypeConverter converter, CultureInfo culture, object value,
            Type destinationType)
        {
            return converter.ConvertTo(context: (ITypeDescriptorContext)null, culture, value, destinationType);
        }

        // Converts the value to a string representation using the type converter and culture.
        internal static object ConvertToString(this TypeConverter converter, CultureInfo culture, object value,
            Type destinationType)
        {
            return converter.ConvertToString(context: (ITypeDescriptorContext)null, culture, value);
        }

        // Converts a string value to an object using the type converter and culture.
        internal static object ConvertFromString(this TypeConverter converter, CultureInfo culture, string value)
        {
            return converter.ConvertFromString(context: (ITypeDescriptorContext)null, culture, value);
        }

        // Checks if the object is of the specified type.
        internal static bool Is(this object obj, Type type)
        {
            return obj?.GetType() == type;
        }

        // Attempts to convert from the specified value using the type converter and culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryConvertFrom(this TypeConverter converter, CultureInfo culture, object value, out object result)
        {
            result = null;

            if (value == null)
                return false;

            Type sourceType = value.GetType();

            if (converter.CanConvertFrom(sourceType))
                try
                {
                    result = converter.ConvertFrom(null, culture, value);
                    return true;
                }
                catch
                {
                    result = null;
                }

            return false;
        }

        // Attempts to convert from the specified string using the type converter and culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryConvertFromString(this TypeConverter converter, CultureInfo culture, string value, out object result)
        {
            result = null;

            if (value == null)
                return false;

            if (converter.CanConvertFrom(typeof(string)))
                try
                {
                    result = converter.ConvertFrom(null, culture, value);
                    return true;
                }
                catch
                {
                    result = null;
                }

            return false;
        }

        // Attempts to convert the value to the specified destination type using the type converter and culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryConvertTo(this TypeConverter converter, Type destinationType, CultureInfo culture, object value, out object result)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            result = null;

            if (value == null)
                return false;

            if (converter.CanConvertTo(destinationType))
                try
                {
                    result = converter.ConvertTo(null, culture, value, destinationType);
                    return true;
                }
                catch
                {
                    result = null;
                }

            return false;
        }

        // Attempts to convert the value to string type using the type converter and culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryConvertToString(this TypeConverter converter, CultureInfo culture, object value, out string result)
        {
            result = null;

            if (value == null)
                return false;

            if (converter.CanConvertTo(typeof(string)))
                try
                {
                    result = (string) converter.ConvertTo(null, culture, value, typeof(string));
                    return true;
                }
                catch
                {
                    result = null;
                }

            return false;
        }

        // Attempts to convert the value to the specified destination type using the specified culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryChangeType(Type destinationType, CultureInfo culture, object value, out object result)
        {
            result = null;

            if (value == null)
                return false;

            if (value.GetType().IsPrimitive && destinationType.IsPrimitive)
                try
                {
                    result = Convert.ChangeType(value, destinationType, culture);
                    return true;
                }
                catch
                {
                }

            // If conversion fails.
            return false;
        }

        // Returns the default value for a given type.
        // If the type is a value type (like int, float, struct), it creates an instance of that type with default values.
        // If the type is a reference type, it returns null as the default value.
        internal static object GetDefaultValue(this Type type)
        {
            return type.IsValueType 
                ? Activator.CreateInstance(type) 
                : null;
        }

        // Attempts to convert the value to the specified destination type using the type converter and culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryConvert(this object value, Type destinationType, TypeConverter converter,
            CultureInfo culture, out object result)
        {
            result = default;
            if (value == null || destinationType == null || converter == null) return false;
            if (culture == null) culture = CultureInfo.InvariantCulture;
            Type sourceType = value.GetType();

            try
            {
                if (converter.CanConvertFrom(sourceType))
                {
                    result = converter.ConvertFrom(culture, value);
                    return result.Is(destinationType);
                }

                if (converter.CanConvertTo(destinationType))
                {
                    result = converter.ConvertTo(culture, value, destinationType);
                    return result.Is(destinationType);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // Converts the value to the specified destination type using the custom converter or the default converter.
        // If the conversion fails, it returns the provided default value.
        internal static object ConvertTo(this object value, Type destinationType, object defaultValue,
            TypeConverter customConverter, CultureInfo culture)
        {
            if (value == null) return defaultValue;
            Type sourceType = value.GetType();
            if (destinationType == typeof(object) || sourceType == destinationType ||
                destinationType.IsAssignableFrom(sourceType)) return value;
            if (destinationType.IsAssignableFrom(sourceType)) return value;
            if (culture == null) culture = CultureInfo.InvariantCulture;

            if (value.TryConvert(destinationType, customConverter, culture, out object result)) return result;
            if (value.TryConvert(destinationType, destinationType.GetConverter(), culture, out result)) return result;
            if (value.TryConvert(destinationType, sourceType.GetConverter(), culture, out result)) return result;

            return defaultValue;
        }

        // Converts the value from the specified destination type using the custom converter or the default converter.
        // If the conversion fails, it returns the provided default value.
        internal static object ConvertFrom(this object value, Type destinationType, object defaultValue,
            TypeConverter customConverter, CultureInfo culture)
        {
            if (value == null) return defaultValue;
            Type sourceType = value.GetType();
            if (destinationType == typeof(object) || sourceType == destinationType ||
                destinationType.IsAssignableFrom(sourceType)) return value;
            if (destinationType.IsAssignableFrom(sourceType)) return value;
            if (culture == null) culture = CultureInfo.InvariantCulture;

            if (value.TryConvert(destinationType, customConverter, culture, out object result)) return result;
            if (value.TryConvert(destinationType, sourceType.GetConverter(), culture, out result)) return result;
            if (value.TryConvert(destinationType, destinationType.GetConverter(), culture, out result)) return result;

            return defaultValue;
        }

        // Converts the value to the specified destination type using the custom converter.
        internal static object ConvertTo(this object value, Type destinationType, TypeConverter customConverter,
            CultureInfo culture)
        {
            return value.ConvertTo(destinationType, defaultValue: null, customConverter, culture);
        }

        // Converts the value from the specified destination type using the custom converter.
        internal static object ConvertFrom(this object value, Type sourceType, TypeConverter customConverter,
            CultureInfo culture)
        {
            return value.ConvertFrom(sourceType, defaultValue: null, customConverter, culture);
        }

        // Converts the value to the specified destination type using the default converter.
        internal static object ConvertTo(this object value, Type destinationType, object defaultValue,
            CultureInfo culture)
        {
            return value.ConvertTo(destinationType, defaultValue, destinationType.GetConverter(), culture);
        }

        // Converts the value from the specified destination type using the default converter.
        internal static object ConvertFrom(this object value, Type sourceType, object defaultValue,
            CultureInfo culture)
        {
            return value.ConvertFrom(sourceType, defaultValue, sourceType.GetConverter(), culture);
        }

        // Converts the value to the specified type T using the provided converter and culture.
        internal static T ConvertTo<T>(this object value, TypeConverter converter, CultureInfo culture)
        {
            Type type = typeof(T);
            object result = value.ConvertTo(type, converter, culture);
            return (T)(result ?? default(T));
        }

        // Converts the value from the specified type T using the provided converter and culture.
        internal static T ConvertFrom<T>(this object value, TypeConverter converter, CultureInfo culture)
        {
            Type type = typeof(T);
            object result = value.ConvertFrom(type, converter, culture);
            return (T)(result ?? default(T));
        }

        // Converts the value to the specified type T using the provided converter and culture, or returns the default value if the conversion fails.
        internal static T ConvertTo<T>(this object value, T defaultValue, TypeConverter converter, CultureInfo culture)
        {
            Type type = typeof(T);
            object result = value.ConvertTo(type, converter, culture);
            return (T)(result ?? defaultValue);
        }

        // Converts the value from the specified type T using the provided converter and culture, or returns the default value if the conversion fails.
        internal static T ConvertFrom<T>(this object value, T defaultValue, TypeConverter converter, CultureInfo culture)
        {
            Type type = typeof(T);
            object result = value.ConvertFrom(type, converter, culture);
            return (T)(result ?? defaultValue);
        }

        // Converts the value to the specified type T using the default converter and culture.
        internal static T ConvertTo<T>(this object value, CultureInfo culture = null)
        {
            Type type = typeof(T);
            object result = value.ConvertTo(type, Converters.Get(type), culture);
            return (T)(result ?? default(T));
        }

        // Converts the value from the specified type T using the default converter and culture.
        internal static T ConvertFrom<T>(this object value, CultureInfo culture = null)
        {
            Type type = typeof(T);
            object result = value.ConvertFrom(type, Converters.Get(type), culture);
            return (T)(result ?? default(T));
        }

        // Converts the value to the specified type T using the default converter and culture, or returns the provided default value if the conversion fails.
        internal static T ConvertTo<T>(this object value, T defaultValue, CultureInfo culture = null)
        {
            Type type = typeof(T);
            object result = value.ConvertTo(type, defaultValue, Converters.Get(type), culture);
            return (T)(result ?? defaultValue);
        }

        // Converts the value from the specified type T using the default converter and culture, or returns the provided default value if the conversion fails.
        internal static T ConvertFrom<T>(this object value, T defaultValue, CultureInfo culture = null)
        {
            Type type = typeof(T);
            object result = value.ConvertFrom(type, defaultValue, Converters.Get(type), culture);
            return (T)(result ?? defaultValue);
        }

        /************************************************************************************************
         * Methods that convert an array to a new array with elements of a different type.
         * These methods use the specified type converter and culture to perform the conversion.
         ************************************************************************************************/

        // Converts the elements of the input array to the specified element type using the provided type converter and culture.
        // The method creates a new array with the converted elements.
        internal static Array ConvertTo(this Array array, Type elementType, TypeConverter customConverter,
            CultureInfo culture)
        {
            if (array == null)
                return null;

            if (array.GetType().GetElementType() == elementType && array.Rank == 1)
                return array;

            Array newArray = Array.CreateInstance(elementType, array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                
                object value = array.GetValue(i);
                value = value.ConvertTo(elementType, customConverter ?? value.GetType().GetConverter(), culture);
                if (value != null) newArray.SetValue(value, i);
            }

            return newArray;
        }

        // Converts the elements of the input array to the specified element type using the provided type converter and culture.
        // The method creates a new array with the converted elements.
        internal static Array ConvertFrom(this Array array, Type elementType, TypeConverter customConverter,
            CultureInfo culture)
        {
            if (array == null)
                return null;

            if (array.GetType().GetElementType() == elementType && array.Rank == 1)
                return array;

            Array newArray = Array.CreateInstance(elementType, array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                object value = array.GetValue(i).ConvertFrom(elementType, customConverter, culture);
                if (value != null) newArray.SetValue(value, i);
            }

            return newArray;
        }

        // Converts the elements of the input array to the specified element type using the default type converter and culture.
        // The method creates a new array with the converted elements.
        internal static Array ConvertTo(this Array array, Type elementType, CultureInfo culture)
        {
            return array.ConvertTo(elementType, elementType.GetConverter(), culture);
        }

        // Converts the elements of the input array from the specified element type using the default type converter and culture.
        // The method creates a new array with the converted elements.
        internal static Array ConvertFrom(this Array array, Type elementType, CultureInfo culture)
        {
            return array.ConvertFrom(elementType, elementType.GetConverter(), culture);
        }

        // Converts the elements of the input array to the specified generic type T using the provided type converter and culture.
        // The method creates a new array of type T[] with the converted elements.
        internal static T[] ConvertTo<T>(this Array array, TypeConverter customConverter, CultureInfo culture = null)
        {
            return (T[])array.ConvertTo(typeof(T), customConverter, culture);
        }

        // Converts the elements of the input array from the specified generic type T using the provided type converter and culture.
        // The method creates a new array of type T[] with the converted elements.
        internal static T[] ConvertFrom<T>(this Array array, TypeConverter customConverter, CultureInfo culture = null)
        {
            return (T[])array.ConvertFrom(typeof(T), customConverter, culture);
        }

        // Converts the elements of the input array to the specified generic type T using the default type converter and culture.
        // The method creates a new array of type T[] with the converted elements.
        internal static T[] ConvertTo<T>(this Array array, CultureInfo culture = null)
        {
            return (T[])array.ConvertTo(typeof(T), typeof(T).GetConverter(), culture);
        }

        // Converts the elements of the input array from the specified generic type T using the default type converter and culture.
        // The method creates a new array of type T[] with the converted elements.
        internal static T[] ConvertFrom<T>(this Array array, CultureInfo culture = null)
        {
            return (T[])array.ConvertFrom(typeof(T), typeof(T).GetConverter(), culture);
        }

        /************************************************************************************************
         * Methods that convert a single object value to an array of the specified element type.
         * These methods use the specified type converter and culture to perform the conversion.
         ************************************************************************************************/

        // Converts the input value to an array of the specified element type using the provided type converter and culture.
        // If the input value is already an array, it returns the array with the elements converted to the specified type.
        internal static Array AsArray(this object value, Type elementType, TypeConverter customConverter,
            CultureInfo culture)
        {
            if (value == null) return null;

            if (value is Array array)
            {
                return array.GetType().GetElementType() == elementType && array.Rank == 1
                    ? array
                    : array.ConvertTo(elementType, customConverter, culture);
            }

            array = Array.CreateInstance(elementType, 1);
            array.SetValue(value.ConvertTo(elementType, customConverter, culture), 0);
            return array;
        }

        // Converts the input value to an array of the specified element type using the default type converter and culture.
        // If the input value is already an array, it returns the array with the elements converted to the specified type.
        internal static Array AsArray(this object value, Type elementType, CultureInfo culture)
        {
            return value.AsArray(elementType, elementType.GetConverter(), culture);
        }

        // Converts the input value to an array of the specified generic type T using the provided type converter and culture.
        // If the input value is already an array, it returns the array with the elements converted to type T.
        internal static T[] AsArray<T>(this object value, TypeConverter customConverter, CultureInfo culture = null)
        {
            return (T[])value.AsArray(typeof(T), customConverter, culture);
        }

        // Converts the input value to an array of the specified generic type T using the default type converter and culture.
        // If the input value is already an array, it returns the array with the elements converted to type T.
        internal static T[] AsArray<T>(this object value, CultureInfo culture = null)
        {
            return (T[])value.AsArray(typeof(T), typeof(T).GetConverter(), culture);
        }


        /******************************************************************************************
         * Serializes and deserializes a value of the specified type into/from a byte array.
         *******************************************************************************************/

        // Allows to deserialize a byte array into a specified data type.
        // The method accepts a byte array and an offset and returns a value of the specified type.
        [SecurityCritical]
        internal static unsafe T Deserialize<T>(this byte[] bytes, int offset = 0) where T : unmanaged
        {
            fixed (byte* buffer = bytes)
            {
                return *(T*)(buffer + offset);
            }
        }

        // Serializes a value of the specified type into a byte array.
        [SecurityCritical]
        internal static unsafe byte[] Serialize<T>(this T value) where T : unmanaged
        {
            byte[] bytes = new byte[sizeof(T)];
            fixed (byte* buffer = bytes)
            {
                *(T*)buffer = value;
                return bytes;
            }
        }

        // Checks whether the specified type inherits an interface.
        internal static bool InheritInterface<T>(this Type type) where T : class
        {
            Type interfaceType = typeof(T);
            return type != null && interfaceType.IsInterface && type.GetInterfaces().Contains(interfaceType);
        }
    }
}