/***************************************************************

•   File: IniFileCommentCharacter.cs

•   Description

    The  IniFileCommentCharacter enumeration  is    a list  of
    possible  characters that can  be used as comments  in ini
    files.  It  includes   three  values: Semicolon,  Hash and
    SemicolonOrHash.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.ComponentModel;

namespace System.Ini
{
    /// <summary>
    ///		Defines the comment character in the ini file.
    /// </summary>
    [Flags]
    public enum IniFileCommentCharacter
    {
        /// <summary>
        ///		The comment character is a semicolon (;).
        /// </summary>
        [StringValue(";")]
        Semicolon = 1,

        /// <summary>
        ///		The comment character is a hash (#).
        /// </summary>
        [StringValue("#")]
        Hash = 2,

        /// <summary>
        ///		Comment characters are semicolon or hash mark.
        /// </summary>
        SemicolonOrHash = 3
    }
}