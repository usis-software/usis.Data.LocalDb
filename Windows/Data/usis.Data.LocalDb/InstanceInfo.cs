//
//  @(#) InstanceInfo.cs
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
    //  ------------------
    //  InstanceInfo class
    //  ------------------

    public class InstanceInfo
    {
        #region construction

        //  ------------
        //  construction
        //  ------------

        internal InstanceInfo(LocalDBInstanceInfo info)
        {
            Name = info.InstanceName.ToString(Encoding.Unicode);
            Exists = info.Exists;
            ConfigurationCorrupted = info.ConfigurationCorrupted;
            IsRunning = info.IsRunning;
            Version = new Version((int)info.Major, (int)info.Minor, (int)info.Build, (int)info.Revision);
            LastStart = info.LastStartUTC.ToDateTime();
            Connection = info.Connection.ToString(Encoding.Unicode);
            IsShared = info.IsShared;
            Owner = info.OwnerSID.ToString(Encoding.Unicode);
            IsAutomatic = info.IsAutomatic;
        }

        #endregion construction

        public string Name { get; }

        public bool Exists { get; } 

        public bool ConfigurationCorrupted { get; }

        public bool IsRunning { get; }

        public Version Version { get; }

        public DateTime LastStart { get; }

        public string Connection { get; }

        public bool IsShared { get; }

        public string Owner { get; }

        public bool IsAutomatic { get; }

        #region overrides

        //  ---------------
        //  ToString method
        //  ---------------

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>

        public override string ToString() => Name;

        #endregion overrides
    }
}

// eof "InstanceInfo.cs"
