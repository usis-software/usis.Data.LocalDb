//
//  @(#) VersionInfo.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using System;
using System.Text;

namespace usis.Data.LocalDb
{
    //  -----------------
    //  VersionInfo class
    //  -----------------

    public class VersionInfo
    {
        #region construction

        //  ------------
        //  construction
        //  ------------

        internal VersionInfo(LocalDBVersionInfo info)
        {
            Name = info.Version.ToString(Encoding.Unicode);
            Exists = info.Exists;
            Version = new Version((int)info.Major, (int)info.Minor, (int)info.Build, (int)info.Revision);
        }

        #endregion construction

        public string Name { get; }

        public bool Exists { get; }

        public Version Version { get; }

        public override string ToString() => Name;
    }
}

// eof "VersionInfo.cs"
