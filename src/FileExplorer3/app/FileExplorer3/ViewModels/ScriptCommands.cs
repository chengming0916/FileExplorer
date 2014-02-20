using System;
using System.Collections.Generic;
using System.IO;
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
using System.Net.Http;
using System.Net;
using System.Threading;

namespace FileExplorer.ViewModels
{

    public static partial class ScriptCommands
    {
        /// <summary>
        /// Return pd["Parameter"] as IEntryModel[]
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static IEntryModel[] GetEntryModelFromParameter(ParameterDic pd)
        {
            return pd["Parameter"] as IEntryModel[];
        }

        public static IEntryModel GetFirstEntryModelFromParameter(ParameterDic pd)
        {
            return (pd["Parameter"] as IEntryModel[]).FirstOrDefault();
        }
    }

    #region MessageBox

    public static partial class ScriptCommands
    {
        public static IfOkCancel IfOkCancel(IWindowManager wm, Func<ParameterDic, string> captionFunc,
            Func<ParameterDic, string> messageFunc, IScriptCommand okCommand,
            IScriptCommand cancelCommand)
        { return new IfOkCancel(wm, captionFunc, messageFunc, okCommand, cancelCommand); }

        public static ShowFilePicker SaveFile(IWindowManager wm, IEventAggregator events,
            IEntryModel[] rootDirModels, string filter, string defaultFileName,
            Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            if (wm == null) wm = new WindowManager();
            return new ShowFilePicker(wm, events, rootDirModels, FilePickerMode.Save, filter, defaultFileName, 
                successCommand, cancelCommand);
        }

        public static ShowFilePicker SaveFile(IEntryModel[] rootDirModels, string filter, string defaultFileName,
           Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return SaveFile(null, null, rootDirModels, filter, defaultFileName,
                successCommand, cancelCommand);
        }

        public static ShowFilePicker OpenFile(IWindowManager wm, IEventAggregator events,
            IEntryModel[] rootDirModels, string filter, string defaultFileName,
            Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return new ShowFilePicker(wm, events, rootDirModels, FilePickerMode.Open, filter, 
                defaultFileName, successCommand, cancelCommand);
        }

        public static ShowFilePicker OpenFile(IEntryModel[] rootDirModels, string filter, string defaultFileName,
         Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return OpenFile(new WindowManager(), null, rootDirModels, filter, defaultFileName, successCommand, cancelCommand);
        }

        public static ShowMessageBox MessageBox(IWindowManager wm, string caption, string message)
        {
            return new ShowMessageBox(wm, caption, message);
        }


        public static DownloadFile Download(string sourceUrl, IEntryModel entry,
           HttpClient httpClient, IScriptCommand nextCommand = null)
        {
            return new DownloadFile(sourceUrl, entry, httpClient, nextCommand);
        }

    }


    /// <summary>
    /// If user clicked Ok, do first command, otherwise do second command.
    /// </summary>
    public class IfOkCancel : IfScriptCommand
    {
        private IScriptCommand _okCommand;
        private IScriptCommand _cancelCommand;
        internal IfOkCancel(IWindowManager wm, Func<ParameterDic, string> captionFunc,
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

    public class ShowFilePicker : ScriptCommandBase
    {
        private IWindowManager _wm;
        private string _filter;
        private string _defaultFileName;
        private Func<IEntryModelInfo, IScriptCommand> _successCommandFunc;
        private IEventAggregator _events;
        private IEntryModel[] _rootDirModels;
        private IScriptCommand _cancelFunc;
        private FilePickerMode _mode;
        internal ShowFilePicker(IWindowManager wm, IEventAggregator events,
            IEntryModel[] rootDirModels, FilePickerMode mode, string filter, string defaultFileName,
            Func<IEntryModelInfo, IScriptCommand> successCommandFunc, IScriptCommand cancelCommand)
            : base(mode.ToString() + "File")
        {
            _wm = wm;
            _events = events;
            _filter = filter;
            _mode = mode;
            _defaultFileName = defaultFileName;
            _successCommandFunc = successCommandFunc;
            _cancelFunc = cancelCommand;
            _rootDirModels = rootDirModels;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var filePicker = new FilePickerViewModel(_events, _wm, _filter, _mode, _rootDirModels);
            if (!String.IsNullOrEmpty(_defaultFileName))
                filePicker.FileName = _defaultFileName;
            if (_wm.ShowDialog(filePicker).Value)
            {
                string mode = _mode == FilePickerMode.Open ? "Open" : "Save";

                pm[mode + "FileName"] = filePicker.FileName; //OpenFileName or SaveFileName
                pm[mode + "Profile"] = filePicker.Profile;  //OpenProfile or SaveProfile

                return _successCommandFunc(filePicker);
            }
            else return _cancelFunc;
        }
    }

    public class ShowMessageBox : ScriptCommandBase
    {
        private IWindowManager _wm;
        private string _caption;
        private string _message;
        internal ShowMessageBox(IWindowManager wm, string caption, string message)
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

            _wm.ShowWindow(pdv);
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
            pdv.TryClose();
            return _nextCommand;
        }
    }

