using System;
using System.Text;

namespace usis.Data.LocalDb
{
    //  -----------------
    //  VersionInfo class
    //  -----------------

    public class VersionInfo
    {
        #region construction

        //  ------------
        //  construction
        //  ------------

        internal VersionInfo(LocalDBVersionInfo info)
        {
            Name = Encoding.Unicode.GetString(info.Version).TrimEnd('\0');
            Exists = info.Exists;
            Version = new Version((int)info.Major, (int)info.Minor, (int)info.Build, (int)info.Revision);
        }

        #endregion construction

        public string Name { get; }

        public bool Exists { get; }

        public Version Version { get; }

        public override string ToString() => Name;
    }
}
