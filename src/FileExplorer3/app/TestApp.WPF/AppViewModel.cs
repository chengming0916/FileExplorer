﻿using System;
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

        public static IExplorerViewModel initExplorerModel(IExplorerViewModel explorerModel,
            bool updateColumns, bool updateScriptCommands,
            IEntryModel[] rootModels, IWindowManager windowManager)
        {
            if (updateColumns)
            {
                explorerModel.FileList.Columns.ColumnList = new ColumnInfo[] 
                {
                    ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", new ValueComparer<IEntryModel>(p => p.Label), 200),   
                    ColumnInfo.FromBindings("Description", "EntryModel.Description", "", new ValueComparer<IEntryModel>(p => p.Description), 200),
                    ColumnInfo.FromTemplate("FSI.Size", "GridSizeTemplate", "", new ValueComparer<IEntryModel>(p => (p as FileSystemInfoExModel).Size), 200),  
                    ColumnInfo.FromBindings("FSI.Attributes", "EntryModel.Attributes", "", new ValueComparer<IEntryModel>(p => (p as FileSystemInfoModel).Attributes), 200)   
                };

                explorerModel.FileList.Columns.ColumnFilters = new ColumnFilter[]
                {
                    ColumnFilter.CreateNew("0 - 9", "EntryModel.Label", e => Regex.Match(e.Label, "^[0-9]").Success),
                    ColumnFilter.CreateNew("A - H", "EntryModel.Label", e => Regex.Match(e.Label, "^[A-Ha-h]").Success),
                    ColumnFilter.CreateNew("I - P", "EntryModel.Label", e => Regex.Match(e.Label, "^[I-Pi-i]").Success),
                    ColumnFilter.CreateNew("Q - Z", "EntryModel.Label", e => Regex.Match(e.Label, "^[Q-Zq-z]").Success),
                    ColumnFilter.CreateNew("The rest", "EntryModel.Label", e => Regex.Match(e.Label, "^[^A-Za-z0-9]").Success),
                    ColumnFilter.CreateNew("Directories", "EntryModel.Description", e => e.IsDirectory),
                    ColumnFilter.CreateNew("Files", "EntryModel.Description", e => !e.IsDirectory)
                };
            }

            if (updateScriptCommands)
            {
                explorerModel.FileList.Commands.ScriptCommands.Open =
              FileList.IfSelection(evm => evm.Count() == 1,
                  FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                      FileList.OpenSelectedDirectory, //Selected directory                        
                      FileList.AssignSelectionToParameter(
                          new OpenWithScriptCommand(null))),  //Selected non-directory
                  ResultCommand.NoError //Selected more than one item, ignore.
                  );

                explorerModel.FileList.Commands.ScriptCommands.NewFolder =
                    FileList.Do(flvm => ScriptCommands.CreatePath(
                            flvm.CurrentDirectory, "NewFolder", true, true,
                            m => FileList.Refresh(FileList.Select(fm => fm.Equals(m), ResultCommand.OK), true)));

                explorerModel.FileList.Commands.ScriptCommands.Delete =
                     FileList.IfSelection(evm => evm.Count() >= 1,
                        ScriptCommands.IfOkCancel(windowManager, pd => "Delete",
                            pd => String.Format("Delete {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                            new ShowProgress(windowManager,
                                        ScriptCommands.RunInSequence(
                                            FileList.AssignSelectionToParameter(
                                                DeleteFileBasedEntryCommand.FromParameter),
                                            new HideProgress())),
                            ResultCommand.NoError),
                        NullScriptCommand.Instance);

                explorerModel.FileList.Commands.ScriptCommands.Copy =
                     FileList.IfSelection(evm => evm.Count() >= 1,
                       ScriptCommands.IfOkCancel(windowManager, pd => "Copy",
                            pd => String.Format("Copy {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                                ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Copy)),
                                ResultCommand.NoError),
                        NullScriptCommand.Instance);

                explorerModel.FileList.Commands.ScriptCommands.Cut =
                      FileList.IfSelection(evm => evm.Count() >= 1,
                       ScriptCommands.IfOkCancel(windowManager, pd => "Cut",
                            pd => String.Format("Cut {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                                ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Cut)),
                                ResultCommand.NoError),
                        NullScriptCommand.Instance);

                explorerModel.FileList.Commands.ToolbarCommands.ExtraCommandProviders = new[] { 
                new FileBasedCommandProvider(), //Open, Cut, Copy, Paste etc
                new StaticCommandProvider(
                    new SeparatorCommandModel(),
                    new SelectGroupCommand( explorerModel.FileList),    
                    new ViewModeCommand( explorerModel.FileList),
                    new GoogleExportCommandModel(() => rootModels)
                    { IsVisibleOnToolbar = false, WindowManager = windowManager },
                    
                    new SeparatorCommandModel(),
                    new CommandModel(ExplorerCommands.NewFolder) { IsVisibleOnMenu = false, Symbol = Convert.ToChar(0xE188) },
                    new DirectoryCommandModel(new CommandModel(ExplorerCommands.NewFolder) { Header = Strings.strFolder })
                        { IsVisibleOnToolbar = false, Header = Strings.strNew, IsEnabled = true}
                    )
            };

                explorerModel.DirectoryTree.Commands.ToolbarCommands.ExtraCommandProviders = new[] { 
                new StaticCommandProvider(
                    new CommandModel(ExplorerCommands.Refresh) { IsVisibleOnToolbar = false },
                    new CommandModel(ApplicationCommands.Delete)  { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnToolbar = false }                    
                    )
              };

                explorerModel.DirectoryTree.Commands.ScriptCommands.Delete =
                       ScriptCommands.IfOkCancel(windowManager, pd => "Delete",
                           pd => String.Format("Delete {0}?", ((pd["DirectoryTree"] as IDirectoryTreeViewModel).Selection.RootSelector.SelectedValue.Label)),
                                 new ShowProgress(windowManager,
                                        ScriptCommands.RunInSequence(
                                            DirectoryTree.AssignSelectionToParameter(
                                                DeleteFileBasedEntryCommand.FromParameter),
                                            new HideProgress())),
                           ResultCommand.NoError);


                explorerModel.Commands.ScriptCommands.Transfer =
                    TransferCommand =
                    new TransferCommand((effect, source, destDir) =>
                        source.Profile is IDiskProfile ?
                            (IScriptCommand)new FileTransferScriptCommand(source, destDir, effect == DragDropEffects.Move)
                            : ResultCommand.Error(new NotSupportedException())
                        , windowManager);
            }

            return explorerModel;
        }

        private void updateExplorerModel(IExplorerViewModel explorerViewModel)
        {
            explorerViewModel.FileList.EnableDrag = EnableDrag;
            explorerViewModel.FileList.EnableDrop = EnableDrop;
            explorerViewModel.FileList.EnableMultiSelect = EnableMultiSelect;
            explorerViewModel.DirectoryTree.EnableDrag = EnableDrag;
            explorerViewModel.DirectoryTree.EnableDrop = EnableDrop;
            if (_expandRootDirectories)
                explorerViewModel.DirectoryTree.ExpandRootEntryModels();
        }

        public void OpenWindow()
        {
            _explorer = new ExplorerViewModel(_events, _windowManager, RootModels.ToArray());




            updateExplorerModel(initExplorerModel(_explorer, true, true, RootModels.ToArray(), _windowManager));
            _windowManager.ShowWindow(_explorer);
        }

        public void UpdateWindow()
        {
            if (_explorer != null)
            {
                _explorer.RootModels = RootModels.ToArray();
                updateExplorerModel(_explorer);
            }
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
            var directoryPicker = new DirectoryPickerViewModel(_events, _windowManager, rootModels);
            directoryPicker.DirectoryTree.ExpandRootEntryModels();
            directoryPicker.FileList.EnableDrag = false;
            directoryPicker.FileList.EnableDrop = false;
            directoryPicker.FileList.EnableMultiSelect = false;
            directoryPicker.DirectoryTree.EnableDrag = false;
            directoryPicker.DirectoryTree.EnableDrop = false;
            if (_windowManager.ShowDialog(initExplorerModel(directoryPicker, true, false, RootModels.ToArray(), _windowManager)).Value)
                return directoryPicker.SelectedDirectory;
            return null;
        }

        public void Clear()
        {
            RootModels.Clear();
        }

        public void AddDirectoryInfo()
        {
            var rootModel = new[] { _profile.ParseAsync("C:\\").Result };
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
        }

        public void AddDirectoryInfoEx()
        {
            var rootModel = new[] { _profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result };
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
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
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
        }

        public async Task AddGoogleDrive()
        {

            if (_profileGoogleDrive == null)
                using (var gapi_secret_stream = System.IO.File.OpenRead("gapi_client_secret.json")) //For demo only.
                {
                    _profileGoogleDrive = new GoogleDriveProfile(_events, _windowManager, gapi_secret_stream);
                }
            var rootModel = new[] { await _profileGoogleDrive.ParseAsync("") };
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
        }

        public void ShowDialog()
        {
            _windowManager.ShowDialog(new MessageDialogViewModel("Caption", "Message 1 2 3 4 5 6 7 8 9 10",
                MessageDialogViewModel.DialogButtons.OK | MessageDialogViewModel.DialogButtons.Cancel));
        }

        public void MdiWindow()
        {
            new MdiWindow()
            {
                _profileEx = _profileEx,
                _events = _events,
                _windowManager = _windowManager
            }
                .Show();
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
        private bool _enableDrag, _enableDrop, _enableMultiSelect;

        private ObservableCollection<IEntryModel> _rootModels = new ObservableCollection<IEntryModel>();
        private string _fileFilter = "Texts (.txt)|*.txt|Pictures (.jpg, .png)|*.jpg,*.png|Songs (.mp3)|*.mp3|All Files (*.*)|*.*";
        #endregion

        #region Public Properties
        public static IScriptCommand TransferCommand { get; private set; }

        public ObservableCollection<IEntryModel> RootModels { get { return _rootModels; } }
        public bool ExpandRootDirectories { get { return _expandRootDirectories; } set { _expandRootDirectories = value; NotifyOfPropertyChange(() => ExpandRootDirectories); } }
        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }
        public bool EnableMultiSelect { get { return _enableMultiSelect; } set { _enableMultiSelect = value; NotifyOfPropertyChange(() => EnableMultiSelect); } }
        public string FileFilter { get { return _fileFilter; } set { _fileFilter = value; NotifyOfPropertyChange(() => FileFilter); } }

        #endregion



    }
}
