/****************************************************************

•   File: IniFileNameAttribute.cs

•   Description

    The IniFileNameAttribute class is an attribute that specifies
    the name of the file with the settings for the assembly.

****************************************************************/

using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Ini
{
    /// <summary>
    ///		Represents the name of the default ini file used to initialize the settings for the <see cref="Assembly"/> to which this attribute is applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Serializable]
    [ComVisible(true)] // Allows the class to be used in COM interop.
    public sealed class IniFileNameAttribute : Attribute
    {
        private string _fileName;

        /// <summary>
        ///		Gets the file name.
        /// </summary>
        public string FileName => _fileName;

        /// <summary>
        ///		Initialize a new instance of the <see cref="IniFileNameAttribute"/> with the specified file name.
        /// </summary>
        /// <param name="fileName"></param>
        public IniFileNameAttribute(string fileName)
        {
            _fileName = fileName;
        }
    }
}