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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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

        #endregion fields

        #region delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBFormatMessage(uint hr, int flags, int languageId, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder wszMessage, ref int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetVersions(IntPtr versionNames, ref int numberOfVersions);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetVersionInfo([MarshalAs(UnmanagedType.LPWStr)] string versionName, [MarshalAs(UnmanagedType.Struct)] out LocalDBVersionInfo versionInfo, int versionInfoSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint LocalDBGetInstances(IntPtr instanceNames, ref int numberOfInstances);

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
            var size = Marshal.SizeOf<LocalDBVersionInfo>();
            ValidateHResult(function(version, out info, size));
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

        #region TODO

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CreateInstance(/*string version, string instanceName*/)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public InstanceInfo GetInstanceInfo(/*string instanceName*/)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void ShareInstance(/*string owner, string instancePrivateName, string instanceSharedName*/)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string StartInstance(/*string instanceName*/)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string UnshareInstance(/*string instanceName*/)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StartTracing()
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StopTracing()
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StopInstance(/*string instanceName, int flags, int timeout*/)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void DeleteInstance(/*string instanceName*/)
        {
            throw new NotImplementedException();
        }

        #endregion TODO

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
