using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    internal sealed class FilesOnDiskCommand
    {
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            var commandId = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.FileOnDisk);
            var command = new OleMenuCommand(CommandCallback, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            IEnumerable<string> items = SelectedFilesCommand.GetSelectedFiles();

            command.Visible = command.Enabled = items.Count() == 1;
        }

        private static void CommandCallback(object sender, EventArgs e)
        {
            if (CanFilesBeCompared(out var file1, out var file2))
            {
                if (!SelectedFilesCommand.DiffFileUsingCustomTool(file1, file2))
                {
                    SelectedFilesCommand.DiffFilesUsingDefaultTool(file1, file2);
                }
            }
        }

        private static bool CanFilesBeCompared(out string file1, out string file2)
        {
            IEnumerable<string> items = SelectedFilesCommand.GetSelectedFiles();

            file1 = null;
            file2 = items.ElementAtOrDefault(0);

            var dialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(file1)
            };
            dialog.ShowDialog();

            file1 = dialog.FileName;

            return !string.IsNullOrEmpty(file1) && !string.IsNullOrEmpty(file2);
        }
    }
}
