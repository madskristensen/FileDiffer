using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace FileDiffer
{
    internal sealed class DiffFilesCommand
    {
        private readonly Package _package;
        private readonly DTE2 _dte;

        private DiffFilesCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;
            _dte = (DTE2)ServiceProvider.GetService(typeof(DTE));

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

            var id = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.DiffFilesCommandId);
            var command = new OleMenuCommand(CommandCallback, id);
            command.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        public static DiffFilesCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new DiffFilesCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            var selectedFiles = GetSelectedFiles();

            // Only show if 1 or 2 files are selected
            button.Visible = selectedFiles.Count() <= 2;
        }

        private void CommandCallback(object sender, EventArgs e)
        {
            string file1, file2;

            if (CanFilesBeCompared(out file1, out file2))
            {
                // This is the guid and id for the Tools.DiffFiles command
                string diffFilesCmd = "{5D4C0442-C0A2-4BE8-9B4D-AB1C28450942}";
                int diffFilesId = 256;
                object args = $"\"{file1}\" \"{file2}\"";

                _dte.Commands.Raise(diffFilesCmd, diffFilesId, ref args, ref args);
            }
        }

        private bool CanFilesBeCompared(out string file1, out string file2)
        {

            var items = GetSelectedFiles();

            try
            {
                file1 = items.ElementAtOrDefault(0);
                file2 = items.ElementAtOrDefault(1);
            } catch
            {
                file1 = null;
                file2 = null;
            }

            if (string.IsNullOrEmpty(file1) && string.IsNullOrEmpty(file2))
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
                dialog.ShowDialog();
                file1 = dialog.FileName;

                var dialog2 = new OpenFileDialog();
                dialog2.InitialDirectory = Path.GetDirectoryName(file1);
                dialog2.ShowDialog();
                file2 = dialog2.FileName;
            } else if (items.Count() == 1)
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = Path.GetDirectoryName(file1);
                dialog.ShowDialog();

                file2 = dialog.FileName;
            }

            return !string.IsNullOrEmpty(file1) && !string.IsNullOrEmpty(file2);
        }

        public IEnumerable<string> GetSelectedFiles()
        {
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;

            return from item in items.Cast<UIHierarchyItem>()
                   let pi = item.Object as ProjectItem
                   select pi.FileNames[1];
        }
    }
}
