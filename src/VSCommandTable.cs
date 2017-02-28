namespace FileDiffer
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidPackageString = "6e490dec-1b23-471e-8120-f164af6b268a";
        public const string guidDiffFilesCmdSetString = "5034b97c-760a-45e5-a15d-d86dcfae06f7";
        public const string guidImagesString = "ea57ba5f-bab9-4639-bac5-77c2f655fa44";
        public static Guid guidPackage = new Guid(guidPackageString);
        public static Guid guidDiffFilesCmdSet = new Guid(guidDiffFilesCmdSetString);
        public static Guid guidImages = new Guid(guidImagesString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int MyMenuGroup = 0x1020;
        public const int DiffFilesCommandId = 0x0100;
        public const int bmpPic1 = 0x0001;
    }
}
