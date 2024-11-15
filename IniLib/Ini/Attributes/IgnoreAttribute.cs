/***************************************************************

•   File: IgnoreAttribute.cs

•   Description

    The IgnoreAttribute  is used  to mark properties that should
    be ignored during operations like reading, writing, or other
    processes in the Initializer class  or its  derived classes.
    This   helps to    exclude  certain  properties   from being
    processed by the initializer.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.Ini
{
    /// <summary>
    ///     Marks a property to be ignored by the Initializer.
    ///     Properties marked with this attribute will be skipped during
    ///     operations like reading or writing settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IgnoreAttribute"/> class.
        /// </summary>
        public IgnoreAttribute() { }
    }
}