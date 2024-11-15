/***************************************************************

•   File: IniFileEntrySeparatorCharacter.cs

•   Description

    The  IniFileEntrySeparatorCharacter enumeration is a list of
    possible characters used to  separate  entries in ini files.
    It includes three values:  Colon,  Equal  and  ColonOrEqual.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.ComponentModel;

namespace System.Ini
{
    /// <summary>
    ///		Defines the delimiter character between the key and value in the ini file.
    /// </summary>
    [Flags]
    public enum IniFileEntrySeparatorCharacter
    {
        /// <summary>
        ///		The separating character is a colon (:).
        /// </summary>
        [StringValue(":")]
        Colon = 1,

        /// <summary>
        ///		The separating character is the equal sign (=).
        /// </summary>
        [StringValue("=")]
        Equal = 2,

        /// <summary>
        ///		Separating characters are colon or equal sign.
        /// </summary>
        ColonOrEqual = 3
    }
}