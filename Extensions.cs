//
//  @(#) Extensions.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2022
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018-2023 usis GmbH. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace usis.Data.LocalDb
{
    //  ----------------
    //  Extensions class
    //  ----------------

    /// <summary>
    /// Provides extension methods for the <see cref="Manager"/> class.
    /// </summary>

    public static partial class Extensions
    {
        #region EnumerateVersions method

        //  ------------------------
        //  EnumerateVersions method
        //  ------------------------

        /// <summary>
        /// Returns all SQL Server Express LocalDB versions available on the
        /// computer.
        /// </summary>
        /// <param name="manager">The LocalDB manager object.</param>
        /// <returns>An enumerator to iterate through all version informations.
        ///     </returns>

        ///<example>
        ///<code>
        ///using System;
        ///
        ///namespace usis.Data.LocalDb.Samples
        ///{
        ///    public static class Operations
        ///    {
        ///        public static void ListVersions()
        ///        {
        ///            using (var manager = Manager.Create())
        ///            {
        ///                foreach (var version in manager.EnumerateVersions())
        ///                {
        ///                    string format;
        ///                    switch (version.Version.Major)
        ///                    {
        ///                        case 11:
        ///                            format = "Microsoft SQL Server 2012 ({0})";
        ///                            break;
        ///                        case 12:
        ///                            format = "Microsoft SQL Server 2014 ({0})";
        ///                            break;
        ///                        case 13:
        ///                            format = "Microsoft SQL Server 2016 ({0})";
        ///                            break;
        ///                        case 14:
        ///                            format = "Microsoft SQL Server 2017 ({0})";
        ///                            break;
        ///                        default:
        ///                            format = "{0}";
        ///                            break;
        ///                    }
        ///                    Console.WriteLine(format, version.Name);
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        ///</example>

        public static IEnumerable<VersionInfo> EnumerateVersions(this Manager manager)
        {
            return manager == null
                ? throw new ArgumentNullException(nameof(manager))
                : manager.GetVersions().Select(manager.GetVersionInfo);
        }

        #endregion

        #region EnumerateInstances methods

        //  --------------------------
        //  EnumerateInstances methods
        //  --------------------------

        /// <summary>
        /// Gets the informations about both named and default LocalDB instances
        /// on the user’s workstation.
        /// </summary>
        /// <param name="manager">The LocalDB manager object.</param>
        /// <returns>An enumerator to iterate through all instance informations.
        ///     </returns>

        public static IEnumerable<InstanceInfo> EnumerateInstances(this Manager manager)
        {
            return manager == null
                ? throw new ArgumentNullException(nameof(manager))
                : manager.GetInstances().Select(manager.GetInstanceInfo);
        }

        #endregion

        #region CreateInstance method

        //  ---------------------
        //  CreateInstance method
        //  ---------------------

        /// <summary>
        /// Creates a new SQL Server Express LocalDB instance with the highest
        /// installed version.
        /// </summary>
        /// <param name="manager">The LocalDB manager object.</param>
        /// <param name="instanceName">The name for the LocalDB instance to
        ///     create.</param>
        /// <exception cref="ArgumentNullException"><paramref name="manager"/>
        ///     is a <c>null</c> reference.</exception>
        /// <exception cref="LocalDbException">SQL Server Express LocalDB is not
        ///     installed on the computer.</exception>

        public static void CreateInstance(this Manager manager, string instanceName)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            var version = manager.GetVersions().OrderByDescending(s => s).FirstOrDefault() ?? throw new LocalDbException(Strings.NotInstalled);
            manager.CreateInstance(version, instanceName);
        }

        #endregion

        #region StopInstance method

        //  -------------------
        //  StopInstance method
        //  -------------------

        /// <summary>
        /// Stops the specified SQL Server Express LocalDB instance from
        /// running.
        /// </summary>
        /// <param name="manager">The LocalDB manager object.</param>
        /// <param name="instanceName">The name of the LocalDB instance to stop.
        ///     </param>
        /// <remarks>
        /// This function will return immediately without waiting for the
        /// LocalDB instance to stop.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="manager"/>
        ///     is a <c>null</c> reference.</exception>

        public static void StopInstance(this Manager manager, string instanceName)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            manager.StopInstance(instanceName, StopInstanceOptions.None, new TimeSpan(0));
        }

        /// <summary>
        /// Stops the specified SQL Server Express LocalDB instance from
        /// running.
        /// </summary>
        /// <param name="manager">The LocalDB manager object.</param>
        /// <param name="instanceName">The name of the LocalDB instance to stop.
        ///     </param>
        /// <param name="timeout">The time in seconds to wait for this operation
        ///     to complete. If this value is 0, this function will return
        ///     immediately without waiting for the LocalDB instance to stop.
        ///     </param>
        /// <exception cref="ArgumentNullException"><paramref name="manager"/>
        ///     is a <c>null</c> reference.</exception>

        public static void StopInstance(this Manager manager, string instanceName, int timeout)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            manager.StopInstance(instanceName, StopInstanceOptions.None, new TimeSpan(0, 0, timeout));
        }

        #endregion

        #region internal methods

        //  ------------------
        //  GetFunction method
        //  ------------------

        internal static T GetFunction<T>(this NativeLibraryHandle library, string procName, ref T function) where T : class
        {
            if (function == null)
            {
                var address = NativeMethods.GetProcAddress(library, procName);
                if (address == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
                function = Marshal.GetDelegateForFunctionPointer(address, typeof(T)) as T;
            }
            return function;
        }

        //  ----------------------
        //  ValidateHResult method
        //  ----------------------

        internal static bool ValidateHResult(this uint hr, Func<uint, Exception> createException, params uint[] values)
        {
            foreach (var ok in values)
            {
                if (hr == ok) return (hr & 0x80000000) == 0;
            }
            return (hr & 0x80000000) != 0 ? throw createException(hr) : true;
        }

        //  ----------------
        //  Enumerate method
        //  ----------------

        internal static IEnumerable<T> Enumerate<T>(this IntPtr pointer, int count, int size, Func<IntPtr, T> function)
        {
            for (var i = 0; i < count; i++)
            {
                var offset = new IntPtr(pointer.ToInt64() + (size * i));
                yield return function(offset);
            }
        }

        //  ---------------
        //  ToString method
        //  ---------------

        internal static string ToString(this byte[] bytes, Encoding encoding) => new([.. encoding.GetChars(bytes).TakeWhile(c => c != '\0')]);

        //  -----------------
        //  ToDateTime method
        //  -----------------

        internal static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME fileTime) => DateTimeFromFileTime(fileTime.dwHighDateTime, fileTime.dwLowDateTime);

        #endregion

        #region private methods

        //  ---------------------------
        //  DateTimeFromFileTime method
        //  ---------------------------

        private static DateTime DateTimeFromFileTime(int high, int low)
        {
            if (high == 0 && low == 0) return DateTime.MinValue;

            var fileTime = ((long)high << 32) + (uint)low;
            return DateTime.FromFileTime(fileTime);
        }

        #endregion
    }
}

// eof "Extensions.cs"
