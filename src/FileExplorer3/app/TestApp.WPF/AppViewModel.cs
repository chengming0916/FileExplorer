﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.WPF.Defines;
using FileExplorer.Models;
using FileExplorer.WPF.ViewModels;
using FileExplorer;
using FileExplorer.WPF.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using FileExplorer.Utils;
using System.Configuration;
using System.IO;
using System.Windows.Input;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.WPF.BaseControls;
using System.Threading;
using FileExplorer;
using DropNet;
using DropNet.Models;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.Defines;

namespace TestApp
{
    public interface IAppViewModel
    {

    }


    [Export(typeof(IScreen))]
    public class AppViewModel : Screen, IAppViewModel, IHandle<RootChangedEvent>//, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events, IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _events = events;

            _events.Subscribe(this);
            _profile = new FileSystemInfoProfile(_events);
            _profileEx = new FileSystemInfoExProfile(events, windowManager, new FileExplorer.Models.SevenZipSharp.SzsProfile());

            Func<string> loginSkyDrive = () =>
            {
                var login = new SkyDriveLogin(AuthorizationKeys.SkyDrive_Client_Id);
                if (_windowManager.ShowDialog(new LoginViewModel(login)).Value)
                {
                    return login.AuthCode;
                }
                return null;
            };

            if (AuthorizationKeys.SkyDrive_Client_Secret != null)
                _profileSkyDrive = new SkyDriveProfile(_events, AuthorizationKeys.SkyDrive_Client_Id, loginSkyDrive, skyDriveAliasMask);


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

            if (AuthorizationKeys.DropBox_Client_Secret != null)
                _profileDropBox = new DropBoxProfile(_events,
                        AuthorizationKeys.DropBox_Client_Id,
                              AuthorizationKeys.DropBox_Client_Secret,
                              loginDropBox);

            if (System.IO.File.Exists("gapi_client_secret.json"))
                using (var gapi_secret_stream = System.IO.File.OpenRead("gapi_client_secret.json")) //For demo only.
                {
                    _profileGoogleDrive = new GoogleDriveProfile(_events, gapi_secret_stream);
                }


