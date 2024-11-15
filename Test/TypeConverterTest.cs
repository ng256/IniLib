using System;
using System.ComponentModel;
using System.Ini;

namespace Test
{
    internal class TypeConverterTest
    {
        internal static bool TestConverterFrom<T>(params object[] testObjects)
        {
            var converter = (TypeConverter)ConverterCache.Extended[typeof(T)];
            var test = true;
            Console.WriteLine($"Testing {converter}:");

            foreach (var obj in testObjects)
            {
                try
                {
                    object result = converter.ConvertFrom(obj);
                    Console.WriteLine($"\"{obj}\" → {result}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\"{obj ?? "NULL"}\" → {e.Message.Replace(Environment.NewLine, " ")}");
                    test = false;
                }
            }

            Console.WriteLine();

            return test;
        }

        internal static bool TestConverterFrom<T, D>(params object[] testObjects)
        {
            var converter = (TypeConverter)ConverterCache.Extended[typeof(T)];
            var test = true;
            Console.WriteLine($"Testing {converter}:");

            foreach (var obj in testObjects)
            {
                try
                {
                    object result = converter.ConvertTo(obj, typeof(D));
                    Console.WriteLine($"\"{obj}\" → {result}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\"{obj ?? "NULL"}\" → {e.Message.Replace(Environment.NewLine, " ")}");
                    test = false;
                }
            }

            Console.WriteLine();

            return test;
        }


        internal static bool Run()
        {
            return TestConverterFrom<bool>(
                null,
                string.Empty,
                " yes",
                "no",
                "3  ",
                "0",
                "abcd  ",
                "255",
                "0xFF",
                "0b11",
                "0o77",
                "  100h  ",
                "100b ",
                " 100o")
                
                
                &


                TestConverterFrom<int>(
                    null,
                    string.Empty,
                    "  1",
                    "2",
                    "3  ",
                    "abcd  ",
                    "255",
                    "0xFF",
                    "0b11",
                    "0o77",
                    "  100h  ",
                    "100b ",
                    " 100o"
                )
                
                
                &


                TestConverterFrom<LineBreakerStyle>(
                    null,
                    string.Empty,
                    "  1",
                    "2",
                    "3  ",
                    "Default  ",
                    "255",
                    "0xFF",
                    "0b11",
                    "0o77",
                    "  100h  ",
                    "100b ",
                    " 100o"
                );
        }
    }
}
