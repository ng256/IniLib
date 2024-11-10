# IniLib

IniLib is a lightweight and efficient INI file parser and handler for .NET. It provides a flexible way to read and write configuration data from INI files using regular expressions while preserving the original formatting.

## Features
- **INI file parsing**: Reads and parses INI files into sections and entries.
- **Customizable behavior**: Allows customization of string comparison, escape character usage, and multi-line values.
- **Preservation of formatting**: Ensures that original INI file formatting (comments, line breaks) is retained when making changes.
- **Attributes for easy configuration mapping**: Provides `IniSectionAttribute` and `IniEntryAttribute` for mapping INI sections and entries to .NET classes and properties.
- **Cross-platform**: Works across different platforms using .NET.

## Installation
You can install the latest version of IniLib via NuGet Package Manager or by directly referencing the library in your project.

```bash
Install-Package IniLib
```

## Usage

### Reading INI file data:
```csharp
using System.Ini;
var iniFile = IniFile.Load("config.ini");
string sectionValue = iniFile.GetSection("SectionName")["EntryName"];
```

### Writing INI file data:
```csharp
using System.Ini;
var iniFile = IniFile.Load("config.ini");
iniFile["SectionName", "EntryName"] = "NewValue";
iniFile.Save("config.ini");
```

### Working with attributes:
Use **IniSectionAttribute** and **IniEntryAttribute** to map INI data to properties:
```csharp
[IniSection("SectionName")]
public class MyConfig
{
    [IniEntry("EntryName")]
    public string EntryValue { get; set; }
}
```

## License
**IniLib** is licensed under the MIT License.

© 2024 Pavel Bashkardin

For more information, visit the repository: IniLib GitHub
