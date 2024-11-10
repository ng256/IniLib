/*************************************************************** 

•   File: DoubleConverterExtended.cs

•   Description

    The  DoubleConverterExtended class extends the functionality
    of  the  BaseNumberConverterExtended    class   to   provide
    specialized   conversion  methods   for the double-precision
    floating point (Double) type. It  does not  support  parsing
    strings with different   number system prefixes. Instead, it
    focuses  on  decimal values  and converting Double values to
    strings using custom formatting.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel
{
    /// <summary>
    /// The <see cref="DoubleConverterExtended"/> class extends the functionality 
    /// of the <see cref="BaseNumberConverterExtended"/> class for converting 
    /// Double (double-precision floating point) values. It supports parsing strings 
    /// with different number system prefixes (e.g., decimal) and converting 
    /// Double values to their string representations using custom formatting.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public class DoubleConverterExtended : BaseNumberConverterExtended
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleConverterExtended"/> class.
        /// </summary>
        public DoubleConverterExtended() : base(allowBaseEncoding: false, targetType: typeof(double)) { }

        /// <summary>
        /// Converts a string representation of a number to a <see cref="Double"/> 
        /// value using the specified radix.
        /// </summary>
        /// <param name="value">The string representation of the number.</param>
        /// <param name="radix">The base (radix) to use for conversion is unsupported.</param>
        /// <returns>The converted <see cref="Double"/> value.</returns>
        [Obsolete]
        protected override object ConvertFromString(string value, int radix)
        {
            return Convert.ToDouble(value, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts a string representation of a number to a <see cref="Double"/> 
        /// value using the specified number format information.
        /// </summary>
        /// <param name="value">The string representation of the number.</param>
        /// <param name="formatInfo">The <see cref="NumberFormatInfo"/> used for parsing.</param>
        /// <returns>The converted <see cref="Double"/> value.</returns>
        protected override object ConvertFromString(string value, NumberFormatInfo formatInfo)
        {
            return double.Parse(value, NumberStyles.Float, formatInfo);
        }

        /// <summary>
        /// Converts a string representation of a number to a <see cref="Double"/> 
        /// value using the specified culture information.
        /// </summary>
        /// <param name="value">The string representation of the number.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> used for parsing.</param>
        /// <returns>The converted <see cref="Double"/> value.</returns>
        protected override object ConvertFromString(string value, CultureInfo culture)
        {
            return double.Parse(value, culture);
        }

        /// <summary>
        /// Converts a <see cref="Double"/> value to its string representation 
        /// using the specified number format information.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value.</param>
        /// <param name="formatInfo">The <see cref="NumberFormatInfo"/> used for formatting.</param>
        /// <returns>The string representation of the <see cref="Double"/> value.</returns>
        protected override string ConvertToString(object value, NumberFormatInfo formatInfo)
        {
            return ((double)value).ToString("R", formatInfo);
        }
    }
}