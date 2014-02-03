using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels;

namespace FileExplorer.ViewModels
{

    #region MessageBox



    /// <summary>
    /// If user clicked Ok, do first command, otherwise do second command.
    /// </summary>
    public class IfOkCancel : IfScriptCommand
    {
        private IScriptCommand _okCommand;
        private IScriptCommand _cancelCommand;
        public IfOkCancel(IWindowManager wm, Func<ParameterDic, string> captionFunc,
            Func<ParameterDic, string> messageFunc, IScriptCommand okCommand,
            IScriptCommand cancelCommand)
            : base(
                   pd =>
                   {
                       var mdv = new MessageDialogViewModel(captionFunc(pd), messageFunc(pd),
                           MessageDialogViewModel.DialogButtons.Cancel | MessageDialogViewModel.DialogButtons.OK);
                       if (wm.ShowDialog(mdv).Value)
                           return mdv.SelectedButton == MessageDialogViewModel.DialogButtons.OK;
                       return false;
                   },
                    okCommand, cancelCommand)
        {
            _okCommand = okCommand;
            _cancelCommand = cancelCommand;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            return _okCommand.CanExecute(pm);
        }
    }

    public class ShowMessageBox : ScriptCommandBase
    {
        private IWindowManager _wm;
        private string _caption;
        private string _message;
        public ShowMessageBox(IWindowManager wm, string caption, string message)
            : base("ShowMessageBox")
        {
            _wm = wm;
            _caption = caption;
            _message = message;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var mdv = new MessageDialogViewModel(_caption, _message,
                                 MessageDialogViewModel.DialogButtons.OK);
            _wm.ShowDialog(mdv);
            return ResultCommand.NoError;
        }
    }

    public class ShowProgress : ScriptCommandBase
    {
        private IWindowManager _wm;
        public ShowProgress(IWindowManager wm, IScriptCommand nextCommand)
            : base("ShowProgress", nextCommand)
        {
            _wm = wm;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pdv = new ProgressDialogViewModel(pm);

            pm["Progress"] = pdv;
            pm["CancellationToken"] = pdv.CancellationToken;

            _wm.ShowDialog(pdv);
            return _nextCommand;
        }
    }


    public class HideProgress : ScriptCommandBase
    {        
        public HideProgress(IScriptCommand nextCommand = null)
            : base("HideProgress", nextCommand)
        {
            
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pdv = pm["Progress"] as ProgressDialogViewModel;
            pdv.TryClose(true);
            return _nextCommand;
        }
    }

    #endregion

    #region DirectoryTreeBased

    public static class DirectoryTree
    {
        public static IScriptCommand ToggleRename =
          new ToggleRenameCommand(ExtensionMethods.GetCurrentDirectoryVMFunc);

        public static IScriptCommand AssignSelectionToParameter(IScriptCommand thenCommand)
        {
            return new AssignSelectionToParameterAsEntryModelArray(ExtensionMethods.GetCurrentDirectoryFunc, thenCommand);
        }
    }

    #endregion

    #region FileList based.

