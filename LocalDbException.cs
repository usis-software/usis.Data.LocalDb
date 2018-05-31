//
//  @(#) LocalDbException.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018 usis GmbH. All rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace usis.Data.LocalDb
{
    //  ----------------------
    //  LocalDbException class
    //  ----------------------

    [Serializable]
    public class LocalDbException : Win32Exception
    {
        public LocalDbException() { }

        public LocalDbException(string message) : base(message) { }

        public LocalDbException(string message, Exception innerException) : base(message, innerException) { }

        protected LocalDbException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        internal LocalDbException(uint hr, string message) : base((int)hr, message) { }
    }
}

// eof "LocalDbException.cs"
