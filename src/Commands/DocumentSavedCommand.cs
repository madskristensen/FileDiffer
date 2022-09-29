using System;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    internal sealed class DocumentSavedCommand
    {
        private static DTE2 _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);

            var commandId = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.EditorBufferSaved);
            var command = new OleMenuCommand(CommandCallback, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;

            command.Enabled = _dte.ActiveDocument?.Saved == false;
        }

        private static void CommandCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var ext = Path.GetExtension(_dte.ActiveDocument.FullName);
            var left = CreateTempFileFromClipboard(ext, File.ReadAllText(_dte.ActiveDocument.FullName));
            var right = _dte.ActiveDocument.FullName;

            SelectedFilesCommand.Diff(left, right);
            File.Delete(left);
        }

        public static string CreateTempFileFromClipboard(string extension, string content)
        {
            var temp = Path.ChangeExtension(Path.GetTempFileName(), extension);
            File.WriteAllText(temp, content, Encoding.UTF8);
            return temp;
        }
    }
}
