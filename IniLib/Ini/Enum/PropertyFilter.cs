/***************************************************************

•   File: PropertyFilter.cs

•   Description

    The  PropertyFilter enumeration  defines modes for selecting
    which properties of a target  object should be processed. It
    provides options to either process  all  properties  or only
    those   marked  with    a specific attribute,   allowing for
    flexible, attribute-based filtering of properties.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///     Specifies the property filter mode for determining which properties should be processed.
    /// </summary>
    public enum PropertyFilter
    {
        /// <summary>
        ///     All properties of the target class will be processed.
        /// </summary>
        AllProperties,

        /// <summary>
        ///     Only properties marked with a specific attribute will be processed.
        /// </summary>
        MarkedPropertiesOnly
    }
}
