//
//  @(#) Manager.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2022
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018-2023 usis GmbH. All rights reserved.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace usis.Data.LocalDb
{
    //  -------------
    //  Manager class
    //  -------------

    /// <summary>
    /// Provides methods to manage LocalDB instances.
    /// </summary>
    /// <seealso cref="IDisposable"/>

    public sealed class Manager : IDisposable
    {
        #region fields

        private readonly NativeLibraryHandle library;

        private LocalDBFormatMessage localDBFormatMessage;
        private LocalDBGetVersions localDBGetVersions;
        private LocalDBGetVersionInfo localDBGetVersionInfo;
        private LocalDBGetInstances localDBGetInstances;
        private LocalDBGetInstanceInfo localDBGetInstanceInfo;
        private LocalDBCreateInstance localDBCreateInstance;
        private LocalDBDeleteInstance localDBDeleteInstance;
        private LocalDBStartInstance localDBStartInstance;
        private LocalDBStopInstance localDBStopInstance;
        private LocalDBShareInstance localDBShareInstance;
        private LocalDBUnshareInstance localDBUnshareInstance;
        private LocalDBStartTracing localDBStartTracing;
        private LocalDBStopTracing localDBStopTracing;

        #endregion

        #region delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBFormatMessage(uint hr, int flags, int languageId, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder message, ref int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetVersions(IntPtr versionNames, ref int numberOfVersions);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetVersionInfo([MarshalAs(UnmanagedType.LPWStr)] string versionName, [MarshalAs(UnmanagedType.Struct)] out LocalDBVersionInfo versionInfo, int versionInfoSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetInstances(IntPtr instanceNames, ref int numberOfInstances);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetInstanceInfo([MarshalAs(UnmanagedType.LPWStr)] string instanceName, [MarshalAs(UnmanagedType.Struct)] out LocalDBInstanceInfo instanceInfo, int instanceInfoSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBCreateInstance([MarshalAs(UnmanagedType.LPWStr)] string version, [MarshalAs(UnmanagedType.LPWStr)] string instanceName, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBDeleteInstance([MarshalAs(UnmanagedType.LPWStr)] string instanceName, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBStartInstance([MarshalAs(UnmanagedType.LPWStr)] string instanceName, uint flags, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder sqlConnection, ref int sqlConnectionSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBStopInstance([MarshalAs(UnmanagedType.LPWStr)] string instanceName, uint flags, uint timeout);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBShareInstance(IntPtr ownerSID, [MarshalAs(UnmanagedType.LPWStr)] string instancePrivateName, [MarshalAs(UnmanagedType.LPWStr)] string instanceSharedName, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBUnshareInstance([MarshalAs(UnmanagedType.LPWStr)] string instanceSharedName, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBStartTracing();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBStopTracing();

        #endregion

        #region construction

        //  ------------
        //  construction
        //  ------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Manager"/> class.
        /// </summary>
        /// <param name="path">The path of the LocalDB API library.</param>

        private Manager(string path) => library = new NativeLibraryHandle(path);

        #endregion

        #region IDisposable implementation

        //  --------------
        //  Dispose method
        //  --------------

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>

        public void Dispose() => library.Dispose();

        #endregion

        #region methods

        //  ------------------
        //  IsInstalled method
        //  ------------------

        /// <summary>
        /// Determines whether SQL Server Express LocalDB is installed on the
        /// computer.
        /// </summary>
        /// <returns><c>true</c> if SQL Server Express LocalDB is installed on
        ///     the computer; otherwise, <c>false</c>.</returns>

        public static bool IsInstalled() => InstalledVersions.FromRegistry().Count > 0;

        //  -------------
        //  Create method
        //  -------------

        /// <summary>
        /// Creates a new instance of the <see cref="Manager"/> class.
        /// </summary>
        /// <returns>A newly created instance of the <see cref="Manager"/>
        ///     class.</returns>

        public static Manager Create()
        {
            var dictionary = InstalledVersions.FromRegistry();
            return dictionary.Count == 0 ? throw new LocalDbException(Strings.NotInstalled) : new Manager(dictionary.LastOrDefault().Value);
        }

        //  ------------------
        //  GetVersions method
        //  ------------------

        /// <summary>
        /// Returns all SQL Server Express LocalDB versions available on the
        /// computer.
        /// </summary>
        /// <returns>An array that contains the names of the LocalDB versions
        ///     that are available on the user’s workstation.</returns>

        public string[] GetVersions()
        {
            var function = library.GetFunction(nameof(LocalDBGetVersions), ref localDBGetVersions);
            var count = 0;
            if (!ValidateHResult(function(IntPtr.Zero, ref count), Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER))
            {
                var size = (Constants.MAX_LOCALDB_VERSION_LENGTH + 1) * sizeof(char);
                var pVersions = Marshal.AllocHGlobal(size * count);
                try
                {
                    if (ValidateHResult(function(pVersions, ref count)))
                    {
                        return [.. pVersions.Enumerate(count, size, Marshal.PtrToStringAuto)];
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(pVersions);
                }
            }
            return [];
        }

        //  ---------------------
        //  GetVersionInfo method
        //  ---------------------

        /// <summary>
        /// Gets information for the specified SQL Server Express LocalDB
        /// version, such as whether it exists and the full LocalDB version
        /// number (including build and release numbers).
        /// </summary>
        /// <param name="version">The LocalDB version name.</param>
        /// <returns>The information about the specified LocalDB version.
        ///     </returns>

        public VersionInfo GetVersionInfo(string version)
        {
            var info = new LocalDBVersionInfo();
            var hr = library.GetFunction(nameof(LocalDBGetVersionInfo), ref localDBGetVersionInfo)(version, out info, Marshal.SizeOf(typeof(LocalDBVersionInfo)));
            return ValidateHResult(hr) ? new VersionInfo(info) : null;
        }

        //  -------------------
        //  GetInstances method
        //  -------------------

        /// <summary>
        /// Gets the names of both named and default LocalDB instances on the
        /// user’s workstation.
        /// </summary>
        /// <returns>An array that contains the names of both named and default LocalDB instances on the user’s workstation.</returns>

        /// <example>
        ///using System;
        ///
        ///namespace usis.Data.LocalDb.Samples
        ///{
        ///    public static class Operations
        ///    {
        ///        public static void ListInstances()
        ///        {
        ///            using (var manager = Manager.Create())
        ///            {
        ///                foreach (var instance in manager.GetInstances())
        ///                {
        ///                    Console.WriteLine(instance);
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </example>

        public string[] GetInstances(double timeout = 3000)
        {
            var function = library.GetFunction(nameof(LocalDBGetInstances), ref localDBGetInstances);
            var count = 0;
            if (!ValidateHResult(function(IntPtr.Zero, ref count), Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER))
            {
                var stopWatch = Stopwatch.StartNew();
                do
                {
                    var size = (Constants.MAX_LOCALDB_INSTANCE_NAME_LENGTH + 1) * sizeof(char);
                    var pInstances = Marshal.AllocHGlobal(size * count);
                    try
                    {
                        if (ValidateHResult(function(pInstances, ref count), Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER))
                        {
                            return [.. pInstances.Enumerate(count, size, Marshal.PtrToStringAuto)];
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pInstances);
                    }
                }
                while (stopWatch.Elapsed.TotalMilliseconds < timeout || double.IsInfinity(timeout));
                throw new TimeoutException();
            }
            return [];
        }

        //  ----------------------
        //  GetInstanceInfo method
        //  ----------------------

        /// <summary>
        /// Gets information for the specified SQL Server Express LocalDB
        /// instance, such as whether it exists, the LocalDB version it uses,
        /// whether it is running, and so on.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <returns>The information about the specified LocalDB instance.
        ///     </returns>

        public InstanceInfo GetInstanceInfo(string instanceName)
        {
            var info = new LocalDBInstanceInfo();
            var hr = library.GetFunction(nameof(LocalDBGetInstanceInfo), ref localDBGetInstanceInfo)(instanceName, out info, Marshal.SizeOf(typeof(LocalDBInstanceInfo)));
            return ValidateHResult(hr) ? new InstanceInfo(info) : null;
        }

        //  ---------------------
        //  CreateInstance method
        //  ---------------------

        /// <summary>
        /// Creates a new SQL Server Express LocalDB instance.
        /// </summary>
        /// <param name="version">The LocalDB version, for example <c>11.0</c>
        ///     or <c>11.0.1094.2</c>.</param>
        /// <param name="instanceName">The name for the LocalDB instance to
        ///     create.</param>

        public void CreateInstance(string version, string instanceName) => _ = ValidateHResult(library.GetFunction(nameof(LocalDBCreateInstance), ref localDBCreateInstance)(version, instanceName, 0));

        //  ---------------------
        //  DeleteInstance method
        //  ---------------------

        /// <summary>
        /// Removes the specified SQL Server Express LocalDB instance.
        /// </summary>
        /// <param name="instanceName">The name of the LocalDB instance to
        ///     remove.</param>

        public void DeleteInstance(string instanceName) => ValidateHResult(library.GetFunction(nameof(LocalDBDeleteInstance), ref localDBDeleteInstance)(instanceName, 0));

        //  --------------------
        //  StartInstance method
        //  --------------------

        /// <summary>
        /// Starts the specified SQL Server Express LocalDB instance.
        /// </summary>
        /// <param name="instanceName">The name of the LocalDB instance to
        ///     start.</param>
        /// <returns>The name of the TDS named pipe to connect to the instance.
        ///     </returns>

        public string StartInstance(string instanceName)
        {
            var size = Constants.LOCALDB_MAX_SQLCONNECTION_BUFFER_SIZE + 1;
            var buffer = new StringBuilder(size);
            var hr = library.GetFunction(nameof(LocalDBStartInstance), ref localDBStartInstance)(instanceName, 0, buffer, ref size);
            return ValidateHResult(hr) ? buffer.ToString() : null;
        }

        //  -------------------
        //  StopInstance method
        //  -------------------

        /// <summary>
        /// Stops the specified SQL Server Express LocalDB instance from
        /// running.
        /// </summary>
        /// <param name="instanceName">The name of the LocalDB instance to stop.
        ///     </param>
        /// <param name="options">One or a combination of the option values
        ///     specifying the way to stop the instance.</param>
        /// <param name="timeout">The time to wait for this operation to
        ///     complete. If this value is <c>0</c>, this function will return
        ///     immediately without waiting for the LocalDB instance to stop.
        ///     </param>

        public void StopInstance(string instanceName, StopInstanceOptions options, TimeSpan timeout)
        {
            // if timeout is 0, the function returns immediately with an error code
            var t = Convert.ToUInt32(timeout.TotalSeconds);
            var hr = library.GetFunction(nameof(LocalDBStopInstance), ref localDBStopInstance)(instanceName, (uint)options, t);
            _ = ValidateHResult(hr, t == 0 ? Constants.LOCALDB_ERROR_WAIT_TIMEOUT : 0);
        }

        //  --------------------
        //  ShareInstance method
        //  --------------------

        /// <summary>
        /// Shares the specified SQL Server Express LocalDB instance with other
        /// users of the computer, using the specified shared name.
        /// </summary>
        /// <param name="owner">The SID of the instance owner.</param>
        /// <param name="instancePrivateName">The private name for the LocalDB
        ///     instance to share.</param>
        /// <param name="instanceSharedName">The shared name for the LocalDB
        ///     instance to share.</param>

        public void ShareInstance(string owner, string instancePrivateName, string instanceSharedName)
        {
            var sid = new SecurityIdentifier(owner);
            var bytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bytes, 0);

            var ownerSID = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, ownerSID, bytes.Length);
                var hr = library.GetFunction(nameof(LocalDBShareInstance), ref localDBShareInstance)(ownerSID, instancePrivateName, instanceSharedName, 0);
                _ = ValidateHResult(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(ownerSID);
            }
        }

        //  ----------------------
        //  UnshareInstance method
        //  ----------------------

        /// <summary>
        /// Stops the sharing of the specified SQL Server Express LocalDB
        /// instance.
        /// </summary>
        /// <param name="instanceName">The shared name for the LocalDB instance
        ///     to unshare.</param>

        public void UnshareInstance(string instanceName) => ValidateHResult(library.GetFunction(nameof(LocalDBUnshareInstance), ref localDBUnshareInstance)(instanceName, 0));

        //  -------------------
        //  StartTracing method
        //  -------------------

        /// <summary>
        /// Enables tracing of API calls for all the SQL Server Express LocalDB
        /// instances owned by the current Windows user.
        /// </summary>

        public void StartTracing() => ValidateHResult(library.GetFunction(nameof(LocalDBStartTracing), ref localDBStartTracing)());

        //  ------------------
        //  StopTracing method
        //  ------------------

        /// <summary>
        /// Disables tracing of API calls for all the SQL Server Express LocalDB
        /// instances owned by the current Windows user.
        /// </summary>

        public void StopTracing() => ValidateHResult(library.GetFunction(nameof(LocalDBStopTracing), ref localDBStopTracing)());

        #region private methods

        //  --------------------
        //  FormatMessage method
        //  --------------------

        private string FormatMessage(uint hr, int flags)
        {
            if (hr == Constants.LOCALDB_ERROR_NOT_INSTALLED) return Strings.NotInstalled;

            var function = library.GetFunction(nameof(LocalDBFormatMessage), ref localDBFormatMessage);
            var size = 0;
            var builder = new StringBuilder();
            var result = function(hr, flags, 0, builder, ref size);
            if (result == Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER)
            {
                builder.Capacity = size;
                result = function(hr, flags, 0, builder, ref size);
                if (result == 0) return builder.ToString();
            }
            var message = MessageForError(result, hr) ?? string.Format(CultureInfo.CurrentCulture, Strings.ErrorCode, result);
            return string.Format(CultureInfo.CurrentCulture, Strings.FailedToRetrieveMessage, hr, message);

            //  ----------------------
            //  MessageForError method
            //  ----------------------

            static string MessageForError(uint error, uint hr) => error switch
            {
                Constants.LOCALDB_ERROR_NOT_INSTALLED => Strings.NotInstalled,
                Constants.LOCALDB_ERROR_INVALID_PARAMETER => Strings.InvalidParameter,
                Constants.LOCALDB_ERROR_UNKNOWN_ERROR_CODE => string.Format(CultureInfo.CurrentCulture, Strings.UnknownErrorCode, hr),
                Constants.LOCALDB_ERROR_UNKNOWN_LANGUAGE_ID => Strings.UnknownLanguageId,
                Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER => Strings.InsufficientBuffer,
                Constants.LOCALDB_ERROR_INTERNAL_ERROR => Strings.InternalError,
                _ => string.Empty,
            };
        }

        //  ----------------------
        //  ValidateHResult method
        //  ----------------------

        private bool ValidateHResult(uint hr, params uint[] values) => hr.ValidateHResult(error => new LocalDbException(error, FormatMessage(hr, 0)), values);

        #endregion

        #endregion
    }
}

// eof "Manager.cs"
