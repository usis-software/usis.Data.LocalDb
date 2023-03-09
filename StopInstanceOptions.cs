//
//  @(#) StopInstanceOptions.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2022
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018-2023 usis GmbH. All rights reserved.

using System;

namespace usis.Data.LocalDb
{
    //  -------------------------------
    //  StopInstanceOptions enumeration
    //  -------------------------------

    /// <summary>
    /// Describes to options that spectify the way to stop an instance.
    /// </summary>

    [Flags]
    public enum StopInstanceOptions
    {
        /// <summary>
        /// Shut down by using the SHUTDOWN Transact-SQL command. 
        /// </summary>

        None = 0x0,

        /// <summary>
        /// Shut down immediately using the kill process operating system
        /// command.
        /// </summary>

        ShutdownKillProcess = 0x1,

        /// <summary>
        /// Shut down using the WITH NOWAIT option Transact-SQL command.
        /// </summary>

        ShutdownWithNoWait = 0x2
    }
}

// eof "StopInstanceOptions.cs"
