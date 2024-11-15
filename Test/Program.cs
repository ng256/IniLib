using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Ini;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IniFileSettings iniSettings = new IniFileSettings() {AllowEscapeCharacters = true};

            TestSettings test = new TestSettings();
            using (var iniFile = IniFile.Load(iniSettings))
            {
                iniFile.ImportSettings(test);
            }

            Console.WriteLine(test);
            //TypeConverterTest.Run();
            Console.ReadKey();
        }
    }
}
