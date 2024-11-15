







/***********************************************************
Multi-Source Initialization Library v. 1.0

The MIT License (MIT)
Copyright: © Pavel Bashkardin 2022-2024.

Permission is  hereby granted, free of charge, to any person
obtaining   a copy    of    this  software    and associated
documentation  files  (the "Software"),    to  deal   in the
Software without  restriction, including without  limitation
the rights to use, copy, modify, merge, publish, distribute,
sublicense,  and/or  sell  copies   of  the Software, and to
permit persons to whom the Software  is furnished to  do so,
subject to the following conditions:

The above copyright  notice and this permission notice shall
be  included  in all copies   or substantial portions of the
Software.

THE  SOFTWARE IS  PROVIDED  "AS IS", WITHOUT WARRANTY OF ANY
KIND, EXPRESS  OR IMPLIED, INCLUDING  BUT NOT LIMITED TO THE
WARRANTIES  OF MERCHANTABILITY, FITNESS    FOR A  PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN  NO EVENT SHALL  THE AUTHORS
OR  COPYRIGHT HOLDERS  BE  LIABLE FOR ANY CLAIM,  DAMAGES OR
OTHER LIABILITY,  WHETHER IN AN  ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF   OR IN CONNECTION  WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
************************************************************/

#if COMVISIBLE
using System.EnterpriseServices;
#endif
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// General information about this assembly is provided by the following set
// attributes. Change the values of these attributes to change the information,
// related to the assembly. 
[assembly: AssemblyTitle("IniLib 1.0")] // Assembly name. 
[assembly: AssemblyDescription("Multi-Source Initialization Library 1.0. A versatile initialization library for .NET applications that enables seamless initialization and management of application settings from multiple sources.")] // Assembly description. 
[assembly: AssemblyCompany("Pavel Bashkardin")] // Developer.
[assembly: AssemblyProduct("Pavel Bashkardin Multi-Source Initialization Library")] // Product name.
[assembly: AssemblyCopyright("© Pavel Bashkardin 2022-2024")] // Copyright.
//[assembly: AssemblyTrademark("Pavel Bashkardin® Multi-Source Initialization Library®")] // Trademark.
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.2411.0006")]
[assembly: AssemblyFileVersion("1.0.2411.0006")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: InternalsVisibleTo("Test")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Setting ComVisible to False makes the types in this assembly invisible
// for COM components. If you need to refer to the type in this assembly via COM,
// set the ComVisible attribute to TRUE for this type. 
#if COMVISIBLE
[assembly: ComVisible(true)]
[assembly: ApplicationName("IniLib")] // COM application name.
[assembly: ApplicationID("c43532ef-7d4f-4932-87a2-ba620096fa40")]
#else
[assembly: ComVisible(false)]
#endif
// The following GUID serves to identify the type library if this project will be visible to COM 
[assembly: Guid("a7ac5442-5fb0-4e0c-a579-dfb928eef058")]
