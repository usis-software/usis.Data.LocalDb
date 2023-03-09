//
//  @(#) LocalDbException.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2022
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018-2023 usis GmbH. All rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace usis.Data.LocalDb
{
    //  ----------------------
    //  LocalDbException class
    //  ----------------------

    /// <summary>
    /// Throws an exception for a SQL Server Express LocalDB error.
    /// </summary>
    /// <seealso cref="Win32Exception"/>

    [Serializable]
    public class LocalDbException : Win32Exception
    {
        #region construction

        //  ------------
        //  construction
        //  ------------

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDbException"/>
        /// class.
        /// </summary>

        public LocalDbException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDbException"/>
        /// class.
        /// </summary>
        /// <param name="message">A detailed description of the error.</param>

        public LocalDbException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDbException"/>
        /// class.
        /// </summary>
        /// <param name="message">A detailed description of the error.</param>
        /// <param name="innerException">A reference to the inner exception that
        ///     is the cause of this exception.</param>

        public LocalDbException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDbException"/>
        /// class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> associated
        ///     with this exception.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that
        ///     represents the context of this exception.</param>

        protected LocalDbException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        internal LocalDbException(uint hr, string message) : base((int)hr, message) { }

        #endregion
    }
}

// eof "LocalDbException.cs"