    public class DownloadFile : ScriptCommandBase
    {
        private string _sourceUrl;
        private bool _disposeHttpClient;
        private Func<HttpClient> _httpClientFunc;
        private Func<CancellationToken, Task<Stream>> _destStreamFunc;
        private IEntryModel _entry;
        public DownloadFile(string sourceUrl, Func<CancellationToken, Task<Stream>> destStreamFunc,
            Func<HttpClient> httpClientFunc, IScriptCommand nextCommand = null)
            : base("Download", nextCommand)
        {
            _sourceUrl = sourceUrl;
            _destStreamFunc = destStreamFunc;
            _httpClientFunc = httpClientFunc;
            _disposeHttpClient = true;
        }

        public DownloadFile(string sourceUrl, Func<CancellationToken, Task<Stream>> destStreamFunc,
            HttpClient httpClient, IScriptCommand nextCommand = null)
            : base("Download", nextCommand)
        {
            _sourceUrl = sourceUrl;
            _destStreamFunc = destStreamFunc;
            _httpClientFunc = () => httpClient;
            _disposeHttpClient = false;
        }

        public DownloadFile(string sourceUrl, IEntryModel entry,
           HttpClient httpClient, IScriptCommand nextCommand = null)
            : this(sourceUrl, (ct) => 
                (entry.Profile as IDiskProfile).DiskIO.OpenStreamAsync(entry, 
                FileAccess.Write, ct), httpClient, nextCommand )
        {
        }


        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var httpClient = _httpClientFunc();
            var pdv = pm.ContainsKey("Progress") && pm["Progress"] is IProgress<TransferProgress>
                ? pm["Progress"] as IProgress<TransferProgress> :
                NullProgresViewModel.Instance;

            if (httpClient != null)
                try
                {
                    var response = await httpClient.GetAsync(_sourceUrl, HttpCompletionOption.ResponseHeadersRead, pm.CancellationToken);
                    if (!response.IsSuccessStatusCode)
                        throw new WebException(String.Format("{0} when downloading {1}", response.StatusCode, _sourceUrl));

                    using (Stream srcStream = await response.Content.ReadAsStreamAsync())
                    using (Stream destStream = await _destStreamFunc(pm.CancellationToken))
                    {
                        byte[] buffer = new byte[1024];
                        ulong totalBytesRead = 0;
                        int byteRead = await srcStream.ReadAsync(buffer, 0, buffer.Length, pm.CancellationToken);
                        while (byteRead > 0)
                        {
                            await destStream.WriteAsync(buffer, 0, byteRead, pm.CancellationToken);
                            totalBytesRead = totalBytesRead + (uint)byteRead;
                            pdv.Report(TransferProgress.UpdateCurrentProgress(1));

                            byteRead = await srcStream.ReadAsync(buffer, 0, buffer.Length, pm.CancellationToken);

                        }
                        await destStream.FlushAsync();
                    }
                }
                finally
                {
                    if (_disposeHttpClient && httpClient != null)
                        httpClient.Dispose();

                }

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
            return new AssignSelectionToVariable(
                ExtensionMethods.GetCurrentDirectoryFunc, "Parameter", thenCommand);
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

