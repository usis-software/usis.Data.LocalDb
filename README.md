# Welcome to the usis LocalDB Class Library

The usis LocalDB Class Library provides a .NET wrapper for the SQL Server Express LocalDB Instance APIs.

## Namespace

All types of the class library are contained in the **usis.Data.LocalDb** namespace.

## NuGet

usis.Data.LocalDb is available as NuGet Package: https://www.nuget.org/packages/usis.Data.LocalDb/

## Documentation

This documentation is available online at https://help.usis-software.com/LocalDB/ and can be download as Compiled HTML Help (.CHM) file.

## Sample

The following sample lists all LocalDB versions  available on the computer:

```c#
using System;

namespace usis.Data.LocalDb.Samples
{
    public static class Operations
    {
        public static void ListVersions()
        {
            using (var manager = Manager.Create())
            {
                foreach (var version in manager.EnumerateVersions())
                {
                    string format;
                    switch (version.Version.Major)
                    {
                        case 11:
                            format = "Microsoft SQL Server 2012 ({0})";
                            break;
                        case 12:
                            format = "Microsoft SQL Server 2014 ({0})";
                            break;
                        case 13:
                            format = "Microsoft SQL Server 2016 ({0})";
                            break;
                        case 14:
                            format = "Microsoft SQL Server 2017 ({0})";
                            break;
                        default:
                            format = "{0}";
                            break;
                    }
                    Console.WriteLine(format, version.Name);
                }
            }
        }
    }
}
```