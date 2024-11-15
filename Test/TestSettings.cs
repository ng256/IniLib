using System;
using System.Ini;
using System.Text;

namespace Test
{
    [Section("settings")]
    internal class TestSettings
    {
        public string Unknown { get; set; } // Unknown text
        public string Text { get; set; }  // Text value
        public int DecNumber { get; set; } // Decimal number
        public int HexNumber { get; set; } // Hexadecimal number
        public int OctNumber { get; set; } // Octal number
        public int BinNumber { get; set; } // Binary number
        public char[] Chars { get; set; } // Character string
        public byte[] Bytes { get; set; } // Byte array
        public string[] Strings { get; set; } // List of strings
        [Section("web"), Entry("host")]
        public Uri[] Hosts { get; set; }

        #region Additional tools

        public TestSettings Default => new TestSettings
        {
            Unknown = "Some value",
            Text = "Hello, world!",
            DecNumber = 1024,
            HexNumber = 0x1FFF,
            OctNumber = 493,
            BinNumber = 0b111,
            Chars = "abcdef".ToCharArray(),
            Bytes = new byte[] { 0x34, 0x32, 0x0A, 0x0D },
            Strings = new[] { "Hello", "world" },
            Hosts = new[] { new Uri("http://127.0.0.1:8080"), new Uri("http://192.168.0.1:80") }
        };

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // Append basic properties
            sb.AppendLine("[Settings]");
            sb.AppendLine($"Unknown: {Unknown ?? "null"}".ToEscape());
            sb.AppendLine($"Text: {Text ?? "null"}".ToEscape());
            sb.AppendLine($"DecNumber: {DecNumber}");
            sb.AppendLine($"HexNumber: {HexNumber:X}h");
            sb.AppendLine($"OctNumber: {Convert.ToString(OctNumber, 8)}o");
            sb.AppendLine($"BinNumber: {Convert.ToString(BinNumber, 2)}b");

            // Append Chars array
            sb.Append("Chars: ");
            sb.AppendLine(Chars != null ? new string(Chars) : "null");

            // Append Bytes array
            sb.Append("Bytes: ");
            if (Bytes != null)
                sb.AppendLine(BitConverter.ToString(Bytes).Replace("-", "").ToLower());
            else
                sb.AppendLine("null");

            // Append Strings array
            sb.AppendLine("Strings: ");
            if (Strings != null && Strings.Length > 0)
            {
                foreach (var str in Strings)
                    sb.AppendLine($"  - {str}");
            }
            else
            {
                sb.AppendLine("  null");
            }

            // Append Hosts array
            sb.AppendLine("[Web]");
            sb.AppendLine("Hosts: ");
            if (Hosts != null && Hosts.Length > 0)
            {
                foreach (var host in Hosts)
                    sb.AppendLine($"  - {host}");
            }
            else
            {
                sb.AppendLine("  null");
            }

            return sb.ToString();
        }

        #endregion
    }
}