    public static class FileList
    {
        public static IScriptCommand If(Func<IFileListViewModel, bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
        {
            return new IfFileList(condition, ifTrueCommand, otherwiseCommand);
        }

        public static IScriptCommand IfSelection(Func<IEntryViewModel[], bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
        {
            return new IfFileList(flvm => condition(flvm.Selection.SelectedItems.ToArray()), ifTrueCommand, otherwiseCommand);
        }

        public static IScriptCommand IfSelectionModel(Func<IEntryModel[], bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
        {
            return new IfFileList(flvm => condition(flvm.Selection.SelectedItems.Select(vm => vm.EntryModel).ToArray()),
                ifTrueCommand, otherwiseCommand);
        }

        public static IScriptCommand AssignSelectionToParameter(IScriptCommand thenCommand)
        {
            return new AssignSelectionToParameterAsEntryModelArray(ExtensionMethods.GetFileListSelectionFunc, thenCommand);
        }

        public static IScriptCommand OpenSelectedDirectory =
            new OpenSelectedDirectory(ExtensionMethods.GetFileListSelectionFunc);

        public static IScriptCommand ToggleRename =
            new ToggleRenameCommand(ExtensionMethods.GetFileListSelectionVMFunc);

        public static IScriptCommand Lookup(Func<IEntryModel, bool> lookupFunc,
            Func<IEntryModel, IScriptCommand> foundCommandFunc, IScriptCommand notFoundCommand)
        {
            return new LookupEntryCommand(lookupFunc, foundCommandFunc, notFoundCommand, ExtensionMethods.GetFileListItemsFunc);
        }

    }

    /// <summary>
    /// If Condition for the file list is true, then do the first command, otherwise do the second command.
    /// required FileList (IFileListViewModel)
    /// </summary>
    internal class IfFileList : IfScriptCommand
    {
        /// <summary>
        /// If Condition for the file list is true, then do the first command, otherwise do the second command,
        /// required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        internal IfFileList(Func<IFileListViewModel, bool> condition, IScriptCommand ifTrueCommand,
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
    internal class AssignSelectionToParameterAsEntryModelArray : RunInSequenceScriptCommand
    {
        /// <summary>
        /// FileList.Selection.SelectedItems as IEntryModel[] -> Parameter, required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="thenCommand"></param>
        public AssignSelectionToParameterAsEntryModelArray(Func<ParameterDic, IEntryModel[]> getSelectionFunc, IScriptCommand thenCommand)
            : base(
            new SimpleScriptCommand("AssignSelectionToParameterAsEntryModelArray",
                pd =>
                {
                    pd["Parameter"] = getSelectionFunc(pd);
                    return ResultCommand.NoError;
                }),
            thenCommand)
        {

        }
    }


    /// <summary>
    /// Broadcast change directory to current selected directory
    /// required FileList (IFileListViewModel)
    /// </summary>
    internal class OpenSelectedDirectory : ScriptCommandBase
    {
        public static OpenSelectedDirectory FromFileList = new OpenSelectedDirectory(ExtensionMethods.GetFileListSelectionFunc);
        private Func<ParameterDic, IEntryModel[]> _getSelectionFunc;

        /// <summary>
        /// Broadcast change directory to current selected directory, required FileList (IFileListViewModel)
        /// </summary>
        public OpenSelectedDirectory(Func<ParameterDic, IEntryModel[]> getSelectionFunc)
            : base("OpenSelectedDirectory")
        {
            _getSelectionFunc = getSelectionFunc;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            var selectedItems = _getSelectionFunc(pm);
            return selectedItems.Length == 1 && selectedItems[0].IsDirectory;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var selectedItem = _getSelectionFunc(pm).FirstOrDefault();

            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;
            IEventAggregator events = pm["Events"] as IEventAggregator;

            events.Publish(new DirectoryChangedEvent(flvm,
                   selectedItem, flvm.CurrentDirectory));

            return ResultCommand.OK;
        }
    }

    internal class ToggleRenameCommand : ScriptCommandBase
    {
        public static ToggleRenameCommand ForSelectedItem = new ToggleRenameCommand(ExtensionMethods.GetFileListSelectionVMFunc);
        private Func<ParameterDic, IEntryViewModel[]> _getSelectionFunc;

        /// <summary>
        /// Broadcast change directory to current selected directory, required FileList (IFileListViewModel)
        /// </summary>
        public ToggleRenameCommand(Func<ParameterDic, IEntryViewModel[]> getSelectionFunc)
            : base("ToggleRenameCommand")
        {
            _getSelectionFunc = getSelectionFunc;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            var selectedItems = _getSelectionFunc(pm);
            return selectedItems.Length == 1 && selectedItems[0].IsRenamable;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var selectedItem = _getSelectionFunc(pm).FirstOrDefault();

            if (selectedItem != null)
                selectedItem.IsRenaming = !selectedItem.IsRenaming;

            return ResultCommand.OK;
        }
    }


    internal class RenameFileBasedEntryCommand : ScriptCommandBase
    {
        //public static RenameFileBasedEntryCommand FromParameter = new RenameFileBasedEntryCommand(
        //    FileBasedScriptCommandsHelper.GetFirstEntryModelFromParameter);
        private Func<ParameterDic, IEntryModel> _srcModelFunc;
        private string _newName;

        public RenameFileBasedEntryCommand(Func<ParameterDic, IEntryModel> srcModelFunc, string newName = null)
            : base("Rename")
        {
            _srcModelFunc = srcModelFunc;
            _newName = newName;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string newName = _newName ?? pm["NewName"] as string;
            if (String.IsNullOrEmpty(newName))
                return ResultCommand.Error(new ArgumentException("NewName"));
            var srcModel = _srcModelFunc(pm);
            if (srcModel == null)
                return ResultCommand.Error(new ArgumentException());

            IDiskProfile profile = srcModel.Profile as IDiskProfile;
            if (profile == null)
                return ResultCommand.Error(new ArgumentException());

            IEntryModel destModel = await profile.DiskIO.RenameAsync(srcModel, newName, pm.CancellationToken);
            return new NotifyChangedCommand(destModel.Profile, destModel.FullPath, ChangeType.Moved, srcModel.FullPath);
        }
    }


    internal class LookupEntryCommand : ScriptCommandBase
    {
        private Func<ParameterDic, IEntryModel[]> _getItemsFunc;
        private Func<IEntryModel, bool> _lookupFunc;
        private Func<IEntryModel, IScriptCommand> _foundCommandFunc;
        private IScriptCommand _notFoundCommand;

        public LookupEntryCommand(
            Func<IEntryModel, bool> lookupFunc,
            Func<IEntryModel, IScriptCommand> foundCommandFunc, IScriptCommand notFoundCommand,
            Func<ParameterDic, IEntryModel[]> getItemsFunc = null
            )
            : base("LookupEntry")
        {
            _getItemsFunc = getItemsFunc ?? ExtensionMethods.GetFileListItemsFunc;
            _lookupFunc = lookupFunc;
            _foundCommandFunc = foundCommandFunc;
            _notFoundCommand = notFoundCommand;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var foundItem = _getItemsFunc(pm).FirstOrDefault(_lookupFunc);
            if (foundItem != null)
                return _foundCommandFunc(foundItem);
            else return _notFoundCommand;
        }
    }

    #endregion

    #region Explorer Based

    /// <summary>
    /// Transfer Source entries (IEntryModel[]) to Dest directory (IEntryModel)
    /// </summary>
    public class TransferCommand : ScriptCommandBase
    {        
        #region Constructor

        public TransferCommand(Func<DragDropEffects, IEntryModel, IEntryModel, IScriptCommand> transferOneFunc, IWindowManager windowManager = null)
            : base("Transfer", "Source", "Dest", "DragDropEffects")
        {
            _windowManager = windowManager;            
            _transferOneFunc = transferOneFunc;
        }

        #endregion

        #region Methods

        public override bool CanExecute(ParameterDic pm)
        {
            var source = pm["Source"] as IEntryModel[];
            var dest = pm["Dest"] as IEntryModel;
            DragDropEffects effects = pm.ContainsKey("DragDropEffects") && pm["DragDropEffects"] is DragDropEffects ?
                (DragDropEffects)pm["DragDropEffects"] : DragDropEffects.Copy;

            if (source == null || source.Length == 0 || dest == null)
                return false;

            var transferCommands = source.Select(s => _transferOneFunc(effects, s, dest)).ToArray();
            return transferCommands.Any(c => c == null || !c.CanExecute(pm));                
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var source = pm["Source"] as IEntryModel[];
            var dest = pm["Dest"] as IEntryModel;
            DragDropEffects effects = pm.ContainsKey("DragDropEffects") && pm["DragDropEffects"] is DragDropEffects ?
              (DragDropEffects)pm["DragDropEffects"] : DragDropEffects.Copy;

            var transferCommands = source.Select(s => _transferOneFunc(effects, s, dest)).ToArray();
            if (transferCommands.Any(c => c == null || !c.CanExecute(pm)))
                return ResultCommand.Error(new ArgumentException("Not all items are transferrable."));

            if (_windowManager != null)
                return new ShowProgress(_windowManager,
                    new RunInSequenceScriptCommand(transferCommands, new HideProgress()));
            else return new RunInSequenceScriptCommand(transferCommands);
        }

        #endregion

        #region Data

        private Func<DragDropEffects, IEntryModel, IEntryModel, IScriptCommand> _transferOneFunc;
        private IWindowManager _windowManager;        

        #endregion

        #region Public Properties

        #endregion
    }

    #endregion
}
