using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Cinch;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels;

namespace FileExplorer
{
    public static partial class ExtensionMethods
    {
        public static Func<ParameterDic, IEntryModel[]> GetFileListItemsFunc =
            pd => pd.ContainsKey("FileList") && pd["FileList"] is IFileListViewModel ?
                (pd["FileList"] as IFileListViewModel).ProcessedEntries.EntriesHelper.AllNonBindable
                .Select(evm => evm.EntryModel).ToArray()
                : new IEntryModel[] { };

        public static Func<ParameterDic, IEntryViewModel[]> GetFileListItemsVMFunc =
            pd => pd.ContainsKey("FileList") && pd["FileList"] is IFileListViewModel ?
                (pd["FileList"] as IFileListViewModel).ProcessedEntries.EntriesHelper.AllNonBindable
                .ToArray()
                : new IEntryViewModel[] { };

        public static Func<ParameterDic, IEntryModel[]> GetFileListSelectionFunc =
            pd => pd.ContainsKey("FileList") && pd["FileList"] is IFileListViewModel ?
                (pd["FileList"] as IFileListViewModel).Selection.SelectedItems
                .Select(evm => evm.EntryModel).ToArray()
                : new IEntryModel[] { };

        public static Func<ParameterDic, IEntryViewModel[]> GetFileListSelectionVMFunc =
            pd => pd.ContainsKey("FileList") && pd["FileList"] is IFileListViewModel ?
                (pd["FileList"] as IFileListViewModel).Selection.SelectedItems
                .ToArray()
                : new IEntryViewModel[] { };

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


        public static void RegisterCommand(this IExportCommandBindings container, UIElement ele, ScriptBindingScope scope)
        {
            foreach (var c in container.ExportedCommandBindings)
                if (scope.HasFlag(c.Scope))
                {
                    var binding = c.CommandBinding;
                    if (binding != null)
                        ele.CommandBindings.Add(binding);
                }
        }

        public static IScriptCommand Rename(this IEntryModel entryModel, string newName)
        {
            return new RenameFileBasedEntryCommand(pm => entryModel, newName);
        }

    }
}
