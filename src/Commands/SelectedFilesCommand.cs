using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace FileDiffer
{
    internal sealed class SelectedFilesCommand
    {
        private static DTE2 _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);

            var commandId = new CommandID(PackageGuids.guidDiffFilesCmdSet, PackageIds.DiffFilesCommandId);
            var command = new OleMenuCommand(CommandCallback, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            var command = (OleMenuCommand)sender;
            IEnumerable<string> items = GetSelectedFiles();

            command.Visible = command.Enabled = items.Count() == 2;
        }

        private static void CommandCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (CanFilesBeCompared(out var file1, out var file2))
            {
                Diff(file1, file2);
            }
        }

        public static void Diff(string left, string right)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!DiffFileUsingCustomTool(left, right))
            {
                DiffFilesUsingDefaultTool(left, right);
            }
        }

        private static void DiffFilesUsingDefaultTool(string file1, string file2)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // This is the guid and id for the Tools.DiffFiles command
            var diffFilesCmd = "{5D4C0442-C0A2-4BE8-9B4D-AB1C28450942}";
            var diffFilesId = 256;
            object args = $"\"{file1}\" \"{file2}\"";

            _dte.Commands.Raise(diffFilesCmd, diffFilesId, ref args, ref args);
        }

        //Visual Studio allows replacing the default diff tool with a custom one.
        //See, for example:
        //Using WinMerge: https://blog.paulbouwer.com/2010/01/31/replace-diffmerge-tool-in-visual-studio-team-system-with-winmerge/
        //Using BeyondCompare: http://stackoverflow.com/questions/4466238/how-to-configure-visual-studio-to-use-beyond-compare
        private static bool DiffFileUsingCustomTool(string file1, string file2)
        {
            try
            {
                //Checking the registry to see if a custom tool is configured
                //Relevant information: https://social.msdn.microsoft.com/Forums/vstudio/en-US/37a26013-2f78-4519-85e5-d896ac27f31e/see-what-default-visual-studio-tfexe-compare-tool-is-set-to-using-visual-studio-api?forum=vsx
                var registryFolder = $"{_dte.RegistryRoot}\\TeamFoundation\\SourceControl\\DiffTools\\.*\\Compare";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryFolder))
                {
                    var command = key?.GetValue("Command") as string;
                    if (string.IsNullOrEmpty(command))
                    {
                        return false;
                    }

                    var args = key.GetValue("Arguments") as string;
                    if (string.IsNullOrEmpty(args))
                    {
                        return false;
                    }

                    //Understanding the arguments: https://msdn.microsoft.com/en-us/library/ms181446(v=vs.100).aspx
                    args =
                        args.Replace("%1", $"\"{file1}\"")
                            .Replace("%2", $"\"{file2}\"")
                            .Replace("%5", string.Empty)
                            .Replace("%6", $"\"{file1}\"")
                            .Replace("%7", $"\"{file2}\"");
                    System.Diagnostics.Process.Start(command, args);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return false;
            }
        }

        private static bool CanFilesBeCompared(out string file1, out string file2)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IEnumerable<string> items = GetSelectedFiles();

            file1 = items.ElementAtOrDefault(0);
            file2 = items.ElementAtOrDefault(1);

            return items.Count() == 2;
        }

        public static IEnumerable<string> GetSelectedFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;

            return from item in items.Cast<UIHierarchyItem>()
                   let pi = item.Object as ProjectItem
                   select pi.FileNames[1];
        }
    }
}
