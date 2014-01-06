using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Cinch;
using FileExplorer.Models;
using FileExplorer.ViewModels;

namespace FileExplorer
{
    public static partial class ExtensionMethods
    {
        //public static IEnumerable<IDirectoryNodeViewModel> GetHierarchy(
        //    this IDirectoryNodeViewModel node, bool includeCurrent)
        //{
        //    if (includeCurrent)
        //        yield return node;

        //    IDirectoryNodeViewModel current = node.ParentNode;
        //    while (current != null)
        //    {
        //        yield return current;
        //        current = current.ParentNode;
        //    }
        //}

        public static IEnumerable<IEntryModel> GetHierarchy(
            this IEntryModel node, bool includeCurrent)
        {
            if (includeCurrent)
                yield return node;

            IEntryModel current = node.Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }


        public static void RegisterCommand(this IExportCommandBindings container, UIElement ele)
        {
            foreach (var c in container.ExportedCommandBindings)
            {
                var binding = c.CommandBinding;
                if (binding != null)
                    ele.CommandBindings.Add(binding);
            }
        }

    }
}
