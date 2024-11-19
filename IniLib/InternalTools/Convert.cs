/*****************************************************************

    File: Convert.cs

•   Description:

•   Represents a set of static methods  and properties designed
    to perform various operations on data. These methods can be
    useful when working with different  data  types  and  their
    transformation. They can  be used to simplify code and make
    it more efficient.

•   Copyright

    © Pavel Bashkardin, 2022-2024

*****************************************************************/

using System.ComponentModel;
using System.Globalization;
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
        internal static ConverterCache Converters = ConverterCache.Default;

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
            {
                try
                {
                    result = converter.ConvertFrom(null, culture, value);
                    return true;
                }
                catch
                {
                    result = null;
                }
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
            {
                try
                {
                    result = converter.ConvertFrom(null, culture, value);
                    return true;
                }
                catch
                {
                    result = null;
                }
            }

            return false;
        }

        // Attempts to convert the value to the specified destination type using the type converter and culture.
        // Returns true if the conversion is successful and stores the result in the 'result' parameter.
        internal static bool TryConvertTo(this TypeConverter converter, Type destinationType, CultureInfo culture, object value, out object result)
        {
            result = null;

            if (value == null)
                return false;

            if (destinationType.IsInstanceOfType(value))
            {
                result = value;
                return true;
            }

            if (converter.CanConvertTo(destinationType))
            {
                try
                {
                    result = converter.ConvertTo(null, culture, value, destinationType);
                    return true;
                }
                catch
                {
                    result = null;
                }
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

            if (value is string str)
            {
                result = str;
                return true;
            }

            if (converter.CanConvertTo(typeof(string)))
            {
                try
                {
                    result = (string) converter.ConvertTo(null, culture, value, typeof(string));
                    return true;
                }
                catch
                {
                    result = null;
                }
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

            if (destinationType.IsInstanceOfType(value))
            {
                result = value;
                return true;
            }

            if (value.GetType().IsPrimitive && destinationType.IsPrimitive)
            {
                try
                {
                    result = Convert.ChangeType(value, destinationType, culture);
                    return true;
                }
                catch
                {
                }
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

        // Parses a string representation of a number into the smallest possible numeric type.
        internal static object ToNumber(this string number)
        {
            if (number.IsNullOrEmpty()) return null;

            // Check if the number is negative.
            bool isNegative = number.StartsWith("-");

            // Determine whether the number has a decimal point or exponent.
            bool isFloatingPoint = number.Contains('.') || number.Contains('e') || number.Contains('E');

            // Branch for floating-point numbers.
            if (isFloatingPoint)
            {
                // Analyze precision and range.
                int decimalIndex = number.IndexOf('.');
                int exponentIndex = number.IndexOfAny(new[] { 'e', 'E' });

                // Calculate the length of the fractional part (if any).
                int fractionalLength = decimalIndex >= 0
                    ? (exponentIndex > 0 ? exponentIndex : number.Length) - decimalIndex - 1
                    : 0;

                if (fractionalLength <= 7 && float.TryParse(number, out var floatValue)) return floatValue;
                if (fractionalLength <= 15 && double.TryParse(number, out var doubleValue)) return doubleValue;
                if (decimal.TryParse(number, out var decimalValue)) return decimalValue;
            }
            else
            {
                // Determine the length of the integer portion.
                int length = isNegative ? number.Length - 1 : number.Length;

                // Branch for negative numbers: skip unsigned types.
                if (isNegative)
                {
                    if (length <= 10 && int.TryParse(number, out var intValue)) return intValue;
                    if (length <= 19 && long.TryParse(number, out var longValue)) return longValue;
                }
                else
                {
                    // Branch for non-negative numbers: check both signed and unsigned types.
                    if (length <= 10 && int.TryParse(number, out var intValue)) return intValue;
                    if (length <= 10 && uint.TryParse(number, out var uintValue)) return uintValue;
                    if (length <= 19 && long.TryParse(number, out var longValue)) return longValue;
                    if (length <= 20 && ulong.TryParse(number, out var ulongValue)) return ulongValue;
                }
            }

            // If all parsing attempts fail, return null.
            return null;
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