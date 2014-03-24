using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels;
using FileExplorer;
using FileExplorer.Utils;
using Cofe.Core.Script;
using System.Collections.ObjectModel;
using System.Windows;
using Cofe.Core.Utils;
using System.Configuration;
using System.IO;
using System.Windows.Input;
using FileExplorer.ViewModels.Helpers;
using FileExplorer.BaseControls;
using System.Threading;
using Cofe.Core;
using DropNet;
using DropNet.Models;

namespace TestApp
{
    [Export(typeof(IScreen))]
    public class AppViewModel : Screen//, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events, IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _events = events;

            _profile = new FileSystemInfoProfile(_events, windowManager);
            _profileEx = new FileSystemInfoExProfile(events, windowManager);

            RootModels.Add(_profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result);
        }

        #endregion

        #region Methods


        public static IExplorerInitializer getInitializer(IWindowManager windowManager,
            IEventAggregator events, IEntryModel[] rootModels, params IViewModelInitializer<IExplorerViewModel>[] initalizers)
        {
            var retVal = new ExplorerInitializer(windowManager, events, rootModels);
            retVal.Initializers.AddRange(initalizers);
            return retVal;
        }


        public void OpenWindow()
        {
            var sr = new ScriptRunner();
            sr.Run(Explorer.NewWindow(
                getInitializer(_windowManager, _events, RootModels.ToArray(),
                 new BasicParamInitalizers(_expandRootDirectories, _enableMultiSelect, _enableDrag, _enableDrop),
                 new ColumnInitializers(),
                 new ScriptCommandsInitializers(_windowManager, _events),
                 new ToolbarCommandsInitializers(_windowManager))), new ParameterDic());
        }


        public void PickFiles()
        {
            new ScriptRunner().Run(new ParameterDic(),
                ScriptCommands.OpenFile(_windowManager, _events, RootModels.ToArray(), FileFilter, "demo.txt",
                    (fpvm) => ScriptCommands.MessageBox(_windowManager, "Open", fpvm.FileName), ResultCommand.OK));
            //var filePicker = new FilePickerViewModel(_events, _windowManager, FileFilter, FilePickerMode.Open, RootModels.ToArray());
            //updateExplorerModel(initExplorerModel(filePicker));
            //if (_windowManager.ShowDialog(filePicker).Value)
            //{
            //    MessageBox.Show(String.Join(",", filePicker.SelectedFiles.Select(em => em.FullPath)));
            //}
        }

        public void SaveFile()
        {
            new ScriptRunner().Run(new ParameterDic(),
               ScriptCommands.SaveFile(_windowManager, null, RootModels.ToArray(), FileFilter, "demo.txt",
                   (fpvm) => ScriptCommands.MessageBox(_windowManager, "Save", fpvm.FileName), ResultCommand.OK));

            //var filePicker = new FilePickerViewModel(_events, _windowManager, FileFilter, FilePickerMode.Save, RootModels.ToArray());
            //updateExplorerModel(initExplorerModel(filePicker));
            //if (_windowManager.ShowDialog(filePicker).Value)
            //{
            //    MessageBox.Show(filePicker.FileName);
            //}
        }

        private IEntryModel showDirectoryPicker(IEntryModel[] rootModels)
        {
            var directoryPicker = new DirectoryPickerViewModel(
                AppViewModel.getInitializer(_windowManager, _events, rootModels,
                new BasicParamInitalizers(true, false, false, false),
                new ColumnInitializers()));

            if (_windowManager.ShowDialog(directoryPicker).Value)
                return directoryPicker.SelectedDirectory;
            return null;
        }

        public void Clear()
        {
            RootModels.Clear();
            _events.Publish(new RootChangedEvent(ChangeType.Changed, RootModels.ToArray()));
        }

        public void Remove()
        {
            if (SelectedRootModel != null)
            {
                _events.Publish(new RootChangedEvent(ChangeType.Deleted, SelectedRootModel));
                RootModels.Remove(SelectedRootModel);
            }
        }

        private void pickAndAdd(IEntryModel[] rootModel)
        {
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
            _events.Publish(new RootChangedEvent(ChangeType.Created, selectedModel));
        }

        public void AddDirectoryInfo()
        {
            var rootModel = new[] { _profile.ParseAsync("C:\\").Result };
            pickAndAdd(rootModel);
        }

        public void AddDirectoryInfoEx()
        {
            var rootModel = new[] { _profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result };
            pickAndAdd(rootModel);
        }


        public static string skyDriveAliasMask = "{0}'s OneDrive";
        public async Task AddSkyDrive()
        {

            Func<string> loginSkyDrive = () =>
                {
                    var login = new SkyDriveLogin(AuthorizationKeys.SkyDrive_Client_Id);
                    if (_windowManager.ShowDialog(new LoginViewModel(login)).Value)
                    {
                        return login.AuthCode;
                    }
                    return null;
                };

            if (_profileSkyDrive == null)
                _profileSkyDrive = new SkyDriveProfile(_events, _windowManager, AuthorizationKeys.SkyDrive_Client_Id, loginSkyDrive, skyDriveAliasMask);
            var rootModel = new[] { await _profileSkyDrive.ParseAsync("") };
            pickAndAdd(rootModel);
        }

        public async Task AddGoogleDrive()
        {

            if (_profileGoogleDrive == null)
                using (var gapi_secret_stream = System.IO.File.OpenRead("gapi_client_secret.json")) //For demo only.
                {
                    _profileGoogleDrive = new GoogleDriveProfile(_events, _windowManager, gapi_secret_stream);
                }
            var rootModel = new[] { await _profileGoogleDrive.ParseAsync("") };
            pickAndAdd(rootModel);
        }

        public async Task AddDropBox()
        {
            Func<UserLogin> loginDropBox = () =>
            {
                var login = new DropBoxLogin(AuthorizationKeys.DropBox_Client_Id,
                    AuthorizationKeys.DropBox_Client_Secret);
                if (_windowManager.ShowDialog(new LoginViewModel(login)).Value)
                {
                    return login.AccessToken;
                }
                return null;
            };

            if (_profileDropBox == null)
                _profileDropBox = new DropBoxProfile(_events, _windowManager,
                    AuthorizationKeys.DropBox_Client_Id,
                          AuthorizationKeys.DropBox_Client_Secret,
                          loginDropBox);

            var rootModel = new[] { await _profileDropBox.ParseAsync("") };
            pickAndAdd(rootModel);
        }

        public void ShowDialog()
        {
            _windowManager.ShowDialog(new MessageDialogViewModel("Caption", "Message 1 2 3 4 5 6 7 8 9 10",
                MessageDialogViewModel.DialogButtons.OK | MessageDialogViewModel.DialogButtons.Cancel));
        }

        public void ProgressDialog()
        {
            new ScriptRunner().Run(
                ScriptCommands.ShowProgress(_windowManager, "Testing",
                    ScriptCommands.ReportProgress(TransferProgress.From("C:\\Demo\\Abc.txt", "http://FileExplorer.org/abc.txt"),
                    ScriptCommands.ReportProgress(TransferProgress.IncrementTotalEntries(100),
                    ScriptCommands.ReportProgress(TransferProgress.IncrementProcessedEntries(20),
                    ScriptCommands.ReportProgress(TransferProgress.UpdateCurrentProgress(50)))))),
                new ParameterDic());
            //_windowManager.ShowDialog(new ProgressDialogViewModel(new ParameterDic() 
            //{

            //}));
        }

        public void MdiWindow()
        {
            new MdiWindow(RootModels.ToArray()).Show();
        }



        #endregion

        #region Data

        IProfile _profile;
        IProfile _profileEx;
        IProfile _profileSkyDrive;
        IProfile _profileGoogleDrive;

        //private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };
        //private string _addPath = lookupPath;
        private IEventAggregator _events;
        private IWindowManager _windowManager;
        private IExplorerViewModel _explorer = null;
        private bool _expandRootDirectories = false;
        private bool _enableDrag = true, _enableDrop = true, _enableMultiSelect = true;

        private ObservableCollection<IEntryModel> _rootModels = new ObservableCollection<IEntryModel>();
        private string _fileFilter = "Texts (.txt)|*.txt|Pictures (.jpg, .png)|*.jpg,*.png|Songs (.mp3)|*.mp3|All Files (*.*)|*.*";
        private DropBoxProfile _profileDropBox;
        private IEntryModel _selectedRootModel;
        #endregion

        #region Public Properties

        public ObservableCollection<IEntryModel> RootModels { get { return _rootModels; } }
        public IEntryModel SelectedRootModel { get { return _selectedRootModel; } set { _selectedRootModel = value; NotifyOfPropertyChange(() => SelectedRootModel); } }
        public bool ExpandRootDirectories { get { return _expandRootDirectories; } set { _expandRootDirectories = value; NotifyOfPropertyChange(() => ExpandRootDirectories); } }
        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }
        public bool EnableMultiSelect { get { return _enableMultiSelect; } set { _enableMultiSelect = value; NotifyOfPropertyChange(() => EnableMultiSelect); } }
        public string FileFilter { get { return _fileFilter; } set { _fileFilter = value; NotifyOfPropertyChange(() => FileFilter); } }

        #endregion



    }
}
