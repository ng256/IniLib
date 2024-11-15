/***************************************************************

•   File: ResourcesCache.cs

•   Description

    Provides a convenient way to cache and access resources from
    different assemblies as ResourceSet objects, allowing you to
    quickly access  them without  having  to search the assembly
    again.

•   Copyright

    © Pavel Bashkardin, 2022-2024


***************************************************************/

using System.Collections;
using System.Globalization;
using System.Reflection;

namespace System.Resources
{
    // It is an internal class that is a descendant of the Hashtable class and is intended for caching resources from assemblies.
    internal class ResourcesCache : Hashtable
    {
        // .NET Framework main dll resources. Contains most error messages.
        public ResourceSet MSCorLib;

        public ResourcesCache()
        {
            MSCorLib = GetResourceSet(typeof(object));
        }

        // Returns a set of resources from the specified assembly.
        public ResourceSet GetResourceSet(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            AssemblyName assemblyName = assembly.GetName();
            if (base[assemblyName.Name] is ResourceSet resources)
                return resources;

            ResourceManager resManager = new ResourceManager(assemblyName.Name, assembly);
            ResourceSet resourceSet = resManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            base.Add(assemblyName.Name, resourceSet);
            return resourceSet;
        }

        // Returns a set of resources from the assembly that implements the specified type.
        public ResourceSet GetResourceSet(Type type)
        {
            return GetResourceSet((type ?? typeof(object)).Assembly);
        }

        // Gets or sets resources associated with the specified key.
        public override object this[object key]
        {
            get
            {
                switch (key)
                {
                    case Assembly assembly:
                        return GetResourceSet(assembly);
                    case Type type:
                        return GetResourceSet(type.Assembly);
                    case string str:
                        return base[str] as ResourceSet;
                    default:
                        return GetResourceSet(key.GetType().Assembly);
                }
            }
            set
            {
                if (key is string && value is ResourceSet) base[key] = value;
            }
        }

        // Searches for a string resource with the specified name.
        public string this[object key, string name] => (this[key] as ResourceSet)?.GetString(name);
    }
}