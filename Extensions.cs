//
//  @(#) Extensions.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace usis.Data.LocalDb
{
    //  ----------------
    //  Extensions class
    //  ----------------

    internal static class Extensions
    {
        //  ------------------
        //  GetFunction method
        //  ------------------

        internal static T GetFunction<T>(this NativeLibraryHandle library, string procName, ref T function)
        {
            if (function == null)
            {
                var address = SafeNativeMethods.GetProcAddress(library, procName);
                if (address == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
                function = Marshal.GetDelegateForFunctionPointer<T>(address);
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
            if ((hr & 0x80000000) != 0) throw createException(hr);
            return true;
        }

        //  ----------------
        //  Enumerate method
        //  ----------------

        internal static IEnumerable<T> Enumerate<T>(this IntPtr pointer, int count, int size, Func<IntPtr, T> function)
        {
            for (int i = 0; i < count; i++)
            {
                var offset = new IntPtr(pointer.ToInt64() + size * i);
                yield return function(offset);
            }
        }

        //  ---------------
        //  ToString method
        //  ---------------

        internal static string ToString(this byte[] bytes, Encoding encoding) => new string(encoding.GetChars(bytes).TakeWhile(c => c != '\0').ToArray());

        //  -----------------
        //  ToDateTime method
        //  -----------------

        internal static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME fileTime) => DateTimeFromFileTime(fileTime.dwHighDateTime, fileTime.dwLowDateTime);

        #region private methods

        //  ---------------------------
        //  DateTimeFromFileTime method
        //  ---------------------------

        private static DateTime DateTimeFromFileTime(int high, int low)
        {
            if (high == 0 && low == 0) return DateTime.MinValue;

            long fileTime = ((long)high << 32) + (uint)low;
            return DateTime.FromFileTime(fileTime);
        }

        #endregion private methods
    }
}

// eof "Extensions.cs"
