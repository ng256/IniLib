using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Test
{
    internal class Program
    {
        static void TestConverter()
        {
            string[] testStrings =
            {
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
                " 100o",
            };

            var converter = (TypeConverter) ConverterCache.Extended[typeof(int)];

            foreach (var s in testStrings)
            {
                try
                {
                    object result = converter.ConvertFromInvariantString(s);
                    Console.WriteLine($"\"{s}\" → {result}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\"{s ?? "NULL"}\" → {e.Message}");
                }
            }
        }

        static void Main(string[] args)
        {
            TestConverter();

            Console.ReadKey();
        }
    }
}
