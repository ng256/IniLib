/***************************************************************

•   File: IniFile.cs

•   Description

    The   IniFile class   provides    functionality   to  manage
    configuration files formatted as INI files, allowing for the
    reading, writing, and  modification of settings contained in
    key-value   pairs  organized  under  different sections.  It
    supports custom settings,  culture-specific  operations, and
    optional    extended  type    converters for   enhanced data
    handling.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.InternalTools;

namespace System.Ini
{
    /// <summary>
    ///     Represents the configuration file that contains the application settings
    ///     that are presented in the form of key-value pairs and wrapped into different sections.
    /// </summary>
    
    [DebuggerDisplay("{Content}")]
    public class IniFile : Initializer
    {
        private readonly IniFileParser _parser;

        // Encoding used to encode and decode binary data.
        private BaseEncoding _bytesEncoding;

        private PropertyFilter _filter;

        /// <summary>
        ///     Returns text content of the current <see cref="IniFile"/> instance.
        /// </summary>
        public string Content
        {
            get
            {
                string content = _parser.Content;
                return content;
            }
        }

        #region Constructor

        // Initialize a new instance of the IniFile with the specified content and settings.
        internal IniFile(string content,  IniFileSettings settings) : base(settings)
        {
            _parser = settings.ParsingMethod == IniFileParsingMethod.ReformatFile
                ? new IniFileDictionary(content, settings) as IniFileParser
                : new IniFileRegexParser(content, settings);

            _bytesEncoding = BaseEncoding.GetEncoding(settings.BytesEncoding);
            _filter = settings.PropertyFilter;
        }

        /// <summary>
        ///     Initialize a new empty instance of th <see cref="IniFile"/> with default settings.
        /// </summary>
        public IniFile() : base(IniFileSettings.DefaultSettings)
        {
            _parser = new IniFileDictionary(string.Empty, IniFileSettings.DefaultSettings);
            _bytesEncoding = BaseEncoding.GetEncoding(BytesEncoding.Hexadecimal);
            _filter = PropertyFilter.AllProperties;
        }

        #endregion

        #region Load

        // Retrieves the full path to the INI file name based on the IniFileNameAttribute
        // or derives it from the current executable name, searching in specified directories
        // and creating the file if it does not exist.
        internal static string GetIniFileName(Assembly assembly = null, params string[] customDirectories)
        {
            // Get the current assembly
            if (assembly == null) assembly = Assembly.GetEntryAssembly();
            List<string> directoriesToSearch = new List<string>(customDirectories ?? new string[0])
            {
                Environment.CurrentDirectory
            };

            
            string iniFileName = null;
            string directoryName = Environment.CurrentDirectory;
            if (assembly == null)
            {
                iniFileName = "settings.ini";
            }
            else
            {
                // Check if the IniFileNameAttribute is applied.
                string assemblyLocation = assembly.Location;
                directoryName = Path.GetDirectoryName(assemblyLocation);
                directoriesToSearch.Add(directoryName);

                foreach (object attribute in assembly.GetCustomAttributes(typeof(IniFileNameAttribute), false))
                {
                    if(attribute is IniFileNameAttribute iniFileNameAttrinbute)
                    {
                        iniFileName = Path.GetFileName(iniFileNameAttrinbute.FileName);
                    }
                }

                // Get exe-based file name.
                if (iniFileName.IsNullOrEmpty())
                {
                    iniFileName = Path.GetFileNameWithoutExtension(assemblyLocation) + ".ini";
                }
            }

            // Check the specified directories and other locations
            foreach (string directory in directoriesToSearch.Distinct())
            {
                string filePath = Path.GetFullPath(Path.Combine(directory, iniFileName));
                if (File.Exists(filePath))
                {
                    return filePath; // Return the full path if found
                }
            }

            // Create a new INI file if not found
            string newFilePath = Path.GetFullPath(directoryName == null
                ? iniFileName
                : Path.Combine(directoryName, iniFileName));
            File.OpenWrite(newFilePath).Close();
            return newFilePath;
        }

        // Determines the encoding from the INI file if it is explicitly specified in the file entry or in the BOM.
        internal static Encoding DetectEncoding(string fileName, Encoding defaultEncoding = null)
        {
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);
                if (content.Length > 0)
                {
                    using (IniFile iniFile = new IniFile(content, IniFileSettings.InternalSettings))
                    {
                        var value = iniFile[null, "encoding"];
                        if (value != null)
                            try
                            {
                                return Encoding.GetEncoding(value);
                            }
                            catch
                            {
                                // If fails try to autodetect.
                            }
                    }
                }
            }

            return InternalTools.AutoDetectEncoding(fileName, defaultEncoding ?? Encoding.UTF8);
        }

        /// <summary>
        ///     Loads content from the specified file and create an <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="fileName">
        ///     File name to read the content.
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the INI file.
        /// </param>
        /// <param name="encoding">
        ///     The encoding applied to the contents of the file.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="fileName"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="fileName"/> is empty or contains only whitespace characters,
        ///     or if fileName contains invalid characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when the file does not exist.
        /// </exception>
        public static IniFile Load(string fileName, Encoding encoding, IniFileSettings settings = null)
        {
            if (ValidateFileName(fileName, out string filePath, true) is Exception exception)
                throw exception;

            if (settings == null)
                settings = IniFileSettings.LoadFromFile(fileName);

            if (encoding == null)
                encoding = DetectEncoding(fileName, Encoding.UTF8);

            string content = File.ReadAllText(filePath, encoding);
            return new IniFile(content, settings);
        }

        /// <summary>
        ///     Loads content from the specified file and create an <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="fileName">
        ///     File name to read the content.
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the INI file.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="fileName"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="fileName"/> is empty or contains only whitespace characters,
        ///     or contains invalid characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when the file does not exist.
        /// </exception>

        public static IniFile Load(string fileName, IniFileSettings settings)
        {
            return Load(fileName, encoding: null, settings);
        }

        /// <summary>
        ///     Loads content from the specified file and create an <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="fileName">
        ///     File name to read the content.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="fileName"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="fileName"/> is empty or contains only whitespace characters,
        ///     or contains invalid characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when the file does not exist.
        /// </exception>
        public static IniFile Load(string fileName)
        {
            return Load(fileName, encoding: null, IniFileSettings.DefaultSettings);
        }

        /// <summary>
        ///     Loads the content from the INI file,
        ///     whose name and path depend on the properties of the current assembly,
        ///     and creates an instance of <see cref="IniFile"/>.
        /// </summary>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        public static IniFile Load()
        {
            IniFileSettings settings = IniFileSettings.Load();
            string fileName = GetIniFileName(Assembly.GetEntryAssembly(), Environment.SystemDirectory);
            return Load(fileName, encoding: null, settings);
        }

        /// <summary>
        ///     Loads the content from the INI file,
        ///     whose name and path depend on the properties of the current assembly,
        ///     and creates an instance of <see cref="IniFile"/>.
        /// </summary>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the INI file.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        public static IniFile Load(IniFileSettings settings)
        {
            string fileName = GetIniFileName(Assembly.GetEntryAssembly(), Environment.SystemDirectory);
            return Load(fileName, encoding: null, settings);
        }

        /// <summary>
        ///     Searches for an INI file,
        ///     whose name and path depend on the properties of the current assembly,
        ///     and creates an instance of <see cref="IniFile"/>.
        /// </summary>
        /// <param name="directories">
        ///     A list of directories to search for ini files.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        public static IniFile Search(params string[] directories)
        {
            IniFileSettings settings = IniFileSettings.Load();
            string fileName = GetIniFileName(Assembly.GetEntryAssembly(), directories);
            return Load(fileName, encoding: null, settings);
        }

        /// <summary>
        ///     Searches for an INI file,
        ///     whose name and path depend on the properties of the current assembly,
        ///     and creates an instance of <see cref="IniFile"/>.
        /// </summary>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the INI file.
        /// </param>
        /// <param name="directories">
        ///     A list of directories to search for ini files.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        public static IniFile Search(IniFileSettings settings, params string[] directories)
        {
            string fileName = GetIniFileName(Assembly.GetEntryAssembly(), directories);
            return Load(fileName, encoding: null, settings);
        }

        /// <summary>
        ///     Loads content from the specified stream and create an <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="stream">
        ///     Stream name to read the content.
        /// </param>
        /// <param name="encoding">
        ///     The encoding applied to the contents of the stream.
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the stream.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="stream"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when stream is not readable or does not contain any data to read.
        /// </exception>
        public static IniFile Load(Stream stream, Encoding encoding, IniFileSettings settings = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), GetResourceString("ArgumentNull_Stream"));
            if (!stream.CanRead)
                throw new ArgumentException(GetResourceString("Argument_StreamNotReadable"));
            if (stream.Length == 0)
                throw new ArgumentException(GetResourceString("Serialization_Stream"));
            if (settings == null)
                settings = IniFileSettings.DefaultSettings;

            using (StreamReader reader = new StreamReader(stream, encoding ?? Encoding.UTF8, true))
            {
                return new IniFile(reader.ReadToEnd(), settings);
            }
        }

        /// <summary>
        ///     Loads content from the specified stream and create an <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="stream">
        ///     Stream name to read the content.
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the stream.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="stream"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the parameter <paramref name="stream"/> is readable.
        /// </exception>
        public static IniFile Load(Stream stream, IniFileSettings settings = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), GetResourceString("ArgumentNull_Stream"));
            if (!stream.CanRead)
                throw new ArgumentException(GetResourceString("Argument_StreamNotReadable"));
            if (settings == null)
                settings = IniFileSettings.DefaultSettings;

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return new IniFile(reader.ReadToEnd(), settings);
            }
        }

        /// <summary>
        ///     Loads content from the specified text reader and create an <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="reader">
        ///     Text reader to read the content.
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the stream.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="IniFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="reader"/> is null.
        /// </exception>
        public static IniFile Load(TextReader reader, IniFileSettings settings = null)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (settings == null)
                settings = IniFileSettings.DefaultSettings;

            return new IniFile(reader.ReadToEnd(), settings);
        }

        #endregion

        #region Save

        /// <summary>
        ///     Save content of current <see cref="IniFile"/> instance to the specified file.
        /// </summary>
        /// <param name="fileName">
        ///     Stream name to write the content.
        /// </param>
        /// <param name="encoding">
        ///     The encoding applied to the file content.
        /// </param>
        public void Save(string fileName, Encoding encoding)
        {
            if (ValidateFileName(fileName) is Exception exception)
                throw exception;

            string content = _parser.Content;
            File.WriteAllText(fileName, content, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        ///     Save content of current <see cref="IniFile"/> instance to the specified file.
        /// </summary>
        /// <param name="fileName">
        ///     Stream name to write the content.
        /// </param>
        public void Save(string fileName)
        {
            Save(fileName, Encoding.UTF8);
        }

        /// <summary>
        ///     Save the contents of the current instance of <see cref="IniFile"/>
        ///     to the INI file whose name and path depend on the properties of the current assembly.
        /// </summary>
        public void Save()
        {
            string fileName = GetIniFileName(Assembly.GetEntryAssembly(), Environment.SystemDirectory);
            Save(fileName, Encoding.UTF8);
        }

        /// <summary>
        ///     Save the contents of the current instance of <see cref="IniFile"/>
        ///     to the INI file whose name and path depend on the properties of the current assembly.
        /// </summary>
        /// <param name="encoding">
        ///     The encoding applied to the file content.
        /// </param>
        public void Save(Encoding encoding)
        {
            string fileName = GetIniFileName(Assembly.GetEntryAssembly(), Environment.SystemDirectory);
            Save(fileName, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        ///     Save content of current <see cref="IniFile"/> instance to the specified stream.
        /// </summary>
        /// <param name="stream">
        ///     Stream name to write the content.
        /// </param>
        /// <param name="encoding">
        ///     The encoding applied to the contents of the stream.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when stream is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when stream is not writable.
        /// </exception>
        public void Save(Stream stream, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), GetResourceString("ArgumentNull_Stream"));
            if (!stream.CanWrite)
                throw new ArgumentException(GetResourceString("Argument_StreamNotWritable"));

            using (StreamWriter writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
            {
                string content = _parser.Content;
                writer.Write(content);
            }
        }

        /// <summary>
        ///     Save content of current <see cref="IniFile"/> instance to the specified text writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="writer"/> is null.
        /// </exception>
        public void Save(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            string content = _parser.Content;
            writer.Write(content);
        }

        #endregion

        #region Standart methods

        /// <summary>
        ///     Reads all sections from the INI file.
        /// </summary>
        /// <returns>
        ///     A string array contains all names of sections.
        /// </returns>
        public string[] ReadSections()
        {
            return _parser.GetSections().ToArray();
        }

        /// <summary>
        ///     Reads all keys associated with the specified section from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <returns>
        ///     A string array contains all names of keys associated with the specified section.
        /// </returns>
        public string[] ReadKeys(string section = null)
        {
            return _parser.GetKeys(section).ToArray();
        }

        /// <summary>
        ///     Reads a string associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public string ReadString(string section, string key, string defaultValue = "")
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return _parser.GetValue(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads an array of strings associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValues">
        ///     The values to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public string[] ReadStrings(string section, string key, params string[] defaultValues)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            string[] strings = _parser.GetValues(section, key).ToArray();

            if (strings.Length == 0 && defaultValues != null && defaultValues.Length > 0)
                strings = defaultValues;

            return strings;
        }

        /// <summary>
        ///     Reads an array of strings associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValues">
        ///     The values to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public char[] ReadChars(string section, string key, params char[] defaultValues)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            char[] chars = _parser.GetValue(section, key, string.Empty).ToCharArray();

            if (chars.Length == 0 && defaultValues != null && defaultValues.Length > 0)
                chars = defaultValues;

            return chars;
        }

        /// <summary>
        ///     Reads an array of bytes associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValues">
        ///     The values to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public byte[] ReadBytes(string section, string key, params byte[] defaultValues)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                // Convert the string values to a byte array using the configured encoding.
                string value = _parser.GetValue(section, key, string.Empty);
                byte[] bytes = _bytesEncoding.GetBytes(value);

                return bytes;
            }

            catch
            {
                return defaultValues ?? Empty<byte>.Array;
            }
        }

        /// <summary>
        ///     Reads a value associated with the specified section and key from the ini file and converts it to the specified type.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="type">
        ///     The desired value type.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a value. If it is null, the default converter will be used.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters <paramref name="key"/> or <paramref name="type"/> is null.
        /// </exception>
        public object ReadObject(string section, string key, Type type,
            object defaultValue = default, TypeConverter converter = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(string))
                return _parser.GetValue(section, key) ?? defaultValue;

            // If the type is an array, read the array values.
                if (type.IsArray)
                return type.GetArrayRank() == 1
                    ? ReadArray(section, key, type.GetElementType(), converter)
                    : null;

            // Attempt to read the string value from the ini file for the given section and key.
            string value = _parser.GetValue(section, key);
            object result = ConvertFromString(type, value, defaultValue, converter);

            return result;
        }

        /// <summary>
        ///     Reads a values associated with the specified section and key from the INI file
        ///     and converts it to the specified type.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="elementType">
        ///     Desired value type.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <returns>
        ///     Read array.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of parameters <paramref name="key"/> or <paramref name="elementType"/> is null.
        /// </exception>
        public Array ReadArray(string section, string key, Type elementType, TypeConverter converter = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (elementType == null)
                throw new ArgumentNullException(nameof(elementType));

            // Handle common array types.
            if (elementType == typeof(string))
                return _parser.GetValues(section, key).ToArray();

            if (elementType == typeof(byte))
                return ReadBytes(section, key);

            if (elementType == typeof(char))
                return _parser.GetValue(section, key, string.Empty).ToCharArray();

            // Get the values associated with the specified key.
            string[] values = _parser.GetValues(section, key).ToArray();
            Array array = ConvertFromStrings(elementType, values, converter);

            return array;
        }



        /// <summary>
        ///     Writes a string associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteString(string section, string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _parser.SetValue(section, key, value);
        }

        /// <summary>
        ///     Writes a strings associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="values">
        ///     The values to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteStrings(string section, string key, params string[] values)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                return;

            _parser.SetValues(section, key, values);
        }

        /// <summary>
        ///     Writes a char array as string associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="values">
        ///     The values to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteChars(string section, string key, params char[] values)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                return;

            _parser.SetValue(section, key, new string(values));
        }

        /// <summary>
        ///     Writes a strings associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="bytes">
        ///     The bytes to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteBytes(string section, string key, params byte[] bytes)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return;

            try
            {
                string value = _bytesEncoding.GetString(bytes);
                _parser.SetValue(section, key, value);
            }
            catch
            {
                // If encoding fails, there is nothing to do.
            }
        }

        /// <summary>
        ///     Writes a value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <param name="converter">
        ///     Optional. A type converter used to convert a value. If it is null, the default converter will be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteObject(string section, string key, object value, TypeConverter converter = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // Handle common types.
            switch (value)
            {
                case null:
                    return;

                case string str:
                    _parser.SetValue(section, key, str);
                    return;

                // If the value is an array, call the WriteArray method.
                case Array array:
                    if(array.Rank == 1)
                        WriteArray(section, key, array, converter);
                    return;
            }

            // If a converter is not provided, get the default converter for the type.
            Type type = value.GetType();
            if (converter == null)
                converter = Converters.Get(type);

            string result = ConvertToString(value, converter);
            _parser.SetValue(section, key, result);
        }

        /// <summary>
        ///     Writes a values associated with the specified section and key to the INI file.
        ///     and converts it to the specified type.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="array">
        ///     The array to be written.
        /// </param>
        /// <param name="converter">
        ///     Optional. A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of parameters <paramref name="key"/> or <paramref name="array"/> is null.
        /// </exception>
        public void WriteArray(string section, string key, Array array, TypeConverter converter = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Handle common array types.
            switch (array)
            {
                case string[] strings:
                    _parser.SetValues(section, key, strings);
                    return;

                case byte[] bytes:
                    WriteBytes(section, key, bytes);
                    return;

                case char[] chars:
                    _parser.SetValues(section, key, new string(chars));
                    return;
            }

            string[] values = ConvertToStrings(array, converter);
            _parser.SetValues(section, key, values);
        }

        #endregion

        #region Reflection methods

        // Processes a property by extracting its section and key using attributes or defaults,
        // then invokes the specified action (e.g., RestorePropertyValue or StorePropertyValue).
        private void ProcessProperty(PropertyInfo property, object obj,
            Action<string, string, PropertyInfo, object, TypeConverter> action)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            string section = null;
            string key = null;
            TypeConverter converter = null;

            // Process attributes.
            foreach (object attribute in property.GetCustomAttributes(false))
            {
                switch (attribute)
                {
                    case SectionAttribute sectionAttribute when section == null:
                        section = sectionAttribute.Name;
                        break;
                    case EntryAttribute entryAttribute:
                        key = entryAttribute.Name;
                        break;
                    case IgnoreAttribute _:
                        return;
                    case TypeConverterAttribute converterAttribute:
                        converter = converterAttribute.CreateInstance() as TypeConverter;
                        break;
                }
            }

            // If section is still null, try to get it from the declaring type attributes.
            Type declaringType = property.DeclaringType;
            if (declaringType == null) return;

            if (section == null)
            {
                foreach (object attribute in declaringType.GetCustomAttributes(false))
                {
                    if (attribute is SectionAttribute sectionAttribute)
                    {
                        section = sectionAttribute.Name;
                    }
                }
            }

            // If no custom section is specified on the property, use the declaring type name as the default section name.
            if (section == null)
            {
                if(_filter == PropertyFilter.AllProperties)
                    section = declaringType.GetDeclaringPath();
                else
                    return;
            }
            
            // If no custom key name is specified, use the property name as the default key.
            if (key == null)
            {
                if (_filter == PropertyFilter.AllProperties)
                    key = property.Name;
                else
                    return;
            }

            // Check if the property is read-only.
            if (!property.CanWrite || property.GetCustomAttributes(typeof(IgnoreAttribute), false)
                    .FirstOrDefault() is IgnoreAttribute ignoreAttribute)
                return;

            action?.Invoke(section, key, property, obj, converter);
        }

        /// <summary>
        ///     Restores the property value associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="property">
        ///     Property to initialize.
        /// </param>
        /// <param name="obj">
        ///     The object whose property value will be set.
        ///     Pass null to use a static property.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of parameters <paramref name="key"/> or <paramref name="property"/> is null.
        /// </exception>
        protected void RestorePropertyValue(string section, string key, PropertyInfo property, object obj, TypeConverter converter = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // If the property is static, the object instance is not required.
            if (property.IsStatic())
                obj = null;

            // If the property is not static and the object instance is null, exit the method.
            else if (obj == null)
                return;

            // Determine the type of the property.
            Type propertyType = property.PropertyType;

            // Read the value associated with the specified key.
            object value = ReadObject(section, key, propertyType, converter);

            // If the value is null or empty, do not set the property.
            if (value == null
                || (value is string str && str.Length == 0)
                || (value is Array arr && arr.Length == 0))
                return;

            // Set the property value on the specified object.
            property.SetValue(obj, value);
        }


        /// <summary>
        ///     Restores the property value associated with a section and key from an INI file based on attributes.
        /// </summary>
        /// <param name="property">
        ///     The property to restore from the INI file.
        /// </param>
        /// <param name="obj">
        ///     Optional. The object whose property will be set. Pass null for static context.
        ///     Pass null to use a static property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="property"/> is null.
        /// </exception>
        protected override void RestorePropertyValue(PropertyInfo property, object obj = null)
        {
            ProcessProperty(property, obj, RestorePropertyValue);
        }

        /// <summary>
        ///     Stores the property value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="property">
        ///     Property to store.
        /// </param>
        /// <param name="obj">
        ///     The object whose property value will be get.
        ///     Pass null to use a static property.
        /// </param>
        /// <param name="converter">
        ///     Optional. A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of parameters <paramref name="key"/> or <paramref name="property"/> is null.
        /// </exception>
        protected void StorePropertyValue(string section, string key, PropertyInfo property, object obj, TypeConverter converter = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // If the property is static, the object instance is not required.
            if (property.IsStatic())
                obj = null;

            // If the property is not static and the object instance is null, exit the method.
            else if (obj == null)
                return;

            // Retrieve the current value of the property from the object instance.
            object value = property.GetValue(obj);

            // If the value is null, an empty string, or an empty array, do not proceed with storing the value.
            if (value == null
                || (value is string str && str.Length == 0) // Check for empty strings.
                || (value is Array arr && arr.Length == 0)) // Check for empty arrays.
                return;

            // Write the value to the INI structure using the provided section, key, and optional converter.
            WriteObject(section, key, value, converter);
        }

        /// <summary>
        ///     Stores the property value associated with a section and key to the INI file based on attributes.
        /// </summary>
        /// <param name="property">
        ///     The property to store to the INI file.
        /// </param>
        /// <param name="obj">
        ///     Optional. The object whose property will be set.
        ///     Pass null to use a static property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="property"/> is null.
        /// </exception>
        protected override void StorePropertyValue(PropertyInfo property, object obj = null)
        {
            ProcessProperty(property, obj, StorePropertyValue);
        }

        #endregion

        #region Generic methods

        /// <summary>
        ///     Reads a value associated with the specified section and key from the INI file
        ///     and converts it to the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     Desired value type.
        /// </typeparam>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public T Read<T>(string section, string key, T defaultValue = default)
        {
            return Read(section, key, converter: null, defaultValue);
        }

        /// <summary>
        ///     Reads a value associated with the specified section and key from the INI file
        ///     and converts it to the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     Desired value type.
        /// </typeparam>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a value. If it is null, the default converter will be used.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public T Read<T>(string section, string key, TypeConverter converter, T defaultValue = default)
        {
            if (converter == null)
                converter = Converters.Get(typeof(T));

            return ReadObject(section, key, typeof(T), converter) is T value ? value : defaultValue;
        }

        /// <summary>
        ///     Reads a values associated with the specified section and key from the INI file
        ///     and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">
        ///     Desired type of array elements.
        /// </typeparam>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <param name="defaultValues">
        ///     The values to be returned if the specified key is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public T[] ReadArray<T>(string section, string key, TypeConverter converter, params T[] defaultValues)
        {
            Array array = ReadArray(section, key, typeof(T), converter);

            if (array.Length > 0 && array is T[] t)
                return t;

            return defaultValues ?? Empty<T>.Array;
        }

        /// <summary>
        ///     Reads a values associated with the specified section and key from the INI file
        ///     and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">
        ///     Desired type of array elements.
        /// </typeparam>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValues">
        ///     The values to be returned if the specified key is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public T[] ReadArray<T>(string section, string key, params T[] defaultValues)
        {
            return ReadArray(section, key, converter: null, defaultValues);
        }

        /// <summary>
        ///     Writes a value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a value. If it is null, the default converter will be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void Write<T>(string section, string key, T value, TypeConverter converter)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            WriteObject(section, key, value, converter);
        }

        /// <summary>
        ///     Writes a value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void Write<T>(string section, string key, T value)
        {
            Write(section, key, value, converter: null);
        }

        /// <summary>
        ///     Writes a values associated with the specified section and key to the INI file.
        ///     and converts it to the specified type.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="array">
        ///     The array to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of parameters <paramref name="key"/> or <paramref name="array"/> is null.
        /// </exception>
        public void WriteArray<T>(string section, string key, params T[] array)
        {
            WriteArray(section, key, (Array)array);
        }

        /// <summary>
        ///     Writes a values associated with the specified section and key to the INI file.
        ///     and converts it to the specified type.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="array">
        ///     The array to be written.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of parameters <paramref name="key"/> or <paramref name="array"/> is null.
        /// </exception>
        public void WriteArray<T>(string section, string key, T[] array, TypeConverter converter)
        {
            WriteArray(section, key, (Array)array, converter);
        }

        #endregion

        #region Indexer

        /// <summary>
        ///     Reads or writes the value associated with the specified name associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <returns>
        ///     The value associated with the specified section and key.
        ///     If the specified entry is not found, attempting to get it returns the empty string,
        ///     and attempting to set it creates a new entry using the specified name.
        /// </returns>
        public string this[string section, string key]
        {
            get => ReadString(section, key, string.Empty);
            set => WriteString(section, key, value);
        }

        /// <summary>
        ///     Reads or writes the value associated with the specified name.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     The value associated with the specified name.
        ///     If the specified entry is not found, attempting to get it returns the <paramref name="defaultValue"/>,
        ///     and attempting to set it creates a new entry using the specified name.
        /// </returns>
        public string this[string section, string key, string defaultValue]
        {
            get => ReadString(section, key, defaultValue);
        }

        #endregion

        #region Additional methods

        /// <summary>
        ///     Reads a boolean value associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public bool ReadBoolean(string section, string key, bool defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a character associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public char ReadChar(string section, string key, char defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a signed byte associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public sbyte ReadSByte(string section, string key, sbyte defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads an unsigned byte associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public byte ReadByte(string section, string key, byte defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a 16-bit integer associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public short ReadInt16(string section, string key, short defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads an unsigned 16-bit integer associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public ushort ReadUInt16(string section, string key, ushort defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a 32-bit integer associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public int ReadInt32(string section, string key, int defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads an unsigned 32-bit integer associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public uint ReadUInt32(string section, string key, uint defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a 64-bit integer associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public long ReadInt64(string section, string key, long defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads an unsigned 64-bit integer associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public ulong ReadUInt64(string section, string key, ulong defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a 32-bit floating point value associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public float ReadSingle(string section, string key, float defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a 64-bit floating point value associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public double ReadDouble(string section, string key, double defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a decimal value associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public decimal ReadDecimal(string section, string key, decimal defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Reads a <see cref="DateTime"/> value associated with the specified section and key from the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to get global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to be returned if the specified entry is not found.
        /// </param>
        /// <returns>
        ///     Read value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public DateTime ReadDateTime(string section, string key, DateTime defaultValue = default)
        {
            return Read(section, key, defaultValue);
        }

        /// <summary>
        ///     Writes a boolean value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteBoolean(string section, string key, bool value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a character value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteChar(string section, string key, char value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a signed byte associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteSByte(string section, string key, sbyte value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes an unsigned byte associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteByte(string section, string key, byte value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a signed 16-bit integer associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteInt16(string section, string key, short value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes an unsigned 16-bit integer associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteUInt16(string section, string key, ushort value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a signed 32-bit integer associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteInt32(string section, string key, int value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes an unsigned 32-bit integer associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteUInt32(string section, string key, uint value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a signed 64-bit integer associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteInt64(string section, string key, long value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes an unsigned 64-bit integer associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteUInt64(string section, string key, ulong value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a 32-bit floating point value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteSingle(string section, string key, float value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a 64-bit floating point value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteDouble(string section, string key, double value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a decimal value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteDecimal(string section, string key, decimal value)
        {
            Write(section, key, value);
        }

        /// <summary>
        ///     Writes a <see cref="DateTime"/> value associated with the specified section and key to the INI file.
        /// </summary>
        /// <param name="section">
        ///     Section name. Pass null to set global entries above all sections.
        /// </param>
        /// <param name="key">
        ///     Key name.
        /// </param>
        /// <param name="value">
        ///     The value to be written.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when parameter <paramref name="key"/> is null.
        /// </exception>
        public void WriteDateTime(string section, string key, DateTime value)
        {
            Write(section, key, value);
        }

        #endregion

        #region Dispose method

        /// <summary>
        ///     Releases resources used by the Initializer.
        /// </summary>
        /// <param name="disposing">
        ///     True if disposing is requested, false otherwise.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            _parser.Dispose();
        }

        #endregion
    }
}