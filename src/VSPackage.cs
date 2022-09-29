using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidPackageString)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class FileDifferPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            await SelectedFilesCommand.InitializeAsync(this);
            await FilesOnDiskCommand.InitializeAsync(this);
            await UnmodifiedCommand.InitializeAsync(this);
            await ClipboardCommand.InitializeAsync(this);
            await DocumentClipboardCommand.InitializeAsync(this);
            await SelectionClipboardCommand.InitializeAsync(this);
            await DocumentFileCommand.InitializeAsync(this);    
        }
    }
}
