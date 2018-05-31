using System.Runtime.InteropServices;

namespace usis.Data.LocalDb
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LocalDBVersionInfo
    {
        // Contains the size of the LocalDBVersionInfo struct
        internal uint LocalDBVersionInfoSize;

        // Holds the version name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Constants.MAX_LOCALDB_VERSION_LENGTH + 1) * sizeof(char))]
        internal byte[] Version;

        // TRUE if the instance files exist on disk, FALSE otherwise
        internal bool Exists;

        // Holds the LocalDB version for the instance in the format: major.minor.build.revision  
        internal uint Major;
        internal uint Minor;
        internal uint Build;
        internal uint Revision;
    }
}
