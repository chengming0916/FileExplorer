using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels
{


   

    /// <summary>
    /// If Condition for the file list is true, then do the first command, otherwise do the second command.
    /// required FileList (IFileListViewModel)
    /// </summary>
    public class IfFileList : IfScriptCommand
    {
        /// <summary>
        /// If Condition for the file list is true, then do the first command, otherwise do the second command,
        /// required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        public IfFileList(Func<IFileListViewModel, bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
            : base(pd =>
                {
                    if (!pd.ContainsKey("FileList"))
                        return false;
                    IFileListViewModel flvm = pd["FileList"] as IFileListViewModel;
                    return condition(flvm);
                }, ifTrueCommand, otherwiseCommand)
        {

        }

    }

    /// <summary>
    /// Set Selected item to parameter, so it can be used in Toolbar based command.
    /// required FileList (IFileListViewModel)
    /// </summary>
    public class AssignSelectionToParameterAsEntryModelArray : RunInSequenceScriptCommand
    {
        /// <summary>
        /// FileList.Selection.SelectedItems as IEntryModel[] -> Parameter, required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="thenCommand"></param>
        public AssignSelectionToParameterAsEntryModelArray(IScriptCommand thenCommand)
            : base(
            new SimpleScriptCommand("AssignSelectionToParameterAsEntryModelArray",
                pd =>
                {
                    pd["Parameter"] = (pd["FileList"] as IFileListViewModel).Selection
                        .SelectedItems.Select(evm => evm.EntryModel).ToArray();
                    return ResultCommand.NoError;
                }),
            thenCommand)
        {

        }
    }

    /// <summary>
    /// If Condition for the selected items is true, then do the first command, otherwise do the second command,
    /// required FileList (IFileListViewModel)
    /// </summary>
    public class IfFileListSelection : IfFileList
    {
        /// <summary>
        /// If Condition for the selected items is true, then do the first command, otherwise do the second command,
        /// required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        public IfFileListSelection(Func<IList<IEntryViewModel>, bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
            : base(flvm => condition(flvm.Selection.SelectedItems), ifTrueCommand, otherwiseCommand)
        {

        }
    }


    /// <summary>
    /// Broadcast change directory to current selected directory
    /// required FileList (IFileListViewModel)
    /// </summary>
    public class OpenSelectedDirectory : ScriptCommandBase
    {
        /// <summary>
        /// Broadcast change directory to current selected directory, required FileList (IFileListViewModel)
        /// </summary>
        public OpenSelectedDirectory()
            : base("OpenSelectedDirectory")
        {

        }

        public override bool CanExecute(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList"))
                return false;

            var selectedItems = (pm["FileList"] as IFileListViewModel).Selection.SelectedItems;
            return selectedItems.Count == 1 && selectedItems[0].EntryModel.IsDirectory;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList") || !(pm["FileList"] is IFileListViewModel))
                return ResultCommand.Error(new KeyNotFoundException("FileList"));

            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;
            IEventAggregator events = pm["Events"] as IEventAggregator;

            var newDirectory = flvm.Selection.SelectedItems[0].EntryModel;
            events.Publish(new DirectoryChangedEvent(flvm,
                   newDirectory, flvm.CurrentDirectory));

            return ResultCommand.OK;
        }
    }
}
