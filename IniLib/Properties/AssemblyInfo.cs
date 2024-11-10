







/***********************************************************
Multi-Source Configuration Library v. 1.0

The MIT License (MIT)
Copyright: © NG256 2021-2024.

Permission is  hereby granted, free of charge, to any person
obtaining   a copy    of    this  software    and associated
documentation  files  (the "Software"),    to  deal   in the
Software without  restriction, including without  limitation
the rights to use, copy, modify, merge, publish, distribute,
sublicense,  and/or  sell  copies   of  the Software, and to
permit persons to whom the Software  is furnished to  do so,
subject       to         the      following      conditions:

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
[assembly: AssemblyDescription("Multi-Source Configuration Library 1.0. A versatile configuration library for .NET applications that enables seamless initialization and management of application settings from multiple sources.")] // Assembly description. 
[assembly: AssemblyCompany("NG256")] // Developer.
[assembly: AssemblyProduct("NG256 Multi-Source Configuration Library")] // Product name.
[assembly: AssemblyCopyright("© NG256 2021-2024")] // Copyright.
[assembly: AssemblyTrademark("NG256® Multi-Source Configuration Library®")] // Trademark.
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.2411.0018")]
[assembly: AssemblyFileVersion("1.0.2411.0018")]
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
[assembly: ApplicationID("1c27c828-b113-4479-b214-68b2cc8f2868")]
#else
[assembly: ComVisible(false)]
#endif
// The following GUID serves to identify the type library if this project will be visible to COM 
[assembly: Guid("ee011f53-f6f4-4f0e-a166-26c6fa56cb1e")]
