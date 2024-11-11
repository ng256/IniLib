/***************************************************************

•   File: ConverterCache.cs

•   Description

    The ConverterCache  class is used to cache information about
    TypeConverters, which can be  used  to convert  values  ​​from
    one data type to another. This  allows you  to speed  up the
    conversion   process and  reduce   the load on   the system.

    TypeConverter is a class  in the .NET Framework that is used
    to   convert  objects of one    type  to another.  It allows
    developers to create their own type converters, which can be
    used   for  various    purposes    such   as  serialization,
    deserialization, or type casting.

•   Copyright

    © Pavel Bashkardin, 2022

***************************************************************/

using System.Collections;
using System.Globalization;
using System.Text;
using static System.InternalTools;

namespace System.ComponentModel
{

    internal sealed class ConverterCache : Hashtable
    {
        // The default cache instance containing standard converters.
        public static ConverterCache Default { get; } = new ConverterCache();

        // The extended cache instance containing customized converters for specific types.
        public static ConverterCache Extended { get; } = new ConverterCache
        {
            { typeof(bool), new BooleanConverterExtended() },
            { typeof(short), new Int16ConverterExtended() },
            { typeof(ushort), new UInt16ConverterExtended() },
            { typeof(int), new Int32ConverterExtended() },
            { typeof(uint), new UInt32ConverterExtended() },
            { typeof(long), new Int64ConverterExtended() },
            { typeof(ulong), new UInt64ConverterExtended() },
            { typeof(float), new SingleConverterExtended() },
            { typeof(double), new DoubleConverterExtended() },
            { typeof(CultureInfo), new CultureInfoConverterExtended() },
            { typeof(Encoding), new EncodingConverterExtended() },
        };

        // Initializes a new instance of the ConverterCache class.
        private ConverterCache() : base()
        {
        }

        // The method adds a new type-converter pair to the cache.
        // If a converter is not specified, then the default converter is used.
        public TypeConverter Add(Type type, TypeConverter converter = null)
        {
            if (converter is StringConverter) return null;
            if (ContainsKey(type)) Remove(type);
            this[type] = converter ?? (converter = TypeDescriptor.GetConverter(type));
            return converter;
        }

        // Override based method
        public override void Add(object key, object value)
        {
            if (key is Type type && value is TypeConverter converter)
                Add(type, converter);
        }

        // Returns a converter for the specified type.
        public TypeConverter Get(Type type)
        {
            return base[type] as TypeConverter ?? Add(type);
        }

        // If the key is a type, then the corresponding converter is returned from the cache.
        public override object this[object key]
        {
            get
            {
                return key is Type type ? Get(type) : throw new ArgumentException(GetResourceString("Arg_MustBeType"));
            }
            set
            {
                if (key is Type type && value is TypeConverter converter)
                    Add(type, converter);
            }
        }
    }
}