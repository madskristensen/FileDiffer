using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    [Guid(PackageGuids.guidPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuid)]
    [ProvideUIContextRule(UIContextGuid,
        name: "Test auto load",
        expression: "(SingleProject | MultipleProjects)",
        termNames: new[] { "SingleProject", "MultipleProjects" },
        termValues: new[] { VSConstants.UICONTEXT.SolutionHasSingleProject_string, VSConstants.UICONTEXT.SolutionHasMultipleProjects_string },
        delay: 2000)]
    public sealed class VSPackage : AsyncPackage
    {
        private const string UIContextGuid = "1e8c8ec3-283a-43b7-8903-c78b590eb2ee";
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await DiffFilesCommand.Initialize(this);
        }
    }
}
