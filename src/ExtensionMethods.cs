using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace FileDiffer
{
    internal static class ExtensionMethods
    {
        public static string GetText(this Document doc)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var textDocument = (TextDocument)doc.Object(null);
            var startPoint = textDocument.StartPoint.CreateEditPoint();
            return startPoint.GetText(textDocument.EndPoint);
        }
    }
}
