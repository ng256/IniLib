/***************************************************************

•   File: IniFileParsingMethod.cs

•   Description

    The  IniFileParsingMethod  class describes    how   data  is
    formatted  when  written  to  an ini file.  This enumeration
    specifies two ways for parsing  data:  reformat the  file or
    preserve the original format. The  method you choose depends
    on  your specific requirements for  preserving  the original
    formatting  or  providing    the   best data   quality while
    maintaining speed.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///		Defines the parsing ini file data method.
    /// </summary>
    public enum IniFileParsingMethod
    {
        /// <summary>
        ///		Preserves the original formatting style, including indentation, whitespace, and unrecognized data.
        ///		This method is slower, use it if it is very important to preserve the original formatting.
        /// </summary>
        PreserveOriginal = 0,

        /// <summary>
        ///		Reformats data before writing to a file.
        ///		In this case, all entries will be formatted according to the ini file standard, and all unrecognized data will be lost.
        ///		This method allows you to get the best data quality in the minimum time, however, it leads to a loss of user marking of the file.
        /// </summary>
        ReformatFile = 1,

        /// <summary>
        ///		This method does not use data caching and does not change the file format.
        ///		It provides high performance with a small number of reading and writing operations,
        ///		but can be slow with a large number of iterations.
        /// </summary>
        QuickScan = 2,

        /// <summary>
        ///		This method parses the ini file by calling Windows API functions. 
        /// </summary>
        WinAPI = 3
    }
}