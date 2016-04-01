using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace FileDiffer
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidDiffFilesCommandPackageString)]
    public sealed class VSPackage : Package
    {
        protected override void Initialize()
        {
            DiffFilesCommand.Initialize(this);
            base.Initialize();
        }

    }
}
