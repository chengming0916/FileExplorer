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
using Cofe.Core.Utils;

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

        public static IScriptCommand DoSelection(string commandKey,
            Func<ParameterDic, IEntryViewModel[]> getSelectionFunc,
            Func<IEntryViewModel[], IScriptCommand> nextCommandFunc,
            IScriptCommand noSelectionCommand)
        {
            return new DoSelection(commandKey, getSelectionFunc, nextCommandFunc, noSelectionCommand);
        }

        public static IScriptCommand AssignVariableToAnotherVariable(string sourceName, string targetName,
            IScriptCommand thenCommand)
        {
            return new SimpleScriptCommand("AssignVariable", pm =>
            {
                pm[targetName] = pm[sourceName];
                return thenCommand;
            }, pm => pm.ContainsKey(sourceName));
        }

        public static IScriptCommand AssignVariableToParameter(string sourceName,
           IScriptCommand thenCommand)
        {
            return AssignVariableToAnotherVariable(sourceName, "Parameter", thenCommand);
        }

    }

    #region MessageBox

    public static partial class ScriptCommands
    {
        public static IfOkCancel IfOkCancel(IWindowManager wm, Func<ParameterDic, string> captionFunc,
            Func<ParameterDic, string> messageFunc, IScriptCommand okCommand,
            IScriptCommand cancelCommand)
        { return new IfOkCancel(wm, captionFunc, messageFunc, okCommand, cancelCommand); }

        public static ShowFilePicker SaveFile(IExplorerInitializer initializer, string filter, string defaultFileName,
          Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return new ShowFilePicker(initializer, FilePickerMode.Save, filter, defaultFileName,
                successCommand, cancelCommand);
        }

        [Obsolete]
        public static ShowFilePicker SaveFile(IWindowManager wm, IEventAggregator events,
            IEntryModel[] rootDirModels, string filter, string defaultFileName,
            Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            if (wm == null) wm = new WindowManager();
            return new ShowFilePicker(wm, events, rootDirModels, FilePickerMode.Save, filter, defaultFileName,
                successCommand, cancelCommand);
        }

        [Obsolete]
        public static ShowFilePicker SaveFile(IEntryModel[] rootDirModels, string filter, string defaultFileName,
           Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return SaveFile(null, null, rootDirModels, filter, defaultFileName,
                successCommand, cancelCommand);
        }

        public static ShowFilePicker OpenFile(IExplorerInitializer initializer, string filter, string defaultFileName,
         Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return new ShowFilePicker(initializer, FilePickerMode.Open, filter, defaultFileName,
                successCommand, cancelCommand);
        }

        [Obsolete]
        public static ShowFilePicker OpenFile(IWindowManager wm, IEventAggregator events,
            IEntryModel[] rootDirModels, string filter, string defaultFileName,
            Func<IEntryModelInfo, IScriptCommand> successCommand, IScriptCommand cancelCommand)
        {
            return new ShowFilePicker(wm, events, rootDirModels, FilePickerMode.Open, filter,
                defaultFileName, successCommand, cancelCommand);
        }

        [Obsolete]
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

        public static ShowProgress ShowProgress(IWindowManager wm, string header, IScriptCommand nextCommand)
        {
            return new ShowProgress(wm, header, nextCommand);
        }

        public static ReportProgress ReportProgress(TransferProgress progress, IScriptCommand nextCommand = null)
        {
            return new ReportProgress(progress, nextCommand);
        }

        /// <summary>
        /// If events is not assignd here, it must be assigned in ParameterDic (Events) when run.
        /// </summary>
        public static IScriptCommand PublishEvent(object evnt, IScriptCommand nextCommand = null, IEventAggregator events = null)
        {
            return new PublishEvent(events, evnt, nextCommand);
        }

        public static IScriptCommand BroadcastEvent(object evnt, IScriptCommand nextCommand = null, IEventAggregator events = null)
        {
            return new PublishEvent(events, new BroadcastEvent(evnt), nextCommand);
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

        private string _filter;
        private string _defaultFileName;
        private Func<IEntryModelInfo, IScriptCommand> _successCommandFunc;

        private IScriptCommand _cancelFunc;
        private FilePickerMode _mode;
        private IExplorerInitializer _initializer;

        internal ShowFilePicker(IExplorerInitializer initializer, FilePickerMode mode, string filter, string defaultFileName,
           Func<IEntryModelInfo, IScriptCommand> successCommandFunc, IScriptCommand cancelCommand)
            : base(mode.ToString() + "File")
        {
            _initializer = initializer;
            _filter = filter;
            _mode = mode;
            _defaultFileName = defaultFileName;
            _successCommandFunc = successCommandFunc;
            _cancelFunc = cancelCommand;
        }

        internal ShowFilePicker(IWindowManager wm, IEventAggregator events,
            IEntryModel[] rootDirModels, FilePickerMode mode, string filter, string defaultFileName,
            Func<IEntryModelInfo, IScriptCommand> successCommandFunc, IScriptCommand cancelCommand)
            : this(new ExplorerInitializer(wm, events, rootDirModels), mode,
            filter, defaultFileName, successCommandFunc, cancelCommand)
        {
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var filePicker = new FilePickerViewModel(_initializer, _filter, _mode);
            if (!String.IsNullOrEmpty(_defaultFileName))
                filePicker.FileName = _defaultFileName;
            if (_initializer.WindowManager.ShowDialog(filePicker).Value)
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
        private string _header;
        internal ShowProgress(IWindowManager wm, string header, IScriptCommand nextCommand)
            : base("ShowProgress", nextCommand)
        {
            _wm = wm;
            _header = header;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            pm["ProgressHeader"] = _header;
            var pdv = new ProgressDialogViewModel(pm);
            pm["Progress"] = pdv;
            pm["CancellationToken"] = pdv.CancellationToken;

            _wm.ShowWindow(pdv);
            return _nextCommand;
        }
    }

    public class ReportProgress : ScriptCommandBase
    {
        private TransferProgress _progress;
        public ReportProgress(TransferProgress progress, IScriptCommand nextCommand = null)
            : base("ReportProgress", nextCommand)
        {
            _progress = progress;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pdv = pm["Progress"] as ProgressDialogViewModel;
            pdv.Report(_progress);
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
        private string _destId = null;
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
                FileAccess.Write, ct), httpClient, nextCommand)
        {
            _destId = entry.FullPath;
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
                        pdv.Report(TransferProgress.From(_sourceUrl, _destId));
                        byte[] buffer = new byte[1024];
                        ulong totalBytesRead = 0;
                        ulong totalBytes = 0;
                        try { totalBytes = (ulong)srcStream.Length; }
                        catch (NotSupportedException) { }

                        int byteRead = await srcStream.ReadAsync(buffer, 0, buffer.Length, pm.CancellationToken);
                        while (byteRead > 0)
                        {
                            await destStream.WriteAsync(buffer, 0, byteRead, pm.CancellationToken);
                            totalBytesRead = totalBytesRead + (uint)byteRead;
                            short percentCompleted = (short)((float)totalBytesRead / (float)totalBytes * 100.0f);
                            pdv.Report(TransferProgress.UpdateCurrentProgress(percentCompleted));

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

    internal class PublishEvent : ScriptCommandBase
    {
        private IEventAggregator _events;
        private object _evnt;
        public PublishEvent(IEventAggregator events, object evnt, IScriptCommand nextCommand)
            : base("PublishEvent", nextCommand)
        {
            _events = events;
            _evnt = evnt;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            _events = _events ?? pm.AsVMParameterDic().Events;
            _events.Publish(_evnt);
            return _nextCommand;
        }
    }

    #endregion


    public static class TabbedExplorer
    {
        public static IScriptCommand CloseTab(ITabbedExplorerViewModel tevm)
        {
            return new CloseTab(tevm);
        }

        public static IScriptCommand NewTab = new OpenTab();

        public static IScriptCommand AssignActiveTabToParameter(IScriptCommand thenCommand)
        {
            return Do((tevm, pd) => { pd.Parameter = tevm.ActiveItem; return thenCommand; });
        }

        /// <summary>
        /// Open directory (specified as Parameter) in new tab.
        /// </summary>
        /// <param name="tevm"></param>
        /// <returns></returns>
        public static IScriptCommand OpenTab(ITabbedExplorerViewModel tevm)
        {
            return new OpenTab(m => m != null && m.IsDirectory, tevm);
        }

        public static IScriptCommand Do(Func<ITabbedExplorerViewModel, ParameterDic, IScriptCommand> commandFunc)
        {
            return new DoTabbedExplorer(commandFunc);
        }
    }

    internal class CloseTab : ScriptCommandBase
    {
        private ITabbedExplorerViewModel _tevm;
        public CloseTab(ITabbedExplorerViewModel tevm = null)
            : base("CloseTab", "Parameter", "TabbedExplorer")
        {
            _tevm = tevm;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var tevm = (_tevm ?? pm["TabbedExplorer"]) as ITabbedExplorerViewModel;
            if (tevm == null)
                return ResultCommand.Error(new ArgumentNullException("TabbedExplorer"));
            var expvm = pm.Parameter as IExplorerViewModel ??
                tevm.ActiveItem as IExplorerViewModel;
            if (expvm == null)
                return ResultCommand.Error(new ArgumentException("Parameter"));

            tevm.CloseTab(expvm); ;
            return ResultCommand.NoError;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            if (_tevm == null && !pm.ContainsKey("TabbedExplorer"))
                return false;

            if (!pm.ContainsKey("Parameter") || !(pm["Parameter"] is IExplorerViewModel))
                return false;

            return true;
        }
    }

    internal class OpenTab : ScriptCommandBase
    {
        private Func<IEntryModel, bool> _filter;
        private ITabbedExplorerViewModel _tevm;
        public OpenTab(Func<IEntryModel, bool> filter = null, ITabbedExplorerViewModel tevm = null)
            : base("OpenTab", "Parameter", "TabbedExplorer")
        {
            _filter = filter;
            _tevm = tevm;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            IEntryModel dirModel = null;
            if (pd.Parameter is IEntryModel[])
                dirModel = (pd.Parameter as IEntryModel[]).FirstOrDefault();
            else if (pd.Parameter is IEntryModel)
                dirModel = pd.Parameter as IEntryModel;

            var tevm = (_tevm ?? pm["TabbedExplorer"]) as ITabbedExplorerViewModel;
            if (tevm == null)
                return ResultCommand.Error(new ArgumentNullException("TabbedExplorer"));
            if (_filter == null || _filter(dirModel))
                tevm.OpenTab(dirModel);
            return ResultCommand.NoError;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            if (_tevm == null && !pm.ContainsKey("TabbedExplorer"))
                return false;

            if (_filter == null)
                return true;

            var pd = pm.AsUIParameterDic();
            IEntryModel dirModel = null;
            if (pd.Parameter is IEntryModel[])
                dirModel = (pd.Parameter as IEntryModel[]).FirstOrDefault();
            else if (pd.Parameter is IEntryModel)
                dirModel = pd.Parameter as IEntryModel;

            return _filter(dirModel);
        }
    }

    internal class DoTabbedExplorer : DoCommandBase<ITabbedExplorerViewModel>
    {
        internal DoTabbedExplorer(Func<ITabbedExplorerViewModel, ParameterDic, IScriptCommand> commandFunc)
            : base("TabbedExplorer", commandFunc)
        {
        }

        protected DoTabbedExplorer(Func<ITabbedExplorerViewModel, ParameterDic, Task<IScriptCommand>> commandFunc)
            : base("TabbedExplorer", commandFunc)
        {
        }

        internal DoTabbedExplorer(string commandKey, Func<ITabbedExplorerViewModel, ParameterDic, IScriptCommand> commandFunc)
            : base(commandKey, commandFunc)
        {
        }

        protected DoTabbedExplorer(string commandKey, Func<ITabbedExplorerViewModel, ParameterDic, Task<IScriptCommand>> commandFunc)
            : base(commandKey, commandFunc)
        {
        }
    }

    #region ExplorerBased

    public static class Explorer
    {
        //public static IScriptCommand TryCloseWindow =
        //    Explorer.Do(evm => { evm.TryClose(); return ResultCommand.NoError; });

        public static IScriptCommand Do(Func<IExplorerViewModel, IScriptCommand> commandFunc)
        {
            return new DoExplorer(commandFunc);
        }

        public static IScriptCommand DoSelection(Func<IEntryViewModel[], IScriptCommand> nextCommandFunc,
           IScriptCommand noSelectionCommand = null)
        {
            return new DoSelection("DoSelectionExplorer", ExtensionMethods.GetCurrentDirectoryVMFunc,
                nextCommandFunc, noSelectionCommand);
        }




        //public static IScriptCommand GoTo(IScriptCommand thenCommand = null)
        //{
        //    return new GotoDirectory(thenCommand);
        //}

        public static IScriptCommand GoTo(IEntryModel dir, IScriptCommand thenCommand = null)
        {
            return new GotoDirectory(dir, thenCommand);
        }


        public static IScriptCommand Zoom(ZoomMode mode, int multiplier = 1, IScriptCommand nextCommand = null)
        {
            nextCommand = nextCommand ?? ResultCommand.NoError;
            float offset = (float)(mode == ZoomMode.ZoomIn ? 0.1 : -0.1) * multiplier;
            return Do(evm => { evm.Parameters.UIScale += offset; return nextCommand; });
        }

        //public static IScriptCommand GoTo(string path, IScriptCommand thenCommand = null)
        //{
        //    return new GotoDirectory(path, thenCommand);
        //}

        //public static IScriptCommand NewWindow(IExplorerInitializer initializer,
        //    string selectedDirectoryPath, bool openIfNotFound = true)
        //{
        //    return ScriptCommands.ParsePath(initializer.RootModels.GetProfiles(), selectedDirectoryPath,
        //        dirM => Explorer.NewWindow(initializer, dirM),
        //        openIfNotFound ? Explorer.NewWindow(initializer) :
        //        ResultCommand.Error(new System.IO.FileNotFoundException(selectedDirectoryPath)));
        //}

        public static IScriptCommand NewWindow(IExplorerInitializer initializer)
        {
            return NewWindow(initializer, (IEntryModel)null);
        }

        public static IScriptCommand NewWindow(IExplorerInitializer initializer,
            IEntryModel startupDirectory)
        {
            return NewWindow(initializer, null, startupDirectory);
        }

        public static IScriptCommand NewWindow(IExplorerInitializer initializer, object context,
            IEntryModel startupDirectory)
        {
            var dic = startupDirectory == null ? null :
                new Dictionary<string, object>() { { "StartupDirectory", startupDirectory } };
            return new ShowNewExplorer(initializer, null, dic);
        }

        //public static IScriptCommand NewWindow(IExplorerInitializer initializer,
        //    Func<ParameterDic, IEntryModel[]> getSelectedDirectryFunc)
        //{
        //    return new OpenInNewWindowCommand(initializer, getSelectedDirectryFunc);
        //}

        public static IScriptCommand PickDirectory(IExplorerInitializer initializer,
          IProfile[] rootProfiles, Func<IEntryModel, IScriptCommand> nextCommandFunc, IScriptCommand cancelCommand = null)
        {
            return new ShowDirectoryPicker(initializer, rootProfiles, nextCommandFunc, cancelCommand);
        }

        public static IScriptCommand ChangeRoot(ChangeType changeType,
            IEntryModel[] appliedRootDirectories, IScriptCommand nextCommand = null)
        {
            return new ChangeRootCommand(changeType, appliedRootDirectories, nextCommand);
        }

        public static IScriptCommand BroadcastRootChanged(RootChangedEvent evnt, IEventAggregator events = null,
            IScriptCommand nextCommand = null)
        {
            return new BroadcastChangeRoot(evnt, events, nextCommand);
        }




    }

    internal abstract class DoCommandBase<VM> : ScriptCommandBase
    {
        private Func<VM, ParameterDic, Task<IScriptCommand>> _commandFunc;
        private string _viewModelName;
        protected DoCommandBase(string viewModelName, Func<VM, ParameterDic, Task<IScriptCommand>> commandFunc)
            : base(viewModelName, viewModelName)
        {
            _viewModelName = viewModelName;
            _commandFunc = commandFunc;
        }

        protected DoCommandBase(string viewModelName, Func<VM, ParameterDic, IScriptCommand> commandFunc)
            : this(viewModelName, (vm, pd) => Task.Run(() => commandFunc(vm, pd)))
        {

        }

        protected DoCommandBase(string viewModelName, Func<VM, Task<IScriptCommand>> commandFunc)
            : this(viewModelName, (vm, pd) => commandFunc(vm))
        {
        }

        protected DoCommandBase(string viewModelName, Func<VM, IScriptCommand> commandFunc)
            : this(viewModelName, (vm, pd) => commandFunc(vm))
        {

        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            VM evm = (VM)pm[_viewModelName];
            if (evm == null)
                return ResultCommand.Error(new ArgumentException(_viewModelName));
            return await _commandFunc(evm, pm);
        }

        public override bool CanExecute(ParameterDic pm)
        {
            VM evm = (VM)pm[_viewModelName];
            return evm != null;// && AsyncUtils.RunSync(() => _commandFunc(evm)).CanExecute(pm);
        }
    }

    internal class DoSelection : ScriptCommandBase
    {
        private Func<ParameterDic, IEntryViewModel[]> _getSelectionFunc;
        private Func<IEntryViewModel[], IScriptCommand> _nextCommandFunc;
        private IScriptCommand _noSelectionCommand;

        internal DoSelection(string commandKey,
            Func<ParameterDic, IEntryViewModel[]> getSelectionFunc,
            Func<IEntryViewModel[], IScriptCommand> nextCommandFunc,
            IScriptCommand noSelectionCommand)
            : base(commandKey)
        {
            _getSelectionFunc = getSelectionFunc;
            _nextCommandFunc = nextCommandFunc;
            _noSelectionCommand = noSelectionCommand;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var selection = _getSelectionFunc(pm);
            if (selection == null || selection.Length == 0)
                return _noSelectionCommand;
            else return _nextCommandFunc(selection);
        }

        public override bool CanExecute(ParameterDic pm)
        {
            IScriptCommand nextCommand = Execute(pm);
            return nextCommand != null && nextCommand.CanExecute(pm);
        }
    }

    internal class DoExplorer : DoCommandBase<IExplorerViewModel>
    {
        internal DoExplorer(Func<IExplorerViewModel, IScriptCommand> commandFunc)
            : base("Explorer", commandFunc)
        {
        }

        protected DoExplorer(Func<IExplorerViewModel, Task<IScriptCommand>> commandFunc)
            : base("Explorer", commandFunc)
        {
        }

        internal DoExplorer(string commandKey, Func<IExplorerViewModel, IScriptCommand> commandFunc)
            : base(commandKey, commandFunc)
        {
        }

        protected DoExplorer(string commandKey, Func<IExplorerViewModel, Task<IScriptCommand>> commandFunc)
            : base(commandKey, commandFunc)
        {
        }
    }



    internal class BroadcastChangeRoot : ScriptCommandBase
    {
        private RootChangedEvent _evnt;
        private IEventAggregator _events;

        internal BroadcastChangeRoot(RootChangedEvent evnt, IEventAggregator events = null,
            IScriptCommand nextCommand = null)
            : base("BroadcastChangeRoot", nextCommand, "Events")
        {
            _events = events;
            _evnt = evnt;
        }


        public override IScriptCommand Execute(ParameterDic pm)
        {
            _events = _events ?? pm["Events"] as IEventAggregator;
            if (_events != null)
                return ScriptCommands.BroadcastEvent(_evnt, _nextCommand, _events);

            return ResultCommand.Error(new ArgumentException("Events"));
        }

    }

    /// <summary>
    /// Used by Change root directory in current ExplorerViwModel 
    /// </summary>
    internal class ChangeRootCommand : ScriptCommandBase
    {
        private ChangeType? _changeType;
        private IEntryModel[] _appliedRootDirectories = null;
        #region Constructors

        public ChangeRootCommand(IScriptCommand nextCommand = null)
            : base("ChangeRoot", "Explorer", "ChangeType", "AppliedRootDirectories")
        {
            _nextCommand = nextCommand;
        }

        public ChangeRootCommand(ChangeType changeType, IEntryModel[] appliedRootDirectories, IScriptCommand nextCommand = null)
            : this(nextCommand)
        {
            _changeType = changeType;
            _appliedRootDirectories = appliedRootDirectories;
        }



        #endregion

        #region Methods

        public override IScriptCommand Execute(ParameterDic pm)
        {
            IExplorerViewModel evm = pm["Explorer"] as IExplorerViewModel;
            if (evm != null && _appliedRootDirectories != null && _appliedRootDirectories.Length > 0)
            {
                try
                {
                    _changeType = _changeType.HasValue ? _changeType : (ChangeType)pm["ChangeType"];
                    _appliedRootDirectories = _appliedRootDirectories ?? (IEntryModel[])pm["AppliedRootDirectories"];

                    List<IEntryModel> currentList = evm.RootModels.ToList();
                    switch (_changeType)
                    {
                        case ChangeType.Created:
                            currentList.AddRange(_appliedRootDirectories);
                            break;
                        case ChangeType.Deleted:
                            foreach (var root in _appliedRootDirectories)
                                currentList.Remove(root);
                            break;
                        case ChangeType.Changed:
                            currentList = new List<IEntryModel>(_appliedRootDirectories);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    evm.RootModels = currentList.ToArray();

                    return _nextCommand ?? ResultCommand.NoError;
                }
                catch (Exception ex)
                {
                    return ResultCommand.Error(ex);
                }
            }
            return ResultCommand.Error(new ArgumentException("Explorer"));
        }


        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }

    internal class GotoDirectory : ScriptCommandBase
    {
        private IEntryModel _dir = null;

        internal GotoDirectory(IScriptCommand thenCommand)
            : base("GoToDirectory", thenCommand, "Directory")
        {
        }

        public GotoDirectory(IEntryModel dir, IScriptCommand thenCommand)
            : this(thenCommand)
        {
            _dir = dir;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IExplorerViewModel evm = pm["Explorer"] as IExplorerViewModel;
            IEventAggregator events = pm["Events"] as IEventAggregator;
            if (evm != null)
            {
                _dir = _dir ?? (pm.ContainsKey("Directory") ? (IEntryModel)pm["Directory"] : null);
                if (_dir != null)
                    await evm.GoAsync(_dir);
                return _nextCommand ?? ResultCommand.NoError;
            }
            return ResultCommand.Error(new ArgumentException("Explorer"));
        }

        //public GotoDirectory(string path, IScriptCommand thenCommand)
        //    : base(evm =>
        //        Task.Run(async () => { await evm.GoAsync(path); return thenCommand ?? ResultCommand.NoError; }))
        //{

        //}
    }

    public class ShowNewExplorer : ScriptCommandBase
    {
        private IExplorerInitializer _initializer;
        private object _context;
        private IDictionary<string, object> _settings;

        internal ShowNewExplorer(IExplorerInitializer initializer,  
            object context = null, IDictionary<string, object> settings = null)
            : base("NewWindow")
        {
            _context = context;
            _settings = settings;
            _initializer = initializer;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var evm = new ExplorerViewModel(_initializer);
            pm["Explorer"] = evm;
            _initializer.WindowManager.ShowWindow(evm, _context, _settings);
            if (_settings != null && _settings.ContainsKey("StartupDirectory") &&
                _settings["StartupDirectory"] is IEntryModel)
                return Explorer.GoTo(_settings["StartupDirectory"] as IEntryModel);
            return ResultCommand.NoError;
        }
    }

    public class ShowDirectoryPicker : ScriptCommandBase
    {
        private IExplorerInitializer _initializer;
        private IProfile[] _rootProfiles;
        private Func<IEntryModel, IScriptCommand> _nextCommandFunc;
        private IScriptCommand _cancelCommand;

        internal ShowDirectoryPicker(IExplorerInitializer initializer,
            IProfile[] rootProfiles, Func<IEntryModel, IScriptCommand> nextCommandFunc, IScriptCommand cancelCommand)
            : base("PickDirectory")
        {
            _initializer = initializer;
            _rootProfiles = rootProfiles;
            _nextCommandFunc = nextCommandFunc;
            _cancelCommand = cancelCommand;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            _nextCommandFunc = _nextCommandFunc ?? (em => ResultCommand.NoError);
            if (_rootProfiles != null && _rootProfiles.Length > 0)
            {
                if (_rootProfiles.Length == 1)
                {
                    var dpvm = new DirectoryPickerViewModel(_initializer.Events, _initializer.WindowManager,
                        _rootProfiles.First().ParseAsync("").Result);
                    if (_initializer.WindowManager.ShowDialog(dpvm).Value)
                        return _nextCommandFunc(dpvm.SelectedDirectory);
                }
                else
                {
                    var advm = new AddDirectoryViewModel(_initializer, _rootProfiles);
                    if (_initializer.WindowManager.ShowDialog(advm).Value)
                        return _nextCommandFunc(advm.SelectedDirectory);
                }
            }
            return _cancelCommand;
        }
    }

    #endregion

    #region DirectoryTreeBased

    public static class DirectoryTree
    {
        public static IScriptCommand ToggleRename =
          new ToggleRenameCommand(ExtensionMethods.GetCurrentDirectoryVMFunc);

        public static IScriptCommand ExpandSelected =
            new ExpandSelectedDirectory(ExtensionMethods.GetCurrentDirectoryVMFunc);

        public static IScriptCommand AssignSelectionToParameter(IScriptCommand thenCommand)
        {
            return new AssignSelectionToVariable(
                ExtensionMethods.GetCurrentDirectoryFunc, "Parameter", thenCommand);
        }

        public static IScriptCommand Do(Func<IDirectoryTreeViewModel, IScriptCommand> commandFunc)
        {
            return new DoDirectoryTree(commandFunc);
        }

    }

    internal class DoDirectoryTree : DoCommandBase<IDirectoryTreeViewModel>
    {
        internal DoDirectoryTree(Func<IDirectoryTreeViewModel, IScriptCommand> commandFunc)
            : base("DirectoryTree", commandFunc)
        {
        }
    }

    internal class ExpandSelectedDirectory : ScriptCommandBase
    {
        private Func<ParameterDic, IEntryViewModel[]> _getSelectionFunc;

        /// <summary>
        /// Broadcast change directory to current selected directory, required FileList (IDirectoryTreeViewModel)
        /// </summary>
        public ExpandSelectedDirectory(Func<ParameterDic, IEntryViewModel[]> getSelectionVMFunc)
            : base("ExpandSelectedDirectory")
        {
            _getSelectionFunc = getSelectionVMFunc;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            var selectedItems = _getSelectionFunc(pm);
            return selectedItems.Length == 1 && selectedItems[0].EntryModel.IsDirectory;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var selectedItem = _getSelectionFunc(pm).FirstOrDefault();

            //IDirectoryTreeViewModel dlvm = pm["DirectoryTree"] as IDirectoryTreeViewModel;
            //IEventAggregator events = pm["Events"] as IEventAggregator;
            var entryHelper = (selectedItem as IDirectoryNodeViewModel).Entries;
            entryHelper.IsExpanded = !entryHelper.IsExpanded;

            return ResultCommand.OK;
        }
    }

    #endregion

    #region FileList based.

    public static class FileList
    {
        public static IScriptCommand Do(Func<IFileListViewModel, IScriptCommand> commandFunc)
        {
            return new DoFileList(commandFunc);
        }


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

        public static IScriptCommand Zoom(ZoomMode mode, int multiplier = 1, IScriptCommand nextCommand = null)
        {
            nextCommand = nextCommand ?? ResultCommand.NoError;
            int offset = (mode == ZoomMode.ZoomIn ? 1 : -1) * multiplier;
            return Do(flvm =>
            {
                var viewModeModel = flvm.Commands.ToolbarCommands.CommandModels.AllNonBindable
                    .FirstOrDefault(cvm => cvm.CommandModel is ViewModeCommand).CommandModel as ViewModeCommand;

                if (viewModeModel != null)
                {
                    viewModeModel.SliderValue += offset;
                }
                return nextCommand;
            });
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
    /// Do certain action using the filelist.
    /// required FileList (IFileListViewModel)
    /// </summary>
    internal class DoFileList : DoCommandBase<IFileListViewModel>
    {
        internal DoFileList(Func<IFileListViewModel, IScriptCommand> commandFunc)
            : base("FileList", commandFunc)
        {
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

    internal class OpenInNewWindowCommand : ScriptCommandBase
    {
        private Func<ParameterDic, IEntryModel[]> _getSelectionFunc;
        private IExplorerInitializer _initializer;

        /// <summary>
        /// Broadcast change directory to current selected directory, required FileList (IFileListViewModel)
        /// </summary>
        public OpenInNewWindowCommand(IExplorerInitializer initializer, Func<ParameterDic, IEntryModel[]> getSelectionFunc)
            : base("OpenInNewWindowCommand")
        {
            _initializer = initializer;
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

            if (selectedItem != null)
                return Explorer.NewWindow(_initializer, selectedItem);

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
            return new NotifyChangedCommand(destModel.Profile, destModel.FullPath,
                srcModel.Profile, srcModel.FullPath, ChangeType.Moved);
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

    #region Sidebar based.

    public static class Sidebar
    {
        public static IScriptCommand Show(IScriptCommand nextCommand = null)
        { return new ToggleVisibility("Sidebar", true, nextCommand); }
        public static IScriptCommand Hide(IScriptCommand nextCommand = null)
        { return new ToggleVisibility("Sidebar", false, nextCommand); }
        public static IScriptCommand Toggle(IScriptCommand nextCommand = null)
        { return new ToggleVisibility("Sidebar", nextCommand); }
    }

    /// <summary>
    /// Do certain action using the previewer.
    /// required Sidebar (ISidebarViewModel)
    /// </summary>
    internal class DoSidebar : DoCommandBase<ISidebarViewModel>
    {
        internal DoSidebar(Func<ISidebarViewModel, IScriptCommand> commandFunc)
            : base("Sidebar", commandFunc)
        {
        }
    }

    internal class ToggleVisibility : DoCommandBase<IToggleableVisibility>
    {
        internal ToggleVisibility(string viewModelName, bool toValue, IScriptCommand nextCommand)
            : base(viewModelName, pvm => { pvm.IsVisible = toValue; return nextCommand; })
        {
        }

        internal ToggleVisibility(string viewModelName, IScriptCommand nextCommand)
            : base(viewModelName, pvm => { pvm.IsVisible = !pvm.IsVisible; return nextCommand; })
        {
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


    #region Transfer Based

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
                return ScriptCommands.ShowProgress(_windowManager, effects.ToString(),
                            ScriptCommands.ReportProgress(TransferProgress.IncrementTotalEntries(source.Length),
                                new RunInSequenceScriptCommand(transferCommands, new HideProgress())));
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
