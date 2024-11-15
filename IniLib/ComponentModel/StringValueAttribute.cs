/***************************************************************

•   File: StringValueAttribute.cs

•   Description

    The StringValueAttribute  stores the string value associated
    with the field, and the  constructor  allows you to set this
    value   when  creating    an  instance  of    the attribute.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

namespace System.ComponentModel
{
    // This attribute is used to associate a string value with a field
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class StringValueAttribute : Attribute
    {
        private string _stringValue;

        // The string value associated with the field
        public string StringValue
        {
            get => _stringValue;
            private set => _stringValue = value;
        }

        // Initialize a new instance of the StringValueAttribute with the specified string value.
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }
    }
}