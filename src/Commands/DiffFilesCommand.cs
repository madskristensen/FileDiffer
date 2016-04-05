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
                var command = _dte.Commands.Item("Tools.DiffFiles");

                if (command.IsAvailable)
                    _dte.ExecuteCommand(command.LocalizedName, $"\"{file1}\" \"{file2}\"");
            }
        }

        private bool CanFilesBeCompared(out string file1, out string file2)
        {
            var items = GetSelectedFiles();

            file1 = items.ElementAtOrDefault(0);
            file2 = items.ElementAtOrDefault(1);

            if (items.Count() == 1)
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
