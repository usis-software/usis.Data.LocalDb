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
            if ((hr & 0x800000000) != 0) throw createException(hr);
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
    }
}

// eof "Extensions.cs"
