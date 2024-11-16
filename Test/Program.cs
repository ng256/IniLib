using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Ini;
using System.IO;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            JsonFileSettings jsettings = new JsonFileSettings {AllowEscapeCharacters = true};
            string source = File.ReadAllText("test.json");
            JsonParserQuickScan jparser1 = new JsonParserQuickScan(source, jsettings);
            JsonParserCached jparser2 = new JsonParserCached(source, jsettings);
            string unknown1 = jparser1.GetValue("Settings.text", "text");
            string text1 = jparser1.GetValue("Settings.text", "text");
            jparser1.SetValue("Settings.NewNumvalue1", "1");
            jparser1.SetValues("Settings.NewNumvalue2", "2", "3");
            jparser2.SetValue("Settings.NewNumvalue1", "1");
            jparser2.SetValues("Settings.NewNumvalue2", "2", "3");
            string jcontent1 = jparser1.Content;
            string jcontent2 = jparser2.Content;
            bool flag = jcontent1 == jcontent2;

            TestSettings test = new TestSettings();
            using (var iniFile = IniFile.Load())
            {
                iniFile.ImportSettings(test);
            }

            Console.WriteLine(test);
            Console.ReadKey();
        }
    }
}
