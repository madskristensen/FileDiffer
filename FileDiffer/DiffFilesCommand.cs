using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FileDiffer
{
    internal sealed class DiffFilesCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("5034b97c-760a-45e5-a15d-d86dcfae06f7");

        private readonly Package package;

        private DiffFilesCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static DiffFilesCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new DiffFilesCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            string file1, file2;

            if (CanFilesBeCompared(dte, out file1, out file2))
            {
                dte.ExecuteCommand("Tools.DiffFiles", $"\"{file1}\" \"{file2}\"");
            }
        }

        private static bool CanFilesBeCompared(DTE2 dte, out string file1, out string file2)
        {
            var items = GetSelectedFiles(dte);

            file1 = items.ElementAtOrDefault(0);
            file2 = items.ElementAtOrDefault(1);

            return !string.IsNullOrEmpty(file1) && !string.IsNullOrEmpty(file2);
        }

        public static IEnumerable<string> GetSelectedFiles(DTE2 dte)
        {
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

            return from item in items.Cast<UIHierarchyItem>()
                   let pi = item.Object as ProjectItem
                   select pi.FileNames[1];
        }
    }
}
