using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
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
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;
            IEnumerable<string> items = SelectedFilesCommand.GetSelectedFiles();

            command.Enabled = command.Visible = items.Count() == 1;
        }
        
        private static void CommandCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (CanFilesBeCompared())
            {
                var right = SelectedFilesCommand.GetSelectedFiles().FirstOrDefault();

                if (!string.IsNullOrEmpty(right))
                {
                    Encoding encoding = GetEncoding(right);

                    var left = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(right));
                    File.WriteAllText(left, Clipboard.GetText(TextDataFormat.UnicodeText), encoding);

                    SelectedFilesCommand.Diff(left, right);
                    File.Delete(left);
                }
            }
        }

        private static Encoding GetEncoding(string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = (IComponentModel)ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));
            Assumes.Present(componentModel);

            ITextDocumentFactoryService docService = componentModel.GetService<ITextDocumentFactoryService>();
            IFileToContentTypeService contentTypeService = componentModel.GetService<IFileToContentTypeService>();
            IContentType contentType = contentTypeService.GetContentTypeForExtension(Path.GetExtension(fileName));

            using (ITextDocument doc = docService.CreateAndLoadTextDocument(fileName, contentType))
            {
                return doc.Encoding;
            }
        }

        private static bool CanFilesBeCompared()
        {
            return !string.IsNullOrWhiteSpace(Clipboard.GetText(TextDataFormat.UnicodeText));
        }
    }
}
