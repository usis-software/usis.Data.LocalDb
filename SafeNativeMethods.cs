//
//  @(#) SafeNativeMethods.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace usis.Data.LocalDb
{
    //  -----------------------
    //  SafeNativeMethods class
    //  -----------------------

    internal static class SafeNativeMethods
    {
        #region constants

        private const string KernelFileName = "kernel32.dll";

        #endregion constants

        //  --------------------
        //  LoadLibraryEx method
        //  --------------------

        [DllImport(KernelFileName, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibraryEx(string fileName, IntPtr reserved, uint flags);

        //  ------------------
        //  FreeLibrary method
        //  ------------------

        [DllImport(KernelFileName, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr module);

        //  ---------------------
        //  GetProcAddress method
        //  ---------------------

        [DllImport(KernelFileName, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetProcAddress(NativeLibraryHandle handle, [MarshalAs(UnmanagedType.LPStr)] string procName);
    }
}

// eof "SafeNativeMethods.cs"
