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
            IniFileSettings iniSettings = IniFileSettings.Detect();

            TestSettings test = new TestSettings();
            using (var iniFile = IniFile.Load(iniSettings))
            {
                iniFile.ImportSettings(test);
            }

            Console.WriteLine(test);
            Console.ReadKey();
        }
    }
}
