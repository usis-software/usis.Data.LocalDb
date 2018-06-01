//
//  @(#) Structures.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using System.Runtime.InteropServices;

namespace usis.Data.LocalDb
{
    #region LocalDBVersionInfo structure

    //  ----------------------------
    //  LocalDBVersionInfo structure
    //  ----------------------------

    [StructLayout(LayoutKind.Sequential)]
    internal struct LocalDBVersionInfo
    {
        // contains the size of the LocalDBVersionInfo struct
        internal uint LocalDBVersionInfoSize;

        // holds the version name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Constants.MAX_LOCALDB_VERSION_LENGTH + 1) * sizeof(char))]
        internal byte[] Version;

        // TRUE if the instance files exist on disk, FALSE otherwise
        internal bool Exists;

        // holds the LocalDB version for the instance in the format: major.minor.build.revision  
        internal uint Major;
        internal uint Minor;
        internal uint Build;
        internal uint Revision;
    }

    #endregion LocalDBVersionInfo structure

    #region LocalDBInstanceInfo structure

    //  -----------------------------
    //  LocalDBInstanceInfo structure
    //  -----------------------------

    [StructLayout(LayoutKind.Sequential)]
    internal struct LocalDBInstanceInfo
    {
        // contains the size of the LocalDBInstanceInfo struct  
        internal uint LocalDBInstanceInfoSize;

        // holds the instance name  
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Constants.MAX_LOCALDB_INSTANCE_NAME_LENGTH + 1) * sizeof(char))]
        internal byte[] InstanceName;

        // TRUE if the instance files exist on disk, FALSE otherwise  
        internal bool Exists;

        // TRUE if the instance configuration registry is corrupted, FALSE otherwise  
        internal bool ConfigurationCorrupted;

        // TRUE if the instance is running at the moment, FALSE otherwise  
        internal bool IsRunning;

        // holds the LocalDB version for the instance in the format: major.minor.build.revision  
        internal uint Major;
        internal uint Minor;
        internal uint Build;
        internal uint Revision;

        // holds the date and time when the instance was started for the last time  
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastStartUTC;

        // holds the name of the TDS named pipe to connect to the instance  
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.LOCALDB_MAX_SQLCONNECTION_BUFFER_SIZE * sizeof(char))]
        internal byte[] Connection;

        // TRUE if the instance is shared, FALSE otherwise  
        internal bool IsShared;

        // holds the shared name for the instance (if the instance is shared)  
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Constants.MAX_LOCALDB_INSTANCE_NAME_LENGTH + 1) * sizeof(char))]
        internal byte[] SharedInstanceName;

        // holds the SID of the instance owner (if the instance is shared)  
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Constants.MAX_STRING_SID_LENGTH + 1) * sizeof(char))]
        internal byte[] OwnerSID;

        // TRUE if the instance is Automatic, FALSE otherwise  
        internal bool IsAutomatic;
    }

    #endregion LocalDBInstanceInfo structure
}

// eof "Structures.cs"
