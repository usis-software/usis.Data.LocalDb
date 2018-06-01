//
//  @(#) Constants.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

namespace usis.Data.LocalDb
{
    //  ---------------
    //  Constants class
    //  ---------------

    internal static class Constants
    {
        internal const string ProductRegistryKeyPath = @"SOFTWARE\Microsoft\Microsoft SQL Server Local DB";
        internal const string RegistrySubKeyName = "Installed Versions";
        internal const string RegistryValueName = "InstanceAPIPath";

        internal const int MAX_LOCALDB_VERSION_LENGTH = 43;
        internal const int MAX_LOCALDB_INSTANCE_NAME_LENGTH = 128;
        internal const int LOCALDB_MAX_SQLCONNECTION_BUFFER_SIZE = 260;
        internal const int MAX_STRING_SID_LENGTH = 186;

        internal const uint LOCALDB_ERROR_INVALID_PARAMETER = 0x89C50101;
        internal const uint LOCALDB_ERROR_INTERNAL_ERROR = 0x89C50108;
        internal const uint LOCALDB_ERROR_INSUFFICIENT_BUFFER = 0x89C50114;
        internal const uint LOCALDB_ERROR_NOT_INSTALLED = 0x89C50116;
        internal const uint LOCALDB_ERROR_UNKNOWN_ERROR_CODE = 0x89C50110;
        internal const uint LOCALDB_ERROR_UNKNOWN_LANGUAGE_ID = 0x89C5010E;
    }
}

// eof "Constants.cs"
