/***************************************************************

•   File: LineBreakerStyle.cs

•   Description

    The LineBreakerStyle enumeration defines the newline character
    styles available for line-breaking within an INI or similar
    configuration file. It includes options to specify line-breaking
    characters for different platforms, such as POSIX, Windows, and
    legacy Mac systems, allowing flexibility in handling file formats 
    that may use varying newline conventions.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.ComponentModel;

namespace System.Ini
{
    /// <summary>
    ///     Represents different newline character styles for line-breaking in files.
    /// </summary>
    public enum LineBreakerStyle
    {
        /// <summary>
        ///     Determines newline characters automatically based on file contents.
        /// </summary>
        [Browsable(true)]
        Auto = 0,

        /// <summary>
        ///     Carriage return (CR) is used as the newline character.
        ///     This style was used in older Mac versions before OS X; 
        ///     modern Macs use the POSIX style.
        /// </summary>
        [StringValue("\r"), Obsolete("This style is deprecated; it is recommended to use the POSIX style instead.")]
        Cr = 1,

        /// <summary>
        ///     Line feed (LF) is used as the newline character.
        ///     This style follows the POSIX standard.
        /// </summary>
        [StringValue("\n")]
        Lf = 2,

        /// <summary>
        ///     Carriage return + line feed (CRLF) is used as the newline characters.
        ///     This style is common in Windows and various network protocols.
        /// </summary>
        [StringValue("\r\n")]
        CrLf = 3,

        /// <summary>
        ///     Uses the system default newline character(s), as defined by <see cref="Environment.NewLine"/>.
        /// </summary>
        [Browsable(true)]
        Default = 4
    }
}