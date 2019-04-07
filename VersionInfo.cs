//
//  @(#) VersionInfo.cs
//
//  Project:    usis.Data.LocalDb
//  System:     Microsoft Visual Studio 2019
//  Author:     Udo Schäfer
//
//  Copyright (c) 2018,2019 usis GmbH. All rights reserved.

using System;
using System.Text;

namespace usis.Data.LocalDb
{
    //  -----------------
    //  VersionInfo class
    //  -----------------

    /// <summary>
    /// Provides information about a SQL Server Express LocalDB version.
    /// </summary>

    public class VersionInfo : IEquatable<VersionInfo>
    {
        #region construction

        //  ------------
        //  construction
        //  ------------

        internal VersionInfo(LocalDBVersionInfo info)
        {
            Name = info.Version.ToString(Encoding.Unicode);
            Exists = info.Exists;
            Version = new Version((int)info.Major, (int)info.Minor, (int)info.Build, (int)info.Revision);
        }

        #endregion construction

        #region properties

        /// <summary>
        /// Gets the LocalDB version name.
        /// </summary>
        /// <value>
        /// The LocalDB version name.
        /// </value>

        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the instance files exist on disk.
        /// </summary>
        /// <value>
        /// <c>true</c> if the instance files exist on disk; otherwise,
        /// <c>false</c>.
        /// </value>

        public bool Exists { get; }

        /// <summary>
        /// Gets the LocalDB version for the instance.
        /// </summary>
        /// <value>
        /// The LocalDB version for the instance.
        /// </value>

        public Version Version { get; }

        #endregion properties

        #region overrides

        //  ---------------
        //  ToString method
        //  ---------------

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.
        ///     </returns>

        public override string ToString() => Name;

        //  -------------
        //  Equals method
        //  -------------

        /// <summary>
        /// Determines whether the specified <see cref="object"/>, is equal to
        /// this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this
        ///     instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal
        ///     to this instance; otherwise, <c>false</c>.</returns>

        public override bool Equals(object obj) => IsEqualTo(obj as VersionInfo);

        //  ------------------
        //  GetHashCode method
        //  ------------------

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing
        ///     algorithms and data structures like a hash table.</returns>

        public override int GetHashCode() => Version.GetHashCode();

        #endregion overrides

        #region IEquatable<VersionInfo> implementation

        //  -------------
        //  Equals method
        //  -------------

        bool IEquatable<VersionInfo>.Equals(VersionInfo other) => IsEqualTo(other);

        #endregion IEquatable<VersionInfo> implementation

        #region private methods

        //  ----------------
        //  IsEqualTo method
        //  ----------------

        private bool IsEqualTo(VersionInfo other) => other == null ? false : Version.Equals(other.Version);

        #endregion private methods
    }
}

// eof "VersionInfo.cs"
