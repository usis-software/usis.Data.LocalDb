//
//  @(#) InstalledVersions.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using Microsoft.Win32;
using System;

namespace usis.Data.LocalDb
{
    //  -----------------------
    //  InstalledVersions class
    //  -----------------------

    internal static class InstalledVersions
    {
        //  --------------
        //  ForEach method
        //  --------------

        public static void ForEach(Action<string, RegistryKey> action)
        {
            ForEachName((parentKey, name) =>
            {
                using (var versionKey = parentKey.OpenSubKey(name))
                {
                    action.Invoke(name, versionKey);
                }
            });
        }

        //  ------------------
        //  ForEachName method
        //  ------------------

        public static void ForEachName(Action<RegistryKey, string> action)
        {
            using (var registryKey = Registry.LocalMachine.OpenSubKey(Constants.ProductRegistryKeyPath))
            {
                if (registryKey == null) return;
                using (var subKey = registryKey.OpenSubKey(Constants.RegistrySubKeyName))
                {
                    if (subKey == null) return;
                    foreach (var name in subKey.GetSubKeyNames())
                    {
                        action.Invoke(subKey, name);
                    }
                }
            }
        }
    }
}

// eof "InstalledVersions.cs"
