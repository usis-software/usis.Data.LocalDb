//
//  @(#) Manager.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using System;
using System.Collections.Generic;
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
    /// <seealso cref="IDisposable" />

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

        #endregion fields

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

        #endregion delegates

        #region construction

        //  ------------
        //  construction
        //  ------------

        private Manager(string path) => library = new NativeLibraryHandle(path);

        #endregion construction

        #region IDisposable implementation

        //  --------------
        //  Dispose method
        //  --------------

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>

        public void Dispose() => library.Dispose();

        #endregion IDisposable implementation

        #region methods

        //  -------------
        //  Create method
        //  -------------

        /// <summary>
        /// Creates a new instance of the <see cref="Manager"/> class.
        /// </summary>
        /// <returns>A newly created instance of the <see cref="Manager"/> class.</returns>

        public static Manager Create()
        {
            var dictionary = new SortedDictionary<Version, string>();
            InstalledVersions.ForEach((name, key) => dictionary.Add(new Version(name), key.GetValue(Constants.RegistryValueName) as string));
            return new Manager(dictionary.LastOrDefault().Value);
        }

        //  ------------------
        //  GetVersions method
        //  ------------------

        /// <summary>
        /// Returns all SQL Server Express LocalDB versions available on the computer.
        /// </summary>
        /// <returns>
        /// An array that contains the names of the LocalDB versions that are available on the user’s workstation.
        /// </returns>

        public string[] GetVersions()
        {
            var function = library.GetFunction(nameof(LocalDBGetVersions), ref localDBGetVersions);
            int count = 0;
            if (!ValidateHResult(function(IntPtr.Zero, ref count), Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER))
            {
                var size = (Constants.MAX_LOCALDB_VERSION_LENGTH + 1) * sizeof(char);
                var pVersions = Marshal.AllocHGlobal(size * count);
                try
                {
                    ValidateHResult(function(pVersions, ref count));
                    return pVersions.Enumerate(count, size, ptr => Marshal.PtrToStringAuto(ptr)).ToArray();
                }
                finally
                {
                    Marshal.FreeHGlobal(pVersions);
                }
            }
            return new string[0];
        }

        //  ---------------------
        //  GetVersionInfo method
        //  ---------------------

        public VersionInfo GetVersionInfo(string version)
        {
            var function = library.GetFunction(nameof(LocalDBGetVersionInfo), ref localDBGetVersionInfo);
            var info = new LocalDBVersionInfo();
            ValidateHResult(function(version, out info, Marshal.SizeOf<LocalDBVersionInfo>()));
            return new VersionInfo(info);
        }

        //  -------------------
        //  GetInstances method
        //  -------------------

        public string[] GetInstances()
        {
            var function = library.GetFunction(nameof(LocalDBGetInstances), ref localDBGetInstances);
            int count = 0;
            if (!ValidateHResult(function(IntPtr.Zero, ref count), Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER))
            {
                var size = (Constants.MAX_LOCALDB_INSTANCE_NAME_LENGTH + 1) * sizeof(char);
                var pVersions = Marshal.AllocHGlobal(size * count);
                try
                {
                    ValidateHResult(function(pVersions, ref count));
                    return pVersions.Enumerate(count, size, ptr => Marshal.PtrToStringAuto(ptr)).ToArray();
                }
                finally
                {
                    Marshal.FreeHGlobal(pVersions);
                }
            }
            return new string[0];
        }

        //  ----------------------
        //  GetInstanceInfo method
        //  ----------------------

        public InstanceInfo GetInstanceInfo(string instanceName)
        {
            var info = new LocalDBInstanceInfo();
            ValidateHResult(library.GetFunction(nameof(LocalDBGetInstanceInfo), ref localDBGetInstanceInfo)(
                instanceName, out info, Marshal.SizeOf<LocalDBInstanceInfo>()));
            return new InstanceInfo(info);
        }

        //  ---------------------
        //  CreateInstance method
        //  ---------------------

        public void CreateInstance(string version, string instanceName)
        {
            ValidateHResult(library.GetFunction(nameof(LocalDBCreateInstance), ref localDBCreateInstance)(
                version, instanceName, 0));
        }

        //  ---------------------
        //  DeleteInstance method
        //  ---------------------

        public void DeleteInstance(string instanceName)
        {
            ValidateHResult(library.GetFunction(nameof(LocalDBDeleteInstance), ref localDBDeleteInstance)(
                instanceName, 0));
        }

        //  --------------------
        //  StartInstance method
        //  --------------------

        public string StartInstance(string instanceName)
        {
            int size = Constants.LOCALDB_MAX_SQLCONNECTION_BUFFER_SIZE + 1;
            var buffer = new StringBuilder(size);
            ValidateHResult(library.GetFunction(nameof(LocalDBStartInstance), ref localDBStartInstance)(
                instanceName, 0, buffer, ref size));
            return buffer.ToString();
        }

        //  -------------------
        //  StopInstance method
        //  -------------------

        public void StopInstance(string instanceName, StopInstanceOptions options, TimeSpan timeout)
        {
            ValidateHResult(library.GetFunction(nameof(LocalDBStopInstance), ref localDBStopInstance)(
                instanceName, (uint)options, Convert.ToUInt32(timeout.TotalSeconds)));
        }

        //  --------------------
        //  ShareInstance method
        //  --------------------

        public void ShareInstance(string owner, string instancePrivateName, string instanceSharedName)
        {
            var sid = new SecurityIdentifier(owner);
            var bytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bytes, 0);

            var ownerSID = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, ownerSID, bytes.Length);

                ValidateHResult(library.GetFunction(nameof(LocalDBShareInstance), ref localDBShareInstance)(
                    ownerSID, instancePrivateName, instanceSharedName, 0));
            }
            finally
            {
                Marshal.FreeHGlobal(ownerSID);
            }
        }

        //  ----------------------
        //  UnshareInstance method
        //  ----------------------

        public void UnshareInstance(string instanceName)
        {
            ValidateHResult(library.GetFunction(nameof(LocalDBUnshareInstance), ref localDBUnshareInstance)(
                instanceName, 0));
        }

        //  -------------------
        //  StartTracing method
        //  -------------------

        /// <summary>
        /// Enables tracing of API calls for all the SQL Server Express LocalDB instances owned by the current Windows user.
        /// </summary>

        public void StartTracing() => ValidateHResult(library.GetFunction(nameof(LocalDBStartTracing), ref localDBStartTracing)());

        //  ------------------
        //  StopTracing method
        //  ------------------

        /// <summary>
        /// Disables tracing of API calls for all the SQL Server Express LocalDB instances owned by the current Windows user.
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
            int size = 0;
            var builder = new StringBuilder();
            var result = function(hr, flags, 0, builder, ref size);
            if (result == Constants.LOCALDB_ERROR_INSUFFICIENT_BUFFER)
            {
                builder.Capacity = size;
                result = function(hr, flags, 0, builder, ref size);
                if (result == 0) return builder.ToString();
            }
            string message;
            switch (result)
            {
                case Constants.LOCALDB_ERROR_INVALID_PARAMETER:
                    message = Strings.InvalidParameter;
                    break;
                case Constants.LOCALDB_ERROR_UNKNOWN_ERROR_CODE:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UnknownErrorCode, hr);
                case Constants.LOCALDB_ERROR_UNKNOWN_LANGUAGE_ID:
                    message = Strings.UnknownLanguageId;
                    break;
                case Constants.LOCALDB_ERROR_INTERNAL_ERROR:
                    message = Strings.InternalError;
                    break;
                default:
                    message = string.Format(CultureInfo.CurrentCulture, Strings.ErrorCode, result);
                    break;
            }
            return string.Format(CultureInfo.CurrentCulture, Strings.FailedToRetrieveMessage, hr, message);
        }

        //  ----------------------
        //  ValidateHResult method
        //  ----------------------

        private bool ValidateHResult(uint hr, params uint[] values)
        {
            return hr.ValidateHResult(error => new LocalDbException(error, FormatMessage(hr, 0)), values);
        }

        #endregion private methods

        #endregion methods
    }
}

// eof "Manager.cs"