        public static IScriptCommand AssignSelectionToParameter(IScriptCommand thenCommand, string variableName = "Parameter")
        {
            return new AssignSelectionToVariable(ExtensionMethods.GetFileListSelectionFunc,
                variableName, thenCommand);
        }

        public static IScriptCommand AssignCurrentDirectoryToDestination(IScriptCommand thenCommand, string variableName = "Destination")
        {
            return new AssignSelectionToVariable(ExtensionMethods.GetFileListCurrentDirectoryFunc,
                variableName, thenCommand);
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

        /// <summary>
        /// Select or Deselect items in FileList based on querySelectionFunc.
        /// </summary>
        /// <param name="querySelectionProv"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand Select(Func<IEntryModel, bool> querySelectionFunc, IScriptCommand nextCommand)
        {
            return new SelectFileList(querySelectionFunc, nextCommand);
        }

        /// <summary>
        /// Wait until filelist finished loading.
        /// </summary>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand WaitLoad(IScriptCommand nextCommand)
        {
            return new WaitFileList(nextCommand);
        }

        /// <summary>
        /// Refresh the filelist.
        /// </summary>
        /// <param name="nextCommand"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static IScriptCommand Refresh(IScriptCommand nextCommand, bool force = false)
        {
            return new RefreshFileList(nextCommand, force);
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
    internal class AssignSelectionToVariable : RunInSequenceScriptCommand
    {
        /// <summary>
        /// FileList.Selection.SelectedItems as IEntryModel[] -> Parameter, required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="thenCommand"></param>
        public AssignSelectionToVariable(Func<ParameterDic, IEntryModel[]> getSelectionFunc,
            string variableName, IScriptCommand thenCommand)
            : base(
            new SimpleScriptCommand("AssignSelectionToVariableAsEntryModelArray",
                pd =>
                {
                    pd[variableName] = getSelectionFunc(pd);
                    return ResultCommand.NoError;
                }),
            thenCommand)
        {

        }

        public AssignSelectionToVariable(Func<ParameterDic, IEntryModel> getSelectionFunc,
           string variableName, IScriptCommand thenCommand)
            : base(
            new SimpleScriptCommand("AssignSelectionToVariableAsEntryModel",
                pd =>
                {
                    pd[variableName] = getSelectionFunc(pd);
                    return ResultCommand.NoError;
                }),
            thenCommand)
        {

        }
    }


    /// <summary>
    /// Refresh the file list.
    /// </summary>
    internal class RefreshFileList : ScriptCommandBase
    {
        private bool _force;
        /// <summary>
        ///  Refresh the filelist.
        /// required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        internal RefreshFileList(IScriptCommand nextCommand, bool force)
            : base("Refresh", "FileList")
        {
            _nextCommand = nextCommand;
            _force = force;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList"))
                return ResultCommand.Error(new ArgumentException("Paremeter FileList is not found"));
            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;
            await flvm.ProcessedEntries.EntriesHelper.LoadAsync(_force);
            return _nextCommand;
        }


    }

    /// <summary>
    /// Run next command when filelist finished loading
    /// </summary>
    internal class WaitFileList : ScriptCommandBase
    {
        /// <summary>
        ///  Run next command when filelist finished loading,
        /// required FileList (IFileListViewModel)
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        internal WaitFileList(IScriptCommand nextCommand)
           : base("Waiting", "FileList")
        {
            _nextCommand = nextCommand;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList"))
                return ResultCommand.Error(new ArgumentException("Paremeter FileList is not found"));
            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;
            using (var releaser = await flvm.ProcessedEntries.EntriesHelper.LoadingLock.LockAsync())
                return _nextCommand;
        }


    }

    /// <summary>
    /// Select/Deselect item based on queryselection.
    /// </summary>
    internal class SelectFileList : ScriptCommandBase
    {
        private Func<IEntryModel, bool> _querySelectionFunc;
        internal SelectFileList(Func<IEntryModel, bool> querySelectionFunc, IScriptCommand nextCommand)
           : base("Select", "FileList")
        {
            _querySelectionFunc = querySelectionFunc;
            _nextCommand = nextCommand;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList"))
                return ResultCommand.Error(new ArgumentException("Paremeter FileList is not found"));
            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;
            flvm.Selection.Select(evm => evm != null && _querySelectionFunc(evm.EntryModel));

            return _nextCommand;
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

    #region Clipboard

    public static class ClipboardCommands
    {
        public static CopyToClipboardCommand Copy =
         new CopyToClipboardCommand(ScriptCommands.GetEntryModelFromParameter, false);
        public static CopyToClipboardCommand Cut =
            new CopyToClipboardCommand(ScriptCommands.GetEntryModelFromParameter, true);

        public static PasteFromClipboardCommand Paste(Func<ParameterDic, IEntryModel> currentDirectoryModelFunc,
            Func<DragDropEffects, IEntryModel[], IEntryModel, IScriptCommand> transferCommandFunc)
        {
            return new PasteFromClipboardCommand(currentDirectoryModelFunc, transferCommandFunc);
        }
    }

    public class CopyToClipboardCommand : ScriptCommandBase
    {

        private static byte[] preferCopy = new byte[] { 5, 0, 0, 0 };
        private static byte[] preferCut = new byte[] { 5, 0, 0, 0 };

        private Func<ParameterDic, IEntryModel[]> _srcModelFunc;
        private bool _removeOrginal;

        public CopyToClipboardCommand(Func<ParameterDic, IEntryModel[]> srcModelFunc, bool removeOrginal)
            : base(removeOrginal ? "Cut" : "Copy")
        {
            _removeOrginal = removeOrginal;
            _srcModelFunc = srcModelFunc;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var _srcModels = _srcModelFunc(pm);
            var da = await _srcModels.First().Profile.DragDrop.GetDataObject(_srcModels);

            byte[] moveEffect = _removeOrginal ? preferCut : preferCopy;
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);
            da.SetData("Preferred DropEffect", dropEffect);

            Clipboard.Clear();
            Clipboard.SetDataObject(da, true);

            return ResultCommand.NoError;
        }
    }

    public class PasteFromClipboardCommand : ScriptCommandBase
    {

        private Func<ParameterDic, IEntryModel> _currentDirectoryModelFunc;
        private Func<DragDropEffects, IEntryModel[], IEntryModel, IScriptCommand> _transferCommandFunc;

        public PasteFromClipboardCommand(Func<ParameterDic, IEntryModel> currentDirectoryModelFunc,
            Func<DragDropEffects, IEntryModel[], IEntryModel, IScriptCommand> transferCommandFunc)
            : base("Paste")
        {
            _currentDirectoryModelFunc = currentDirectoryModelFunc;
            _transferCommandFunc = transferCommandFunc;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var currentDirectory = _currentDirectoryModelFunc(pm);
            if (currentDirectory != null)
            {
                IDataObject da = Clipboard.GetDataObject();
                if (da != null)
                {
                    var srcModels = currentDirectory.Profile.DragDrop.GetEntryModels(da);
                    if (srcModels != null && srcModels.Count() > 0)
                    {
                        return _transferCommandFunc(DragDropEffects.Copy, srcModels.ToArray(), currentDirectory);
                    }
                }
            }

            return ResultCommand.NoError;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            var currentDirectory = _currentDirectoryModelFunc(pm);
            if (currentDirectory != null)
            {
                IDataObject da = Clipboard.GetDataObject();
                var srcModels = currentDirectory.Profile.DragDrop.GetEntryModels(da);
                return srcModels != null && srcModels.Count() > 0;
            }
            return false;
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
            var cannotTransfer = transferCommands.FirstOrDefault(c => c == null || !c.CanExecute(pm));
            return cannotTransfer == null;
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
