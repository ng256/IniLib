/***************************************************************

•   File: FileSystem.cs

•   Description

    This code  snippet is a set  of methods to make it easier to
    work with the file system and paths to files and folders.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

using System.IO;
using System.Linq;

namespace System
{
    internal static partial class InternalTools
    {
        // Array containing the characters that are not allowed in path names.
        internal static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        // Checks whether the fileName string contains invalid characters for the path.
        internal static bool IsInvalidPath(string fileName)
        {
            return fileName.Any(c => InvalidPathChars.Contains(c));
        }

        // Checks whether the file name is correct and, if necessary, whether the file exists.
        // Returns null if the file name is valid, otherwise returns an Exception object to throw at the calling code.
        internal static Exception ValidateFileName(string fileName, bool checkExists = false)
        {
            if (fileName == null)
                return new ArgumentNullException(nameof(fileName), GetResourceString("ArgumentNull_Path"));
            if (fileName.IsNullOrWhiteSpace())
                return new ArgumentException(GetResourceString("Argument_PathEmpty"), nameof(fileName));
            if (IsInvalidPath(fileName))
                return new ArgumentException(GetResourceString("Argument_InvalidPathChars"));
            if (checkExists && !File.Exists(fileName))
                return new FileNotFoundException(GetResourceString("IO.FileNotFound_FileName", fileName));

            return null;
        }

        // Checks whether the file name is correct and, if necessary, whether the file exists.
        // Returns null if the file name is valid, otherwise returns an Exception object to throw at the calling code.
        internal static Exception ValidateFileName(string fileName, out string filePath, bool checkExists = false)
        {
            filePath = null;

            if (fileName == null)
                return new ArgumentNullException(nameof(fileName), GetResourceString("ArgumentNull_Path"));
            if (fileName.IsNullOrWhiteSpace())
                return new ArgumentException(GetResourceString("Argument_PathEmpty"), nameof(fileName));
            if (IsInvalidPath(fileName))
                return new ArgumentException(GetResourceString("Argument_InvalidPathChars"));
            if (checkExists && !File.Exists(fileName))
                return new FileNotFoundException(GetResourceString("IO.FileNotFound_FileName", fileName));

            filePath = Path.GetFullPath(fileName);

            return null;
        }


    }
}