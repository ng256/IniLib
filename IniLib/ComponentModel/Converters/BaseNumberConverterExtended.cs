using System.Globalization;
using System.Security.Permissions;
using static System.InternalTools;

namespace System.ComponentModel
{
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public abstract class BaseNumberConverterExtended : TypeConverter
    {
        protected bool AllowBaseEncoding = true;

        protected Type TargetType;

        protected abstract object ConvertFromString(string value, int radix);

        protected abstract object ConvertFromString(string value, NumberFormatInfo formatInfo);

        protected abstract object ConvertFromString(string value, CultureInfo culture);

        protected abstract string ConvertToString(object value, NumberFormatInfo formatInfo);

        protected BaseNumberConverterExtended(Type targetType)
        {
            TargetType = targetType;
        }

        protected BaseNumberConverterExtended(bool allowBaseEncoding, Type targetType)
        {
            AllowBaseEncoding = allowBaseEncoding;
            TargetType = targetType;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType.IsPrimitive || sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            // Use case-insensitive comparison for base prefixes
            const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

            // If culture is not provided, use the current culture
            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            // Process the input based on its type
            switch (value)
            {
                // Handle string input
                case string str when (str = str.Trim()).Length > 0:
                    try
                    {
                        // Get NumberFormatInfo based on the specified culture.
                        NumberFormatInfo format = (NumberFormatInfo)culture.GetFormat(typeof(NumberFormatInfo));

                        // If the string consists entirely of digits, parse it directly as decimal.
                        if (char.IsDigit(str, 0) && char.IsDigit(str, str.Length - 1))
                        {
                            return ConvertFromString(str, format);
                        }

                        // Handle encoding-specific prefixes if allowed.
                        if (AllowBaseEncoding)
                        {
                            // Hexadecimal format, prefix #, &, $, 0x, &h or suffix h.
                            if (str[0] == '#' || str[0] == '&' || str[0] == '$')
                                return ConvertFromString(str.Substring(1), 16);

                            if (str.EndsWith("h", comparison))
                                return ConvertFromString(str.Substring(0, str.Length - 1), 16);

                            if (str.StartsWith("0x", comparison) || str.StartsWith("&h", comparison))
                                return ConvertFromString(str.Substring(2), 16);

                            // Binary format, prefix %, 0b or suffix b.
                            if (str[0] == '%')
                                return ConvertFromString(str.Substring(1), 2);

                            if (str.StartsWith("0b", comparison))
                                return ConvertFromString(str.Substring(2), 2);

                            if (str.EndsWith("b", comparison))
                                return ConvertFromString(str.Substring(0, str.Length - 1), 2);

                            // Octal format, prefixes 0o, &o, 8# or suffix o.
                            if (str.StartsWith("0o", comparison) || str.StartsWith("&o", comparison) || str.StartsWith("8#"))
                                return ConvertFromString(str.Substring(2), 8);

                            if (str.EndsWith("o", comparison))
                                return ConvertFromString(str.Substring(0, str.Length - 1), 8);
                        }

                        // Default to parsing the string as a decimal number.
                        return ConvertFromString(str, format);
                    }
                    catch (Exception e)
                    {
                        // If parsing fails, throw an error with details
                        throw new Exception(GetResourceString("ConvertInvalidPrimitive", str, TargetType.Name), e);
                    }

                // Handle convertible types by changing them to the target type
                case IConvertible conv:
                    return Convert.ChangeType(conv, TargetType, culture);
            }

            // Fall back to base conversion if no cases match
            return base.ConvertFrom(context, culture, value);
        }


        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            if (destinationType == typeof(string) && value != null && TargetType.IsInstanceOfType(value))
            {
                if (culture == null)
                    culture = CultureInfo.CurrentCulture;
                NumberFormatInfo format = (NumberFormatInfo)culture.GetFormat(typeof(NumberFormatInfo));
                return ConvertToString(value, format);
            }
            return destinationType.IsPrimitive ? Convert.ChangeType(value, destinationType, culture) : base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.IsPrimitive || base.CanConvertTo(context, destinationType);
        }
    }
}