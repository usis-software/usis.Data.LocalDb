//
//  @(#) NativeLibraryHandle.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace usis.Data.LocalDb
{
    //  -------------------------
    //  NativeLibraryHandle class
    //  -------------------------

    internal class NativeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region construction

        //  ------------
        //  construction
        //  ------------

        public NativeLibraryHandle(string fileName) : base(true)
        {
            var handle = NativeMethods.LoadLibraryEx(fileName, IntPtr.Zero, 0);
            if (handle == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            SetHandle(handle);
        }

        #endregion construction

        #region overrides

        //  --------------------
        //  ReleaseHandle method
        //  --------------------

        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(handle);
        }

        #endregion overrides
    }
}

// eof "NativeLibraryHandle.cs"
