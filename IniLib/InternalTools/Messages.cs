/***************************************************************

•   File: Messages.cs

•   Description

    Represents a set of static methods that allow you to receive
    error messages from various sources.

    The  class can be  used to receive error  messages  that may
    occur during program execution. These messages can be useful
    when  debugging  and   analyzing    problems  in  your code.

    The GetResourceString methods  of   the InternalTools  class
    allow you to get error messages:

    - from the mscorlib library;
    - from  the  assembly   specified   as  a  method parameter;
    - for  the type  specified  as the method's  type parameter.

    Also,   methods  of    the    InternalTools   class  support
    parameterization  of error messages. This allows you to pass
    additional parameters,  such as  arguments, to generate more
    detailed messages.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

using System.Reflection;
using System.Resources;

namespace System
{
    internal partial class InternalTools
    {
        // Used for caching resources such as error messages.
        private static ResourcesCache Resources = new ResourcesCache();

        // Gets mscorlib internal error message.
        internal static string GetResourceString(string resourceName)
        {
            return Resources.MSCorLib.GetString(resourceName) ?? string.Empty;
        }

        // Gets assembly internal error message.
        internal static string GetResourceString(this Assembly assembly, string resourceName)
        {
            return Resources[assembly, resourceName] ?? string.Empty;
        }

        // Gets parametrized error message for assembly contains the specified type.
        internal static string GetResourceString<T>(string resourceName)
        {
            return Resources[typeof(T), resourceName] ?? string.Empty;
        }

        // Gets parametrized mscorlib internal error message.
        internal static string GetResourceString(string resourceName, params object[] args)
        {
            string format = GetResourceString(resourceName);
            return string.Format(format, args);
        }

        // Gets parametrized assembly internal error message.
        internal static string GetResourceString(this Assembly assembly, string resourceName, params object[] args)
        {
            string format = assembly.GetResourceString(resourceName);
            return string.Format(format, args);
        }

        // Gets parametrized error message for assembly contains the specified type.
        internal static string GetResourceString<T>(string resourceName, params object[] args)
        {
            string format = GetResourceString<T>(resourceName);
            return string.Format(format, args);
        }
    }
}
