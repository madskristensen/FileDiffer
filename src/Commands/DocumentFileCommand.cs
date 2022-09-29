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
    internal sealed class DocumentFileCommand
    {
        private static DTE2 _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);

            var commandId = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.EditorBufferFile);
            var command = new OleMenuCommand(CommandCallback, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;

            command.Enabled = !string.IsNullOrEmpty(_dte.ActiveDocument?.FullName);
        }

        private static void CommandCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(_dte.ActiveDocument.FullName)
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var ext = Path.GetExtension(_dte.ActiveDocument.FullName);
            var right = dialog.FileName;
            var left = CreateTempFileFromClipboard(ext, _dte.ActiveDocument.GetText());

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
