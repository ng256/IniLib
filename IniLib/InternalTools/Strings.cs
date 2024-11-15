/***************************************************************

•   File: Strings.cs

•   Description
    This code snippet is a set of static extension methods for
    strings that can be useful when working with text data.

•   Copyright

    © Pavel Bashkardin, 2022-2024

***************************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Ini;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace System
{
    internal static partial class InternalTools
    {
        // Represents the current date and time, with an option to display only the date.
        internal class DateTimeNow
        {
            private readonly bool _onlyDate;

            public DateTimeNow(bool onlyDate = false)
            {
                _onlyDate = onlyDate;
            }

            public override string ToString()
            {
                DateTime dateTime = DateTime.Now;
                return _onlyDate
                    ? dateTime.ToString("d", CultureInfo.CurrentCulture)
                    : dateTime.ToString("g", CultureInfo.CurrentCulture);
            }
        }

        // Represents a new line character.
        internal class NewLine
        {
            private readonly LineBreakerStyle _lineBreaker;

            public NewLine(LineBreakerStyle lineBreaker = LineBreakerStyle.Default)
            {
                _lineBreaker = lineBreaker;
            }

            public override string ToString()
            {
                return _lineBreaker.GetString();
            }
        }

        // Gets a string contains new line characters based on chosen line breaker style.
        internal static string GetString(this LineBreakerStyle lineBreaker)
        {
            switch (lineBreaker)
            {
                case LineBreakerStyle.Cr:
                    return "\r";
                case LineBreakerStyle.Lf:
                    return "\n";
                case LineBreakerStyle.CrLf:
                    return "\r\n";
                default:
                    return Environment.NewLine;
            }
        }

        // Gets a string that represents the specified enum value base on attribute or field name.
        internal static string GetStringValue(this Enum value)
        {
            string stringValue = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(stringValue);
            StringValueAttribute attribute = fieldInfo == null
                ? null 
                : (StringValueAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StringValueAttribute));
            return attribute == null ? stringValue : attribute.StringValue;
        }

        // Generates a list of custom escape characters.
        internal static readonly KeyValuePair<char, object>[] CustomEscapeCharacters =
        {
            new KeyValuePair<char, object>('l', new NewLine()),
            new KeyValuePair<char, object>('D', new DateTimeNow()),
            new KeyValuePair<char, object>('d', new DateTimeNow(true))
        };

        // This method uses boolean flags to determine if either \r (carriage return) or \n (line feed) characters are present.
        // It stops iterating as soon as it finds both characters.
        // This approach is simpler and faster but can be less accurate in detecting line break types in mixed content.
        internal static LineBreakerStyle AutoDetectLineBreaker(this string content)
        {
            bool r = false, n = false;

            for (int index = 0; index < content.Length; index++)
            {
                var c = content[index];
                if (c == '\r') r = true;
                if (c == '\n') n = true;

                // If both carriage return and line feed were found, exit the loop
                if (r && n) break;
            }

            // Determine the line break type based on the flags set
            return n ? r ? LineBreakerStyle.CrLf : LineBreakerStyle.Lf : r ? LineBreakerStyle.Cr : LineBreakerStyle.Default;
        }

        // This method counts the occurrences of both line break types throughout the entire string.
        // It provides a more comprehensive analysis by assessing the relative frequencies of \r and \n,
        // which allows it to make a more informed decision about which line break style is predominant.
        internal static LineBreakerStyle AutoDetectLineBreakerEx(this string content)
        {
            const int threshold = 20; // It means 20%.
            int r = 0, n = 0;

            for (int index = 0; index < content.Length; index++)
            {
                var c = content[index];
                if (c == '\r') r++;
                if (c == '\n') n++;
            }

            // If neither character is found, return the default line breaker
            if (n == 0 && r == 0) return LineBreakerStyle.Default;

            // If the percentage difference between line feeds and carriage returns is less than 20%, assume mixed line endings
            if (n.GetDifference(r) < threshold) return LineBreakerStyle.CrLf;

            // Return the line breaker based on which character is more prevalent
            return n > r ? LineBreakerStyle.Lf : LineBreakerStyle.Cr;
        }

        // Tries to determine the encoding, checking the presence of signature (BOM) for some popular encodings.
        // 
        internal static Encoding AutoDetectEncoding(string fileName, Encoding defaultEncoding = null)
        {
            byte[] bom = new byte[4];

            using (FileStream fs = File.OpenRead(fileName))
            {
                int count = fs.Read(bom, 0, 4);

                // Check for BOM (Byte Order Mark)
                if (count > 2)
                {
                    if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
                    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
                    if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
                }
                else if (count > 1)
                {
                    if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; // UTF-16LE
                    if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; // UTF-16BE
                }
            }

            // Default fallback.
            return defaultEncoding ?? Encoding.Default;
        }

        // Checks if a character is a new line character.
        internal static bool IsNewLine(this char c)
        {
            return c == '\n' || c == '\r';
        }

        // Appends a copy of the specified string followed by the specified line terminator to the end of StringBuilder.
        internal static StringBuilder AppendLine(this StringBuilder sb, string value, string newLine)
        {
            sb.Append(value);
            sb.Append(newLine);

            return sb;
        }

        // Appends a copy of the specified string followed by the specified line terminator to the end of StringBuilder.
        internal static StringBuilder AppendLine(this StringBuilder sb, string value, LineBreakerStyle lineBreaker)
        {
            string newLine = lineBreaker.GetString();

            sb.Append(value);
            sb.Append(newLine);

            return sb;
        }

        // Moves index to the end of current line in the StringBuilder.
        internal static StringBuilder MoveIndexToEndOfLinePosition(this StringBuilder sb, ref int index)
        {
            int length = sb.Length;

            // Adjust index if it's beyond the current length.
            if (index < 0) index = 0;
            else if (index >= length) index = length;

            // Search for the nearest line breaker and move index to position after line breaker.
            else if (index > 0)
            {
                while (index < length && !sb[index].IsNewLine())
                    index++;

                while (index < length && sb[index].IsNewLine())
                    index++;
            }

            return sb;
        }

        // Inserts the default new line at the specified index in the StringBuilder and update the index.
        internal static StringBuilder InsertLine(this StringBuilder sb, ref int index, string newLine)
        {
            if ((uint)index > (uint)sb.Length)
                throw new ArgumentOutOfRangeException(nameof(index), GetResourceString("ArgumentOutOfRange_Index"));

            // Update the index position.
            sb = sb.MoveIndexToEndOfLinePosition(ref index);

            // Insert the default new line at the specified index.
            sb = sb.Insert(index, newLine);
            index += newLine.Length - 1;
            return sb;

        }

        // Inserts the default new line at the specified index in the StringBuilder and update the index.
        internal static StringBuilder InsertLine(this StringBuilder sb, ref int index, LineBreakerStyle lineBreaker)
        {
            if ((uint)index > (uint)sb.Length)
                throw new ArgumentOutOfRangeException(nameof(index), GetResourceString("ArgumentOutOfRange_Index"));

            string newLine = lineBreaker.GetString();

            return sb.InsertLine(ref index, newLine);
        }

        // Inserts a specified line at the specified index in the StringBuilder, followed by a specified new line and update the index.
        internal static StringBuilder InsertLine(this StringBuilder sb, ref int index, string newLine, string line)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), GetResourceString("ArgumentOutOfRange_Index"));

            sb = MoveIndexToEndOfLinePosition(sb, ref index);

            // Insert the line content.
            sb = sb.Insert(index, line);
            index += line.Length;

            // Insert the new line.
            sb = sb.Insert(index, newLine);
            index += newLine.Length - 1;

            return sb;
        }

        // Inserts a specified line at the specified index in the StringBuilder, followed by a specified new line and update the index.
        internal static StringBuilder InsertLine(this StringBuilder sb, ref int index, LineBreakerStyle lineBreaker, string line)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), GetResourceString("ArgumentOutOfRange_Index"));

            string newLine = lineBreaker.GetString();

            return sb.InsertLine(ref index, newLine, line);
        }

        // Inserts multiple lines at the specified index in the StringBuilder, each followed by a specified new line and update the index.
        internal static StringBuilder InsertLines(this StringBuilder sb, ref int index, string newLine, params string[] lines)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), GetResourceString("ArgumentOutOfRange_Index"));

            // If no lines, just append the new line.
            if (lines.Length == 0) return sb.InsertLine(ref index, newLine);

            sb = MoveIndexToEndOfLinePosition(sb, ref index);

            // Insert each line followed by the new line.
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                sb.Insert(index, line);
                index += line.Length;

                sb.Insert(index, newLine);
                index += newLine.Length;
            }

            index--;

            return sb;
        }

        // Inserts multiple lines at the specified index in the StringBuilder, each followed by a specified new line and update the index.
        internal static StringBuilder InsertLines(this StringBuilder sb, ref int index, LineBreakerStyle lineBreaker, params string[] lines)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), GetResourceString("ArgumentOutOfRange_Index"));

            string newLine = lineBreaker.GetString();

            return sb.InsertLines(ref index, newLine, lines);
        }

        // Converts a string to lowercase based on the specified 
        internal static string MayBeToLower(this string s, StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                    return s.ToLower(CultureInfo.CurrentCulture);
                case StringComparison.InvariantCultureIgnoreCase:
                    return s.ToLower(CultureInfo.InvariantCulture);
                case StringComparison.OrdinalIgnoreCase:
                    return s.ToLower();
            }
            return s;
        }

        // Returns a CultureInfo object that defines the string comparison rules for the specified StringComparison.
        internal static CultureInfo GetCultureInfo(this StringComparison comparison)
        {
            return comparison < StringComparison.InvariantCulture
                ? CultureInfo.CurrentCulture
                : CultureInfo.InvariantCulture;
        }

        // Returns the CompareOptions value based on the specified StringComparison.
        internal static CompareOptions GetCompareOptions(this StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareOptions.IgnoreCase;
                case StringComparison.Ordinal:
                    return CompareOptions.Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    return CompareOptions.OrdinalIgnoreCase;
                default:
                    return CompareOptions.None;
            }
        }

        // Returns the StringComparer based on the specified StringComparison.
        internal static StringComparer GetComparer(this StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCulture:
                    return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase:
                    return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture:
                    return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase:
                    return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.Ordinal:
                    return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    return StringComparer.OrdinalIgnoreCase;
                default:
                    return StringComparer.InvariantCultureIgnoreCase;
            }
        }

        // Sets or clears the RegexOptions flags based on the specified StringComparison, returning the modified value.
        internal static RegexOptions GetRegexOptions(this StringComparison comparison, RegexOptions options = RegexOptions.None)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCulture:
                    options &= ~RegexOptions.CultureInvariant;
                    break;
                case StringComparison.CurrentCultureIgnoreCase:
                    options &= ~RegexOptions.CultureInvariant; 
                    options |= RegexOptions.IgnoreCase;
                    break;
                case StringComparison.InvariantCulture:
                    options |= RegexOptions.CultureInvariant;
                    break;
                case StringComparison.InvariantCultureIgnoreCase:
                    options |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    options |= RegexOptions.IgnoreCase;
                    break;
            }

            return options;
        }

        // Allows you to compare strings using the specified StringComparer.
        internal static bool Equals(this string s, string value, StringComparer comparer)
        {
            return comparer.Equals(s, value);
        }

        // Checks whether the string is null, empty or contains only white spaces.
        internal static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrEmpty(s) || s.All(char.IsWhiteSpace);
        }

        // Checks whether the string is null or empty.
        internal static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /*// Allows you to combine strings into one line, separating them with spaces.
        internal static string Concat(params string[] items)
        {
            return new StringDelimiter(items).ToString(delimiter: ' ');
        }

        // Allows you to concatenate strings into one string with a specified delimiter between the items.
        internal static string Concat(char delimiter, params string[] items)
        {
            return new StringDelimiter(items).ToString(delimiter);
        }

        // Allows you to concatenate strings into one string with a specified delimiter between the items.
        internal static string Concat(char delimiter, params object[] items)
        {
            return new StringDelimiter(items).ToString(delimiter);
        }

        // Allows you to concatenate strings into one string with a specified delimiter between the items.
        internal static string Concat(char delimiter, IEnumerable<string> items)
        {
            return new StringDelimiter(items).ToString(delimiter);
        }*/

        // Returns a substring of a specified number of characters, starting from the beginning of the string.
        internal static string SubstringLeft(this string s, int length)
        {
            return s.Substring(0, length);
        }

        internal static char[] Add(this char[] chars, params char[] addChars)
        {
            int length = chars.Length + addChars.Length;
            char[] resultChars = new char[length];
            int i = 0;

            /*Array.Copy(chars, resultChars, chars.Length);
            Array.Copy(addChars, 0, resultChars, chars.Length, addChars.Length);*/

            while (i < chars.Length)
            {
                resultChars[i] = chars[i];
                i++;
            }
            while (i < addChars.Length)
            {
                resultChars[i] = addChars[i];
                i++;
            }

            return resultChars;
        }

        // Checks whether a string contains the specified character.
        internal static bool Contains(this string text, char c)
        {
            for (int i = 0; i < text.Length; i++)
                if (text[i].Equals(c))
                    return true;
            return false;
        }

        // Removes spaces and the end-of-line character (\0) from a string.
        // This method can be useful when processing data received from external sources, where extra characters may appear.
        internal static string TrimWhiteSpaceAndNull(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            int start = 0;
            int end = value.Length - 1;

            // Skip leading and trailing whitespace and null characters.
            while (start <= end && value[start].IsWhiteSpaceOrNull()) 
                start++;

            while (end >= start && value[end].IsWhiteSpaceOrNull())
                end--;

            // If the trimmed string is empty, return an empty string.
            // Otherwise, return the trimmed substring.
            return start > end ? string.Empty : value.Substring(start, end - start + 1);
        }

        // Indicates that character is white space or '\0'.
        internal static bool IsWhiteSpaceOrNull(this char c)
        {
            return char.IsWhiteSpace(c) || c == '\0';
        }

        // Escape characters in the input string with backslashes.
        internal static string ToEscape(this string text, params char[] customEscapeCharacters)
        {
            int pos = 0;
            int inputLength = text.Length;
            bool useCustom = customEscapeCharacters != null && customEscapeCharacters.Length > 0;

            if (inputLength == 0) return text;

            StringBuilder sb = new StringBuilder(inputLength * 2);
            do
            {
                char c = text[pos++];

                switch (c)
                {
                    case '\\':
                        sb.Append(@"\\");
                        break;
                    case '\0':
                        sb.Append(@"\0");
                        break;
                    case '\a':
                        sb.Append(@"\a");
                        break;
                    case '\b':
                        sb.Append(@"\b");
                        break;
                    case '\n':
                        sb.Append(@"\n");
                        break;
                    case '\r':
                        sb.Append(@"\r");
                        break;
                    case '\f':
                        sb.Append(@"\f");
                        break;
                    case '\t':
                        sb.Append(@"\t");
                        break;
                    case '\v':
                        sb.Append(@"\v");
                        break;
                    default: 
                        if (useCustom && customEscapeCharacters.Contains(c))
                            sb.Append($@"\{c}"); // Customized characters are additionally escaped here.
                        else
                            sb.Append(c);
                        break;
                }
            } while (pos < inputLength);

            return sb.ToString();
        }

        internal static string ToEscape(this char c, params char[] customEscapeCharacters)
        {
            return new string(c, 1).ToEscape(customEscapeCharacters);
        }

        // Converts any escaped characters in the input string.
        internal static string UnEscape(this string text, params KeyValuePair<char, object>[] customEscapeCharacters)
        {
            int pos = -1;
            int inputLength = text.Length;
            bool useCustom = customEscapeCharacters != null && customEscapeCharacters.Length > 0;

            if (inputLength == 0) return text;

            // Find the first occurrence of backslash or return the original text.
            for (int i = 0; i < inputLength; ++i)
            {
                if (text[i] == '\\')
                {
                    pos = i;
                    break;
                }
            }

            if (pos < 0) return text; // Backslash not found.

            // If backslash is found.
            StringBuilder sb = new StringBuilder(text.Substring(0, pos));
            // [MethodImpl(MethodImplOptions.AggressiveInlining)] // Uncomment if necessary.
            char UnHex(string hex)
            {
                int c = 0;
                for (int i = 0; i < hex.Length; i++)
                {
                    int r = hex[i];
                    if (r > 0x2F && r < 0x3A) r -= 0x30;
                    else if (r > 0x40 && r < 0x47) r -= 0x37;
                    else if (r > 0x60 && r < 0x67) r -= 0x57;
                    else return '?';
                    c = (c << 4) + r;
                }

                return (char)c;
            }

            do
            {
                char c = text[pos++];
                if (c == '\\')
                {
                    c = pos < inputLength ? text[pos] : '\\';
                    switch (c)
                    {
                        case '\\':
                            c = '\\';
                            break;
                        case '0':
                            c = '\0';
                            break;
                        case 'a':
                            c = '\a';
                            break;
                        case 'b':
                            c = '\b';
                            break;
                        case 'n':
                            c = '\n';
                            break;
                        case 'r':
                            c = '\r';
                            break;
                        case 'f':
                            c = '\f';
                            break;
                        case 't':
                            c = '\t';
                            break;
                        case 'v':
                            c = '\v';
                            break;
                        case 'u' when pos < inputLength - 3:
                            c = UnHex(text.Substring(++pos, 4));
                            pos += 3;
                            break;
                        case 'x' when pos < inputLength - 1:
                            c = UnHex(text.Substring(++pos, 2));
                            pos++;
                            break;
                        case 'c' when pos < inputLength:
                            c = text[++pos];
                            if (c >= 'a' && c <= 'z')
                                c -= ' ';
                            if ((c = (char)(c - 0x40U)) >= ' ')
                                c = '?';
                            break;
                        default:
                            KeyValuePair<char, object> custom;
                            if (useCustom && (custom = customEscapeCharacters.FirstOrDefault(pair => pair.Key == c)).Value != null)
                                sb.Append(custom.Value); // Customized characters are additionally unescaped here.
                            else
                                sb.Append("\\" + c);
                            pos++;
                            continue;
                    }
                    pos++;
                }
                sb.Append(c);

            } while (pos < inputLength);

            return sb.ToString();
        }
        
    }
}