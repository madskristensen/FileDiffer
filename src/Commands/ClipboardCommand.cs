using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    internal sealed class ClipboardCommand
    {
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            var commandId = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.Clipboard);
            var command = new OleMenuCommand(CommandCallback, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            IEnumerable<string> items = SelectedFilesCommand.GetSelectedFiles();

            command.Enabled = items.Count() == 1;
        }

        private static void CommandCallback(object sender, EventArgs e)
        {
            if (CanFilesBeCompared())
            {
                var right = SelectedFilesCommand.GetSelectedFiles().FirstOrDefault();

                if (!string.IsNullOrEmpty(right))
                {
                    Encoding encoding = Encoding.Default;

                    var left = Path.GetTempFileName();
                    File.WriteAllText(left, Clipboard.GetText(TextDataFormat.Text), encoding);

                    if (!SelectedFilesCommand.DiffFileUsingCustomTool(left, right))
                    {
                        SelectedFilesCommand.DiffFilesUsingDefaultTool(left, right);
                    }
                }
            }
        }

        private static bool CanFilesBeCompared()
        {
            return !string.IsNullOrWhiteSpace(Clipboard.GetText(TextDataFormat.Text));
        }
    }
}
