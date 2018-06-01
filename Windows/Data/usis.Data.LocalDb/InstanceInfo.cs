﻿//
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

    /// <summary>
    /// Provides information about a SQL Server Express LocalDB instance.
    /// </summary>

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
            SharedInstanceName = info.SharedInstanceName.ToString(Encoding.Unicode);
            Owner = info.OwnerSID.ToString(Encoding.Unicode);
            IsAutomatic = info.IsAutomatic;
        }

        #endregion construction

        #region properties

        /// <summary>
        /// Gets the instance name.
        /// </summary>
        /// <value>
        /// The instance name.
        /// </value>

        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the instance files exist on disk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the instance files exist on disk; otherwise, <c>false</c>.
        /// </value>

        public bool Exists { get; }

        /// <summary>
        /// Gets a value indicating whether the instance configuration registry is corrupted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the instance configuration registry is corrupted; otherwise, <c>false</c>.
        /// </value>

        public bool ConfigurationCorrupted { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>

        public bool IsRunning { get; }

        /// <summary>
        /// Gets the LocalDB version for the instance.
        /// </summary>
        /// <value>
        /// The LocalDB version for the instance.
        /// </value>

        public Version Version { get; }

        /// <summary>
        /// Gets the date and time when the instance was started for the last time.
        /// </summary>
        /// <value>
        /// The date and time when the instance was started for the last time.
        /// </value>

        public DateTime LastStart { get; }

        /// <summary>
        /// Gets the name of the TDS named pipe to connect to the instance.
        /// </summary>
        /// <value>
        /// The name of the TDS named pipe to connect to the instance.
        /// </value>

        public string Connection { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is shared.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shared; otherwise, <c>false</c>.
        /// </value>

        public bool IsShared { get; }

        /// <summary>
        /// Gets the shared name for the instance (if the instance is shared).
        /// </summary>
        /// <value>
        /// The shared name for the instance or <c>null</c> if the instance is not shared.
        /// </value>

        public string SharedInstanceName { get; }

        /// <summary>
        /// Gets the SID of the instance owner (if the instance is shared).
        /// </summary>
        /// <value>
        /// The SID of the instance owner.
        /// </value>

        public string Owner { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is automatic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is automatic; otherwise, <c>false</c>.
        /// </value>

        public bool IsAutomatic { get; }

        #endregion properties

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
