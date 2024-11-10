using System.ComponentModel;

namespace System.Ini
{
    /// <summary>
    ///		Represents a newline characters.
    /// </summary>
    public enum LineBreakerStyle
    {
        /// <summary>
        ///		Determine newline characters based on file contents.
        /// </summary>
        Auto = 0,

        /// <summary>
        ///		Carriage return will be used as the newline character.
        ///		This style was used in older versions of Mac before OS X. Modern Macs use the POSIX style.
        /// </summary>
        [StringValue("\r"), Obsolete("This style is deprecated, and it is recommended to use the POSIX style instead.")]
        Cr = 1,

        /// <summary>
        ///		Line feed will be used as the newline character.
        ///		This is the POSIX standard.
        /// </summary>
        [StringValue("\n")]
        Lf = 2,

        /// <summary>
        ///		Carriage return + line feed will be used as newline characters.
        ///		This style is used by Windows and common network protocols.
        /// </summary>
        [StringValue("\r\n")]
        CrLf = 3,

        /// <summary>
        ///		System default that determined by <see cref="Environment.NewLine"/> property.
        /// </summary>
        Default = 4
    }
}