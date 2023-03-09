//
//  @(#) InstalledVersions.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2022
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018-2023 usis GmbH. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace usis.Data.LocalDb
{
    //  -----------------------
    //  InstalledVersions class
    //  -----------------------

    internal static class InstalledVersions
    {
        //  -------------------
        //  FromRegistry method
        //  -------------------

        internal static IDictionary<Version, string> FromRegistry()
        {
            var dictionary = new SortedDictionary<Version, string>();
            ForEach((name, key) => dictionary.Add(new Version(name), key.GetValue(Constants.RegistryValueName) as string));
            return dictionary;
        }

        //  --------------
        //  ForEach method
        //  --------------

        public static void ForEach(Action<string, RegistryKey> action)
        {
            ForEachName((parentKey, name) =>
            {
                using var versionKey = parentKey.OpenSubKey(name);
                action.Invoke(name, versionKey);
            });
        }

        //  ------------------
        //  ForEachName method
        //  ------------------

        private static void ForEachName(Action<RegistryKey, string> action)
        {
            using var registryKey = Registry.LocalMachine.OpenSubKey(Constants.ProductRegistryKeyPath);
            if (registryKey == null) return;
            using var subKey = registryKey.OpenSubKey(Constants.RegistrySubKeyName);
            if (subKey == null) return;
            foreach (var name in subKey.GetSubKeyNames())
            {
                action.Invoke(subKey, name);
            }
        }
    }
}

// eof "InstalledVersions.cs"
