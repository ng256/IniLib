/***************************************************************

•   File: Reflection.cs

•   Description

    This code  snippet is a set of extension methods for working
    with  objects, properties, and types in C#.

    Extension methods allow you to   add new methods to existing
    types without changing their source code. This allows you to
    expand the functionality of existing types and  simplify the
    code.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System
{
    internal static partial class InternalTools
    {
        internal const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        // Returns an array of PropertyInfo for all properties of an object.
        internal static PropertyInfo[] GetProperties(this object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(Instance);

            return properties;
        }

        // Returns an array of PropertyInfo for all properties of an object or only for properties with certain attributes.
        internal static PropertyInfo[] GetProperties(this object obj, params Type[] attributes)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Type type = obj.GetType();
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (PropertyInfo property in type.GetProperties(Instance))
                foreach (Type attribute in attributes)
                {
                    if (attribute.IsSubclassOf(typeof(Attribute)) && property.IsDefined(attribute, false))
                        properties.Add(property);
                }

            return properties.ToArray();
        }

        // Returns an array of static PropertyInfo for the specified type.
        internal static PropertyInfo[] GetStaticProperties(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            PropertyInfo[] properties = type.GetProperties(Static);
            return properties;
        }

        // Returns an array of static PropertyInfo for the specified type that have the specified attributes.
        internal static PropertyInfo[] GetStaticProperties(this Type type, params Type[] attributes)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (PropertyInfo property in type.GetProperties(Static))
                foreach (Type attribute in attributes)
                {
                    if (attribute.IsSubclassOf(typeof(Attribute)) && property.IsDefined(attribute, false))
                        properties.Add(property);
                }

            return properties.ToArray();
        }

        // Sets the value of the specified property and object to the provided value.
        internal static void SetValue(this PropertyInfo property, object obj, object value)
        {
            property.SetValue(obj, value, null);
        }

        // Sets the value of the specified property to the provided value.
        internal static void SetValue(this PropertyInfo property, object value)
        {
            property.SetValue(null, value, null);
        }

        // Gets the value of the specified property and object.
        internal static object GetValue(this PropertyInfo property, object obj)
        {
            return property.GetValue(obj, null);
        }

        // Gets the value of the specified property.
        internal static object GetValue(this PropertyInfo property)
        {
            return property.GetValue(null, null);
        }

        // Creates an instance of the type specified by the TypeConverterAttribute.
        internal static TypeConverter CreateInstance(this TypeConverterAttribute attribute)
        {
            string typeName = attribute.ConverterTypeName;
            if (typeName.IsNullOrEmpty())
                throw new InvalidOperationException(GetResourceString("Argument_InvalidTypeName"));
            Type type = Type.GetType(typeName);
            return Activator.CreateInstance(type ?? throw new InvalidOperationException()) as TypeConverter;
        }

        // Gets the default value of the specified property.
        internal static object GetPropertyDefaultValue(this PropertyInfo property)
        {
            DefaultValueAttribute attribute;
            if ((attribute = property.GetCustomAttributes(typeof(TypeConverterAttribute), false)
                    .FirstOrDefault() as DefaultValueAttribute) != null)
                return attribute.Value;

            return property.PropertyType.GetDefaultValue();
        }

        // Gets the type converter for the specified property.
        internal static TypeConverter GetPropertyConverter(this PropertyInfo property, bool createDefault = false)
        {
            Type propertyType = property.GetType();
            TypeConverterAttribute propertyConverter;

            if ((propertyConverter = property.GetCustomAttributes(typeof(TypeConverterAttribute), false)
                    .FirstOrDefault() as TypeConverterAttribute) != null
                && propertyConverter.CreateInstance() is TypeConverter converter)
                return converter;

            if (createDefault)
            {
                return Converters.Get(propertyType);
            }

            return null;
        }

        // Registers base type converter for the specified type.
        internal static bool RegisterConverter(this Type type)
        {
            try
            {
                if (type == null) return false;
                Converters.Add(type);
            }
            catch
            {
                return false;
            }

            return true;
        }

        internal static bool RegisterConverter<TDest>()
        {
            return RegisterConverter(typeof(TDest));
        }

        // Registers custom type converter for the specified type.
        internal static bool RegisterConverter<TConv>(this Type type) where TConv : TypeConverter, new()
        {
            try
            {
                if (type == null || type.IsDefined(typeof(TypeConverterAttribute), false)) return false;
                TypeConverterAttribute attribute = new TypeConverterAttribute(typeof(TConv));
                TypeDescriptor.AddAttributes(type, attribute);
                Converters.Add(type, converter: new TConv());
            }
            catch
            {
                return false;
            }

            return true;
        }

        // Registers custom type converter for the specified type.
        internal static bool RegisterConverter<TDest, TConv>() where TConv : TypeConverter, new()
        {
            return typeof(TDest).RegisterConverter<TConv>();
        }

        // Gets all types from which the current type inherits.
        internal static Type[] GetReflectedTypes(this Type type, bool addThis)
        {
            List<Type> parentTypes = new List<Type>();

            if (type == null)
                return parentTypes.ToArray();

            if (addThis)
            {
                parentTypes.Add(type);
            }

            while (type != null)
            {
                parentTypes.Add(type);
                type = type.ReflectedType;
            }

            parentTypes.Reverse();

            return parentTypes.ToArray();
        }

        // Gets the declaring path of the specified type.
        internal static string GetDeclaringPath(this Type type, char separator = '\\')
        {
            List<string> list = new List<string>(4);
            while (type != null)
            {
                list.Add(type.Name);
                type = type.DeclaringType;
            }

            return string.Join(separator.ToString(), list);
        }

        // Gets the nested type path of the specified type.
        internal static string GetNestedTypePatch(this Type type, char separator = '\\')
        {
            if (type == null) return string.Empty;
            Type[] nestedTypes = type.GetReflectedTypes(true);
            string[] items = new List<string>(nestedTypes.Select(t => t.Name)).ToArray();

            return string.Join(separator.ToString(), items);
        }

        // Checks whether the specified type is a numeric type.
        internal static bool IsNumber(this Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            return typeCode > TypeCode.Char && typeCode < TypeCode.DateTime;
        }

        // Checks whether the specified value is a numeric type.
        internal static bool IsNumber(this object value)
        {
            return value != null && value.GetType().IsNumber();
        }

        // Checks whether the specified type is an integer or string type.
        internal static bool IsIntOrString(this Type type)
        {
            return type != null && (int)Type.GetTypeCode(type) % 9 == 0; // int (9) or string (18).
        }

        // Checks whether the specified value is an integer or string type.
        internal static bool IsIntOrString(this object obj)
        {
            return obj != null && obj.GetType().IsIntOrString();
        }

        // Gets private field value of the specified object.
        internal static object GetPrivateField(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        }
    }
}