            RootModels.Add(AsyncUtils.RunSync(() => _profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName)));



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

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);


        }


        public void OpenWindowUsingScriptCommand()
        {
            var profiles = new IProfile[] {
                _profileEx, _profileSkyDrive, _profileDropBox, _profileGoogleDrive };

            IScriptCommand showWindowCommand = UIScriptCommands.ExplorerShow(                 
              
                  //OnModelCreated
                  ScriptCommands.RunCommandsInSequence(null, 
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.RootModels, "{RootDirectories}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrag, "{EnableDrag}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrop, "{EnableDrop}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableMultiSelect, "{EnableMultiSelect}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.ColumnList, "{ColumnList}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.ColumnFilters, "{ColumnFilters}"),
                        UIScriptCommands.ExplorerDo(epvm => 
                            {                        
                                ScriptCommandsInitializers.InitializeScriptCommands(epvm, _windowManager, _events, profiles);
                                ToolbarCommandsInitializers.InitializeToolbarCommands(epvm, _windowManager);
                            })), 

                  //OnViewAttached.
                  ScriptCommands.IfAssigned("{StartupPath}", 
                     UIScriptCommands.ExplorerParseAndGoTo("{Explorer}", "{Profiles}", "{StartupPath}"), 
                     ScriptCommands.AssignArrayItem("{RootDirectories}", 0, "{Root0}", 
                       UIScriptCommands.ExplorerGoTo("{Explorer}", "{Root0}", 
                            UIScriptCommands.DirectoryTreeToggleExpand("{Root0}")))),

                  "{WindowManager}", "{Events}", "{Explorer}",
                  
                  //ThenCommand
                  ResultCommand.NoError);
                            

            IEventAggregator evnts = new EventAggregator();
            IWindowManager wm = new WindowManager();
            FileSystemInfoExProfile profile = new FileSystemInfoExProfile(_events, _windowManager);
            var rootModel = AsyncUtils.RunSync(() => profile.ParseAsync(""));

            ScriptRunner.RunScriptAsync(new ParameterDic() { 
                    { "Profiles", profiles },
                    { "StartupPath", OpenPath },
                    { "RootDirectories", RootModels.ToArray() },	
                    { "Events", _events },
                    { "WindowManager", _windowManager },
				    { "EnableDrag", _enableDrag }, 
                    { "EnableDrop", _enableDrop },                     
                    { "EnableMultiSelect", _enableMultiSelect}, 
                    { "ColumnList", IOInitializeHelpers.FileList_ColumList_For_DiskBased_Items }, 
                    { "ColumnFilters", IOInitializeHelpers.FileList_ColumnFilter_For_DiskBased_Items }
                }, showWindowCommand);
        }

        public void OpenWindow(object context = null)
        {
            var profiles = new IProfile[] {
                _profileEx, _profileSkyDrive, _profileDropBox, _profileGoogleDrive };

            IExplorerInitializer initializer;

            if (UseScriptCommandInitializer)
            {
                OpenWindowUsingScriptCommand();
                return;
            }

            if (UseScriptCommandInitializer)
                //Use ScriptCommandInitializer
                initializer = new ScriptCommandInitializer()
             {
                 OnModelCreated = new IScriptCommand[] {
                    ScriptCommands.RunCommandsInSequence(
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.RootModels, RootModels.ToArray()),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrag, _enableDrag),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrop, _enableDrop),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableMultiSelect, _enableMultiSelect),
                        UIScriptCommands.ExplorerDo(epvm => 
                            {
                                ColumnInitializers.InitializeColumnInfo(epvm);
                                ScriptCommandsInitializers.InitializeScriptCommands(epvm, _windowManager, _events, profiles);
                                ToolbarCommandsInitializers.InitializeToolbarCommands(epvm, _windowManager);
                            })
                    ) },

                 OnViewAttached =
                 String.IsNullOrEmpty(OpenPath) ?
                    new IScriptCommand[] 
                    {                         
                        ScriptCommands.Assign("{Root}", RootModels.FirstOrDefault(), false,                     
                        UIScriptCommands.ExplorerGoTo("{Explorer}", "{Root}",
                        UIScriptCommands.DirectoryTreeToggleExpand("{Root}")))                    
                    } :
                    new IScriptCommand[] {                                                               
                           UIScriptCommands.ExplorerParseAndGoTo("{Explorer}", "{ProfileEx}", OpenPath)
                    },
                 StartupParameters = new ParameterDic()
                 {
                     { "ProfileEx", _profileEx }
                 }
             };
            else //Use ExplorerInitializer (Obsoluting)            
                initializer = new ExplorerInitializer(_windowManager, _events, RootModels.ToArray())
                {
                    Initializers = new List<IViewModelInitializer<IExplorerViewModel>>()
                    {
                         new BasicParamInitalizers(_expandRootDirectories, _enableMultiSelect, _enableDrag, _enableDrop),
                         new ColumnInitializers(),
                         new ScriptCommandsInitializers(_windowManager, _events, profiles),
                         new ToolbarCommandsInitializers(_windowManager)
                    }
                };


            ExplorerViewModel evm = new ExplorerViewModel(_windowManager, _events) { Initializer = initializer };
            _windowManager.ShowWindow(evm);



        }


        public void OpenToolWindow()
        {
            new TestApp.ToolWindowTest(RootModels.ToArray(), FileFilter, "c:\\").Show();
        }


        public void PickFiles()
        {
            ScriptRunner.RunScriptAsync(
                new ParameterDic() { { "WindowManager", _windowManager } },

                WPFScriptCommands.OpenFileDialog(_windowManager, _events, RootModels.ToArray(), FileFilter, "demo.txt",
                    (fpvm) => WPFScriptCommands.MessageBox("Open", fpvm.FileName), ResultCommand.OK));
            //var filePicker = new FilePickerViewModel(_events, _windowManager, FileFilter, FilePickerMode.Open, RootModels.ToArray());
            //updateExplorerModel(initExplorerModel(filePicker));
            //if (_windowManager.ShowDialog(filePicker).Value)
            //{
            //    MessageBox.Show(String.Join(",", filePicker.SelectedFiles.Select(em => em.FullPath)));
            //}
        }

        public void SaveFile()
        {
            ScriptRunner.RunScriptAsync(
                 new ParameterDic() 
                 { 
                 { "WindowManager", _windowManager }, 
                 { "RootModels", RootModels.ToArray() },
                 { "FilterStr", FileFilter },
                 { "ProfileEx", _profileEx }
                 },
               UIScriptCommands.FileSave(
                 ScriptCommands.RunCommandsInQueue(null, 
                    UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.RootModels, "{RootModels}",
                    UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.FilterStr, "{FilterStr}"))
                 ),
                 
                 UIScriptCommands.ExplorerParseAndGoTo("{Explorer}", "{ProfileEx}", ""),                 
                 "{WindowsManager}", "{Events}", "{DialogResult}", "{SelectionPaths}",
                  
                 ScriptCommands.IfTrue("{DialogResult}", 
                    UIScriptCommands.MessageBoxOK("SaveFile", "{SelectionPaths}"), 
                    UIScriptCommands.MessageBoxOK("SaveFile", "Cancelled"))
                    ));


            //ScriptRunner.RunScriptAsync(
            //     new ParameterDic() { { "WindowManager", _windowManager } },
            //   WPFScriptCommands.SaveFilePicker(_windowManager, null, RootModels.ToArray(), FileFilter, "demo.txt",
            //       (fpvm) => WPFScriptCommands.MessageBox("Save", fpvm.FileName), ResultCommand.OK));

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
            _events.PublishOnUIThread(new RootChangedEvent(ChangeType.Changed, RootModels.ToArray()));
        }

        public void Remove()
        {
            if (SelectedRootModel != null)
            {
                _events.PublishOnUIThread(new RootChangedEvent(ChangeType.Deleted, SelectedRootModel));
                RootModels.Remove(SelectedRootModel);
            }
        }

        private void pickAndAdd(IEntryModel[] rootModel)
        {
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            //if (selectedModel != null)
            //    RootModels.Add(selectedModel);
            _events.PublishOnUIThread(new RootChangedEvent(ChangeType.Created, selectedModel));
        }

        public async Task Add()
        {
            var initializer = getInitializer(_windowManager, /*_events*/ null, null);
            var profiles = new IProfile[] {
                _profileEx, _profileSkyDrive, _profileDropBox, _profileGoogleDrive
            };
            await ScriptRunner.RunScriptAsync(new ParameterDic() { { "Events", _events } },
              Explorer.PickDirectory(initializer, profiles,
                dir => new SimpleScriptCommand("AddToRootProfile",
                    pd =>
                    {
                        return WPFScriptCommands.PublishEvent(RootChangedEvent.Created(dir));
                    })
              , null));


            //var advm = new AddDirectoryViewModel(initializer, profiles);
            //if (_windowManager.ShowDialog(advm).Value)
            //{
            //    RootModels.Add(advm.SelectedDirectory);
            //    _events.Publish(new RootChangedEvent(ChangeType.Created, advm.SelectedDirectory));
            //}
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

            var rootModel = new[] { await _profileSkyDrive.ParseAsync("") };
            pickAndAdd(rootModel);
        }
        public bool CanAddSkyDrive { get { return _profileSkyDrive != null; } }

        public async Task AddGoogleDrive()
        {
            var rootModel = new[] { await _profileGoogleDrive.ParseAsync("") };
            pickAndAdd(rootModel);
        }

        public bool CanAddGoogleDrive { get { return _profileGoogleDrive != null; } }

        public async Task AddDropBox()
        {


            var rootModel = new[] { await _profileDropBox.ParseAsync("") };
            pickAndAdd(rootModel);
        }

        public bool CanAddDropBox { get { return _profileDropBox != null; } }

        public void ShowDialog()
        {
            _windowManager.ShowDialog(new MessageDialogViewModel("Caption", "Message 1 2 3 4 5 6 7 8 9 10",
                MessageDialogViewModel.DialogButtons.OK | MessageDialogViewModel.DialogButtons.Cancel));
        }

        public void ProgressDialog()
        {
            ScriptRunner.RunScript(
                WPFScriptCommands.ShowProgress(_windowManager, "Testing",
                    WPFScriptCommands.ReportProgress(TransferProgress.From("C:\\Demo\\FileExplorer3.txt", "http://fileexplorer.codeplex.com/FileExplorer3.txt"),
                    WPFScriptCommands.ReportProgress(TransferProgress.IncrementTotalEntries(100),
                    WPFScriptCommands.ReportProgress(TransferProgress.IncrementProcessedEntries(20),
                    WPFScriptCommands.ReportProgress(TransferProgress.UpdateCurrentProgress(50))))))
                );
            //_windowManager.ShowDialog(new ProgressDialogViewModel(new ParameterDic() 
            //{

            //}));
        }

        public void MdiWindow()
        {
            new MdiWindow(RootModels.ToArray()).Show();
        }

        public void TabWindow()
        {

            var profiles = new IProfile[] {
                _profileEx, _profileSkyDrive, _profileDropBox, _profileGoogleDrive
            };

            var initializer = getInitializer(_windowManager, _events, RootModels.ToArray(),
                ExplorerInitializers.Parameter(new FileListParameters() { ViewMode = "Icon", ItemSize = 100 }),
                ExplorerInitializers.Parameter(new ExplorerParameters() { UIScale = 1.1f, FileListSize = "3*", NavigationSize = 45 }),
                new BasicParamInitalizers(_expandRootDirectories, _enableMultiSelect, _enableDrag, _enableDrop),
                new ColumnInitializers(),
                new ScriptCommandsInitializers(_windowManager, _events, profiles),
                new ToolbarCommandsInitializers(_windowManager));

            var tabVM = new TabbedExplorerViewModel(initializer);

            //var windowManager = new TabbedAppWindowManager(tabVM);


            _windowManager.ShowWindow(tabVM);
        }

        public void Handle(RootChangedEvent message)
        {
            switch (message.ChangeType)
            {
                case ChangeType.Created:
                case ChangeType.Changed:
                    if (message.ChangeType == ChangeType.Changed)
                        RootModels.Clear();
                    foreach (var root in message.AppliedRootDirectories)
                        RootModels.Add(root);
                    break;
                case ChangeType.Deleted:
                    foreach (var root in message.AppliedRootDirectories)
                        RootModels.Remove(root);
                    break;
            }
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
        private string _openPath = "";

        private bool _useScriptCommandInitializer = true;

        private ObservableCollection<IEntryModel> _rootModels = new ObservableCollection<IEntryModel>();
        private string _fileFilter = "Texts (.txt)|*.txt|Pictures (.jpg, .png)|*.jpg,*.png|Songs (.mp3)|*.mp3|All Files (*.*)|*.*";
        private DropBoxProfile _profileDropBox;
        private IEntryModel _selectedRootModel;
        #endregion

        #region Public Properties


        public string OpenPath { get { return _openPath; } set { _openPath = value; NotifyOfPropertyChange(() => OpenPath); } }
        public ObservableCollection<IEntryModel> RootModels { get { return _rootModels; } }
        public IEntryModel SelectedRootModel { get { return _selectedRootModel; } set { _selectedRootModel = value; NotifyOfPropertyChange(() => SelectedRootModel); } }
        public bool ExpandRootDirectories { get { return _expandRootDirectories; } set { _expandRootDirectories = value; NotifyOfPropertyChange(() => ExpandRootDirectories); } }
        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }
        public bool EnableMultiSelect { get { return _enableMultiSelect; } set { _enableMultiSelect = value; NotifyOfPropertyChange(() => EnableMultiSelect); } }
        public bool UseScriptCommandInitializer { get { return _useScriptCommandInitializer; } set { _useScriptCommandInitializer = value; NotifyOfPropertyChange(() => UseScriptCommandInitializer); } }

        public string FileFilter { get { return _fileFilter; } set { _fileFilter = value; NotifyOfPropertyChange(() => FileFilter); } }

        #endregion





    }


}
