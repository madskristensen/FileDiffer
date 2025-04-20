using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    internal sealed class SelectionClipboardCommand
    {
        private static DTE2 _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);

            var commandId = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.EditorSelectionClipboard);
            var command = new OleMenuCommand(CommandCallback, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;
            var selection = _dte.ActiveDocument?.Selection as TextSelection;

            command.Enabled = selection?.IsEmpty == false;
        }

        private static void CommandCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (CanFilesBeCompared())
            {
                var ext = Path.GetExtension(_dte.ActiveDocument.FullName);
                var selection = (TextSelection)_dte.ActiveDocument.Selection;

                var left = CreateTempFileFromClipboard(ext, selection.Text);
                var right = CreateTempFileFromClipboard(ext, Clipboard.GetText(TextDataFormat.UnicodeText));

                SelectedFilesCommand.Diff(left, right);
                File.Delete(left);
                File.Delete(right);
            }
        }

        public static string CreateTempFileFromClipboard(string extension, string content)
        {
            var temp = Path.ChangeExtension(Path.GetTempFileName(), extension);
            File.WriteAllText(temp, content, Encoding.UTF8);
            return temp;
        }

        private static bool CanFilesBeCompared()
        {
            return !string.IsNullOrWhiteSpace(Clipboard.GetText(TextDataFormat.UnicodeText));
        }
    }
}
