using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace System.Ini
{
    /// <summary>
    ///     Represents a JSON file with methods to read and write properties of various types.
    /// </summary>
    [DebuggerDisplay("{Content}")]
    public class JsonFile
    {
        private readonly JsonParser _parser;
        private BaseEncoding _bytesEncoding;
        private PropertyFilter _filter;

        /// <summary>
        ///     Gets the raw JSON content.
        /// </summary>
        public string Content
        {
            get { return _parser.Content; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonFile"/> class.
        /// </summary>
        /// <param name="content">
        ///     The JSON content to parse
        /// </param>
        /// <param name="settings">
        ///     Settings used for parsing and encoding
        /// </param>
        internal JsonFile(string content, JsonFileSettings settings)
        {
            _parser = JsonParser.Create(content, settings);
            _bytesEncoding = BaseEncoding.GetEncoding(settings.BytesEncoding);
            _filter = settings.PropertyFilter;
        }

        #region Load Methods

        /// <summary>
        ///     Loads a JSON file from the specified file path using the given encoding and settings.
        /// </summary>
        /// <param name="fileName">
        ///     The path to the JSON file
        /// </param>
        /// <param name="encoding">
        ///     The encoding to use when reading the file. Defaults to UTF8 if null
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the JSON file. Defaults to <see cref="JsonFileSettings.DefaultSettings"/>/// </param>
        /// <returns>
        ///     A <see cref="JsonFile"/> instance
        /// </returns>
        public static JsonFile Load(string fileName, Encoding encoding = null, JsonFileSettings settings = null)
        {
            encoding ??= Encoding.UTF8;
            settings ??= JsonFileSettings.DefaultSettings;
            string content = File.ReadAllText(fileName, encoding);
            return new JsonFile(content, settings);
        }

        /// <summary>
        ///     Loads a JSON file from the specified stream using the given encoding and settings.
        /// </summary>
        /// <param name="stream">
        ///     The stream containing the JSON data.
        /// </param>
        /// <param name="encoding">
        ///     The encoding to use when reading the stream. Defaults to UTF8 if null.
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the JSON file. Defaults to <see cref="JsonFileSettings.DefaultSettings"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="JsonFile"/> instance.
        /// </returns>
        public static JsonFile Load(Stream stream, Encoding encoding = null, JsonFileSettings settings = null)
        {
            settings ??= JsonFileSettings.DefaultSettings;
            using (StreamReader reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
            {
                string content = reader.ReadToEnd();
                return new JsonFile(content, settings ?? JsonFileSettings.InternalDefaultSettings);
            }
        }

        /// <summary>
        ///     Loads a JSON file from the specified <see cref="TextReader"/> using the given settings.
        /// </summary>
        /// <param name="reader">
        ///     The <see cref="TextReader"/> to read the JSON data from
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the JSON file. Defaults to <see cref="JsonFileSettings.DefaultSettings"/>/// </param>
        /// <returns>
        ///     A <see cref="JsonFile"/> instance
        /// </returns>
        public static JsonFile Load(TextReader reader, JsonFileSettings settings = null)
        {
            string content = reader.ReadToEnd();
            return new JsonFile(content, settings ?? JsonFileSettings.InternalDefaultSettings);
        }

        /// <summary>
        ///     Loads a JSON file from the specified file path using UTF8 encoding and the given settings.
        /// </summary>
        /// <param name="fileName">
        ///     The path to the JSON file
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the JSON file. Defaults to <see cref="JsonFileSettings.DefaultSettings"/>/// </param>
        /// <returns>
        ///     A <see cref="JsonFile"/> instance
        /// </returns>
        public static JsonFile Load(string fileName, JsonFileSettings settings = null)
        {
            return Load(fileName, null, settings);
        }

        /// <summary>
        ///     Loads a JSON file from the specified stream using UTF8 encoding and the given settings.
        /// </summary>
        /// <param name="stream">
        ///     The stream containing the JSON data
        /// </param>
        /// <param name="settings">
        ///     Settings that used for parsing and formatting the JSON file. Defaults to <see cref="JsonFileSettings.DefaultSettings"/>/// </param>
        /// <returns>
        ///     A <see cref="JsonFile"/> instance
        /// </returns>
        public static JsonFile Load(Stream stream, JsonFileSettings settings = null)
        {
            return Load(stream, null, settings);
        }

        #endregion

        #region Save Methods

        /// <summary>
        ///     Saves the JSON file to the specified file path using the given encoding and settings.
        /// </summary>
        /// <param name="fileName">
        ///     The path to save the JSON file
        /// </param>
        /// <param name="encoding">
        ///     The encoding to use when writing the file. Defaults to UTF8 if null
        /// </param>
        public void Save(string fileName, Encoding encoding = null)
        {
            File.WriteAllText(fileName, Content, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        ///     Saves the JSON file to the specified stream using the given encoding and settings.
        /// </summary>
        /// <param name="stream">
        ///     The stream to write the JSON data to
        /// </param>
        /// <param name="encoding">
        ///     The encoding to use when writing to the stream. Defaults to UTF8 if null
        /// </param>
        public void Save(Stream stream, Encoding encoding = null)
        {
            using (StreamWriter writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
            {
                writer.Write(Content);
            }
        }

        #endregion

        /// <summary>
        ///     Reads a value of the specified type associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the value.
        /// </param>
        /// <param name="type">
        ///     The type of the value to read.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <param name="converter">
        ///     Optional type converter to convert the value.
        /// </param>
        /// <returns>
        ///     The value at the specified path, or the default value.
        /// </returns>
        public object ReadObject(string path, Type type,
            object defaultValue = default, TypeConverter converter = null)
        {
            return defaultValue;
        }

        /// <summary>
        ///     Writes an object associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the object will be written.
        /// </param>
        /// <param name="value">
        ///     The value to write.
        /// </param>
        /// <param name="converter">
        ///     Optional type converter to convert the value before writing.
        /// </param>
        public void WriteObject(string path, object value, TypeConverter converter = null)
        {
        }

        /// <summary>
        ///     Reads an array of objects associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the array.
        /// </param>
        /// <param name="elementType">
        ///     The type of the array elements.
        /// </param>
        /// <param name="converter">
        ///     Optional type converter to convert the values.
        /// </param>
        /// <returns>
        ///     The array of values at the specified path, or <c>
        ///     null</c> if not found.
        /// </returns>
        public Array ReadArray(string path, Type elementType, TypeConverter converter = null)
        {
            return null;
        }

        /// <summary>
        ///     Writes an array of objects associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the array will be written.
        /// </param>
        /// <param name="array">
        ///     The array of values to write.
        /// </param>
        /// <param name="converter">
        ///     Optional type converter to convert the values before writing.
        /// </param>
        public void WriteArray(string path, Array array, TypeConverter converter = null)
        {
        }


        /// <summary>
        ///     Reads a value of the specified type associated with the specified path from the JSON File.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value to read.
        /// </typeparam>
        /// <param name="path">
        ///     The JSON path to the value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <returns>
        ///     The value at the specified path, or the default value.
        /// </returns>
        public T Read<T>(string path, T defaultValue, TypeConverter converter = null)
        {

            return (T)ReadObject(path, typeof(T), defaultValue, converter);
        }

        /// <summary>
        ///     Writes a value of the specified type associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value to write.
        /// </typeparam>
        /// <param name="path">
        ///     The JSON path where the value will be written.
        /// </param>
        /// <param name="value">
        ///     The value to write.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        public void Write<T>(string path, T value, TypeConverter converter = null)
        {
            WriteObject(path, value, converter);
        }

        /// <summary>
        ///     Reads an array of the specified type associated with the specified path from the JSON File.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the array elements.
        /// </typeparam>
        /// <param name="path">
        ///     The JSON path to the array.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>
        /// <returns>
        ///     The array of values at the specified path, or an empty array if not found.
        /// </returns>
        public T[] ReadArray<T>(string path, TypeConverter converter = null)
        {

            return (T[])ReadArray(path, typeof(T), converter);
        }

        /// <summary>
        ///     Writes an array of the specified type associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the array elements.
        /// </typeparam>
        /// <param name="path">
        ///     The JSON path where the array will be written.
        /// </param>
        /// <param name="array">
        ///     The array of values to write.
        /// </param>
        /// <param name="converter">
        ///     A type converter used to convert a values. If it is null, the default converter will be used.
        /// </param>

        public void WriteArray<T>(string path, T[] array, TypeConverter converter = null)
        {
            WriteArray(path, (Array)array, converter);
        }

        /// <summary>
        ///     Reads a string value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the string value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The string value at the specified path, or the default value.
        /// </returns>
        public string ReadString(string path, string defaultValue)
        {
            return Read<string>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a string value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the string value will be written.
        /// </param>
        /// <param name="value">
        ///     The string value to write.
        /// </param>
        public void WriteString(string path, string value)
        {
            Write<string>(path, value);
        }

        /// <summary>
        ///     Reads an array of strings associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the array of strings.
        /// </param>
        /// <returns>
        ///     The array of strings at the specified path.
        /// </returns>
        public string[] ReadStrings(string path)
        {
            return ReadArray<string>(path);
        }

        /// <summary>
        ///     Writes an array of strings associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the array of strings will be written.
        /// </param>
        /// <param name="value">
        ///     The array of strings to write.
        /// </param>
        public void WriteStrings(string path, string[] value)
        {
            WriteArray<string>(path, value);
        }

        /// <summary>
        ///     Reads an array of bytes associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the array of bytes.
        /// </param>
        /// <returns>
        ///     The array of bytes at the specified path.
        /// </returns>
        public byte[] ReadBytes(string path)
        {
            return ReadArray<byte>(path);
        }

        /// <summary>
        ///     Writes an array of bytes associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the array of bytes will be written.
        /// </param>
        /// <param name="bytes">
        ///     The array of bytes to write.
        /// </param>
        public void WriteBytes(string path, byte[] bytes)
        {
            Write<byte[]>(path, bytes);
        }

        /// <summary>
        ///     Reads an array of characters associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the array of characters.
        /// </param>
        /// <returns>
        ///     The array of characters at the specified path.
        /// </returns>
        public char[] ReadChars(string path)
        {
            return ReadArray<char>(path);
        }

        /// <summary>
        ///     Writes an array of characters associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the array of characters will be written.
        /// </param>
        /// <param name="chars">
        ///     The array of characters to write.
        /// </param>
        public void WriteChars(string path, char[] chars)
        {
            WriteArray<char>(path, chars);
        }

        /// <summary>
        ///     Reads a boolean value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the boolean value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The boolean value at the specified path, or the default value.
        /// </returns>
        public bool ReadBoolean(string path, bool defaultValue)
        {
            return Read<bool>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a boolean value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the boolean value will be written.
        /// </param>
        /// <param name="value">
        ///     The boolean value to write.
        /// </param>
        public void WriteBoolean(string path, bool value)
        {
            Write<bool>(path, value);
        }

        /// <summary>
        ///     Reads a character value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the character value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The character value at the specified path, or the default value.
        /// </returns>
        public char ReadChar(string path, char defaultValue)
        {
            return Read<char>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a character value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the character value will be written.
        /// </param>
        /// <param name="value">
        ///     The character value to write.
        /// </param>
        public void WriteChar(string path, char value)
        {
            Write<char>(path, value);
        }

        /// <summary>
        ///     Reads a byte value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the byte value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The byte value at the specified path, or the default value.
        /// </returns>
        public byte ReadByte(string path, byte defaultValue)
        {
            return Read<byte>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a byte value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the byte value will be written.
        /// </param>
        /// <param name="value">
        ///     The byte value to write.
        /// </param>
        public void WriteByte(string path, byte value)
        {
            Write<byte>(path, value);
        }


        /// <summary>
        ///     Reads an unsigned byte value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the unsigned byte value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found
        /// </param>
        /// <returns>
        ///     The unsigned byte value at the specified path, or the default value.
        /// </returns>
        public sbyte ReadSByte(string path, sbyte defaultValue)
        {
            return Read<sbyte>(path, defaultValue);
        }

        /// <summary>
        ///     Writes an unsigned byte value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the unsigned byte value will be written.
        /// </param>
        /// <param name="value">
        ///     The unsigned byte value to write.
        /// </param>
        public void WriteSByte(string path, sbyte value)
        {
            Write<sbyte>(path, value);
        }

        /// <summary>
        ///     Reads a 16-bit integer value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the 16-bit integer value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The 16-bit integer value at the specified path, or the default value.
        /// </returns>
        public short ReadInt16(string path, short defaultValue)
        {
            return Read<short>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a 16-bit integer value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the 16-bit integer value will be written.
        /// </param>
        /// <param name="value">
        ///     The 16-bit integer value to write.
        /// </param>
        public void WriteInt16(string path, short value)
        {
            Write<short>(path, value);
        }

        /// <summary>
        ///     Reads an unsigned 16-bit integer value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the unsigned 16-bit integer value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The unsigned 16-bit integer value at the specified path, or the default value.
        /// </returns>
        public ushort ReadUInt16(string path, ushort defaultValue)
        {
            return Read<ushort>(path, defaultValue);
        }

        /// <summary>
        ///     Writes an unsigned 16-bit integer value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the unsigned 16-bit integer value will be written.
        /// </param>
        /// <param name="value">
        ///     The unsigned 16-bit integer value to write.
        /// </param>
        public void WriteUInt16(string path, ushort value)
        {
            Write<ushort>(path, value);
        }


        /// <summary>
        ///     Reads a 32-bit integer value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the 32-bit integer value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The 32-bit integer value at the specified path, or the default value.
        /// </returns>
        public int ReadInt32(string path, int defaultValue)
        {
            return Read<int>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a 32-bit integer value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the 32-bit integer value will be written.
        /// </param>
        /// <param name="value">
        ///     The 32-bit integer value to write.
        /// </param>
        public void WriteInt32(string path, int value)
        {
            Write<int>(path, value);
        }

        /// <summary>
        ///     Reads an unsigned 32-bit integer value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the unsigned 32-bit integer value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The unsigned 32-bit integer value at the specified path, or the default value.
        /// </returns>
        public uint ReadUInt32(string path, uint defaultValue)
        {
            return Read<uint>(path, defaultValue);
        }

        /// <summary>
        ///     Writes an unsigned 32-bit integer value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the unsigned 32-bit integer value will be written.
        /// </param>
        /// <param name="value">
        ///     The unsigned 32-bit integer value to write.
        /// </param>
        public void WriteUInt32(string path, uint value)
        {
            Write<uint>(path, value);
        }

        /// <summary>
        ///     Reads a 64-bit integer value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the 64-bit integer value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The 64-bit integer value at the specified path, or the default value.
        /// </returns>
        public long ReadInt64(string path, long defaultValue)
        {
            return Read<long>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a 64-bit integer value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the 64-bit integer value will be written.
        /// </param>
        /// <param name="value">
        ///     The 64-bit integer value to write.
        /// </param>
        public void WriteInt64(string path, long value)
        {
            Write<long>(path, value);
        }


        /// <summary>
        ///     Reads an unsigned 64-bit integer value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the unsigned 64-bit integer value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The unsigned 64-bit integer value at the specified path, or the default value.
        /// </returns>
        public ulong ReadUInt64(string path, ulong defaultValue)
        {
            return Read<ulong>(path, defaultValue);
        }

        /// <summary>
        ///     Writes an unsigned 64-bit integer value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the unsigned 64-bit integer value will be written.
        /// </param>
        /// <param name="value">
        ///     The unsigned 64-bit integer value to write.
        /// </param>
        public void WriteUInt64(string path, ulong value)
        {
            Write<ulong>(path, value);
        }

        /// <summary>
        ///     Reads a 32-bit floating point value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the 32-bit floating point value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The 32-bit floating point value at the specified path, or the default value.
        /// </returns>
        public float ReadSingle(string path, float defaultValue)
        {
            return Read<float>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a 32-bit floating point value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the 32-bit floating point value will be written.
        /// </param>
        /// <param name="value">
        ///     The 32-bit floating point value to write.
        /// </param>
        public void WriteSingle(string path, float value)
        {
            Write<float>(path, value);
        }

        /// <summary>
        ///     Reads a 64-bit floating point value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the 64-bit floating point value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The 64-bit floating point value at the specified path, or the default value.
        /// </returns>
        public double ReadDouble(string path, double defaultValue)
        {
            return Read<double>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a 64-bit floating point value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the 64-bit floating point value will be written.
        /// </param>
        /// <param name="value">
        ///     The 64-bit floating point value to write.
        /// </param>
        public void WriteDouble(string path, double value)
        {
            Write<double>(path, value);
        }


        /// <summary>
        ///     Reads a decimal value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the decimal value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The decimal value at the specified path, or the default value.
        /// </returns>
        public decimal ReadDecimal(string path, decimal defaultValue)
        {
            return Read<decimal>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a decimal value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the decimal value will be written.
        /// </param>
        /// <param name="value">
        ///     The decimal value to write.
        /// </param>
        public void WriteDecimal(string path, decimal value)
        {
            Write<decimal>(path, value);
        }

        /// <summary>
        ///     Reads a DateTime value associated with the specified path from the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path to the DateTime value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value if the path is not found.
        /// </param>
        /// <returns>
        ///     The DateTime value at the specified path, or the default value.
        /// </returns>
        public DateTime ReadDateTime(string path, DateTime defaultValue)
        {
            return Read<DateTime>(path, defaultValue);
        }

        /// <summary>
        ///     Writes a DateTime value associated with the associated with the specified path to the JSON File.
        /// </summary>
        /// <param name="path">
        ///     The JSON path where the DateTime value will be written.
        /// </param>
        /// <param name="value">
        ///     The DateTime value to write.
        /// </param>
        public void WriteDateTime(string path, DateTime value)
        {
            Write<DateTime>(path, value);
        }
    }
}