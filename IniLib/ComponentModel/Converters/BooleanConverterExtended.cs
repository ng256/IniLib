/***************************************************************

•   File: BooleanConverterEx.cs

•   Description

    The  BooleanConverterEx class  extends  the functionality of
    the standard class by providing additional  capabilities for
    converting  bool values. In  particular,  it  allows  you to
    convert  bool values  ​​not  only to the strings  "True"  and
    "False", but  also to  other equivalent representations such
    as "1" or "0",  "On" or "Off",  and "Enabled" or "Disabled".

***************************************************************/


using System.Collections.Generic;
using System.Globalization;
using static System.InternalTools;

namespace System.ComponentModel
{
    /// <summary>
    ///		An extended type converter to convert <see cref="T:System.Boolean" />
    ///     objects to and from various other representations.
    ///		It is an extension of the standard class <see cref="BooleanConverter"/>.
    /// </summary>
    public class BooleanConverterExtended : TypeConverter
    {
        private static readonly TypeConverter NumberConverter = new Int32ConverterExtended();

        private static readonly Dictionary<string, bool> Values 
            = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            {"1", true},
            {"On", true},
            {"Yes", true},
            {"True", true},
            {"Enable", true},
            {"Enabled", true},

            {"0", false},
            {"No", false},
            {"Off", false},
            {"False", false},
            {"Disable", false},
            {"Disabled", false},
        };

        private static bool _registered;

        /// <summary>
        ///		Register <see cref="BooleanConverterExtended"/> as default converter for <see cref="bool"/> struct.
        /// </summary>
        public static void Register()
        {
            _registered = _registered || RegisterConverter<bool, BooleanConverterExtended>();
        }

        static BooleanConverterExtended()
        {
            _registered = false;
            //Register();
        }

        // Attempts to parse the given object value as a boolean value.
        private static bool TryParse(object value, CultureInfo culture, out bool result)
        {
            result = false;
            object obj;

            switch (value)
            {
                case null:
                    break;

                case bool b:
                    return b;

                case int i:
                    result = i != 0;
                    return true;

                case string s:
                    s = s.TrimWhiteSpaceAndNull();

                    if (s.Length == 0) 
                        return false;

                    if (Values.TryGetValue(s, out result))
                        return true;

                    if (NumberConverter.TryConvertFromString(culture, s, out obj))
                    {
                        result = (int) obj != 0;
                        return true;
                    }

                    break;

                case IConvertible conv:
                    if (TryChangeType(typeof(int), culture, value, out obj))
                    {
                        result = (int)obj != 0;
                        return true;
                    }
                    break;

                // For primitive types, try converting the value to an integer and then parsing it.
                default:

                    return false;

            }

            return false;
        }

        /// <summary>
        ///		Converts the given value object to a Boolean object.
        /// </summary>
        /// <param name="context">
        ///		A <see cref="ITypeDescriptorContext" /> object that provides the format context.
        /// </param>
        /// <param name="culture">
        ///		A <see cref="CultureInfo" /> object that defines the culture to be converted.
        /// </param>
        /// <param name="value">
        ///		The object to be converted <see cref="object" />.
        /// </param>
        /// <returns>
        ///		The <see cref="object" /> object representing the converted value <paramref name="value" />.
        /// </returns>
        /// <exception cref="FormatException">
        ///		The value of <paramref name="value" /> is not a valid value for the final type.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///		The conversion could not be performed.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return TryParse(value, culture, out bool result)
                ? result
                : throw new FormatException(GetResourceString("ConvertInvalidPrimitive", value, "Boolean"));
        }

        /// <summary>Returns a value indicating whether this converter can convert an object to the specified type using the specified context.</summary>
        /// <param name="context">A <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object that provides the format context.</param>
        /// <param name="destinationType">The <see cref="T:System.Type" /> class representing the type to convert to.</param>
        /// <returns>Is <see langword="true" /> if the converter can perform the conversion, otherwise <see langword="false" />.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            return destinationType == typeof(bool) || destinationType.IsIntOrString();
        }

        /// <summary>
        ///		Converts the given value to a Boolean object using the specified context and culture information.
        /// </summary>
        /// <param name="context">
        ///		A <see cref="ITypeDescriptorContext" /> object that provides the format context.
        /// </param>
        /// <param name="culture">
        /// <see cref="CultureInfo" /> object. If <see langword="null" /> is passed, the current culture settings are used.
        /// </param>
        /// <param name="value">
        ///		The object to be converted <see cref="T:System.Object" />.
        /// </param>
        /// <param name="destinationType">
        /// <see cref="Type" /> to which the <paramref name="value" /> parameter is converted.
        /// </param>
        /// <returns>
        ///		The <see cref="object" /> object representing the converted value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///		The parameter <paramref name="destinationType" /> has the value <see langword="null" />.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///		The conversion could not be performed.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            switch (Type.GetTypeCode(destinationType))
            {
                case TypeCode.Object
                    when destinationType == typeof(bool) &&
                         TryParse(value, culture, out bool result):
                    return result;
                case TypeCode.Int32 when value is bool c:
                    return c ? 1 : 0;
                case TypeCode.String when value is bool c:
                    return c ? bool.TrueString : bool.FalseString;
                default:
                    return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
