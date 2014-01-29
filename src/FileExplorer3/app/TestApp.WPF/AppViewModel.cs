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

namespace TestApp.WPF
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

            _profile = new FileSystemInfoProfile(_events);
            _profileEx = new FileSystemInfoExProfile(events);

            RootModels.Add(_profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result);
        }

        #endregion

        #region Methods

        private IExplorerViewModel initExplorerModel(IExplorerViewModel explorerModel)
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
                ColumnFilter.CreateNew("Files", "EntryModel.Description", e => !e.IsDirectory),
            };
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

            _explorer.FileList.ScriptCommands.Open =
              FileList.IfSelection(evm => evm.Count() == 1,
                  FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                      FileList.OpenSelectedDirectory, //Selected directory                        
                      FileList.AssignSelectionToParameter(
                          new OpenWithScriptCommand(null))),  //Selected non-directory
                  ResultCommand.NoError //Selected more than one item, ignore.
                  );

            _explorer.FileList.ScriptCommands.Delete =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                    new IfOkCancel(_windowManager, pd => "Delete",
                        pd => String.Format("Delete {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                         FileList.AssignSelectionToParameter(
                             DeleteFileBasedEntryCommand.FromParameter
                             ),
                        ResultCommand.NoError),
                    NullScriptCommand.Instance);

            //new ShowMessageBox(_windowManager, "Delete", "Pending to implement.");

            _explorer.FileList.ToolbarCommands.ExtraCommandProviders = new[] { 
                new StaticCommandProvider(
                    new CommandModel(ApplicationCommands.Open) { IsVisibleOnToolbar = false },
                    new SeparatorCommandModel(),
                    new SelectGroupCommand( _explorer.FileList),    
                    new ViewModeCommand( _explorer.FileList),
                    new SeparatorCommandModel(),
                    new CommandModel(FileListCommands.Refresh) { IsVisibleOnToolbar = false },
                    new CommandModel(ApplicationCommands.Delete)  { IsVisibleOnToolbar = false }
                    
                    )
            };


            updateExplorerModel(initExplorerModel(_explorer));
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
            var filePicker = new FilePickerViewModel(_events, _windowManager, FileFilter, RootModels.ToArray());
            updateExplorerModel(initExplorerModel(filePicker));
            if (_windowManager.ShowDialog(filePicker).Value)
            {
                MessageBox.Show(String.Join(",", filePicker.SelectedFiles.Select(em => em.FullPath)));
            }
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
            if (_windowManager.ShowDialog(initExplorerModel(directoryPicker)).Value)
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

        private string loginSkyDrive()
        {
            var login = new SkyDriveLogin(Properties.Settings.Default.skydrive_client_id);
            if (_windowManager.ShowDialog(new LoginViewModel(login)).Value)
            {
                Properties.Settings.Default.skydrive_auth_code = login.AuthCode;
                Properties.Settings.Default.Save();
                return login.AuthCode;
            }
            return null;
        }

        public static string skyDriveAliasMask = "{0}'s OneDrive"; 
        public async Task AddSkyDrive()
        {
            
            if (_profileSkyDrive == null)
                _profileSkyDrive = new SkyDriveProfile(_events, Properties.Settings.Default.skydrive_client_id, loginSkyDrive, skyDriveAliasMask);
            var rootModel = new[] { await _profileSkyDrive.ParseAsync("") };
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
        }

        public async Task TestUpload()
        {
            if (_profileSkyDrive == null)
                _profileSkyDrive = new SkyDriveProfile(_events, Properties.Settings.Default.skydrive_client_id, loginSkyDrive, skyDriveAliasMask);
            //var photos = await _profileSkyDrive.ParseAsync("/photos");
            var uploadtxt = new SkyDriveItemModel(_profileSkyDrive as SkyDriveProfile, "/upload1.txt", false);
            //string ioPath = _profileSkyDrive.PathMapper[uploadtxt].IOPath;
            //Directory.CreateDirectory(Path.GetDirectoryName(ioPath));
            //var rootModel = new[] { await _profileSkyDrive.ParseAsync("/photos") };
            //var newFile = new SkyDriveItemModel(_profileSkyDrive as SkyDriveProfile,
            //    "/me/skydrive/upload.txt", "/SkyDrive/upload.txt", "/me/skydrive", 1);
            //string ioPath = _profileSkyDrive.PathMapper[newFile].IOPath;
            //using (var sw = new StreamWriter(File.Create(ioPath)))
            //    sw.WriteLine("upload");
            using (var sw = new StreamWriter(SkyDriveFileStream.OpenWrite(uploadtxt)))
                sw.WriteLine("upload123");
            //await _profileSkyDrive.PathMapper.UpdateSourceAsync(uploadtxt);           
        }

        public void ShowDialog()
        {
            _windowManager.ShowDialog(new MessageDialogViewModel("Caption", "Message 1 2 3 4 5 6 7 8 9 10",
                MessageDialogViewModel.DialogButtons.OK | MessageDialogViewModel.DialogButtons.Cancel));
        }

        #endregion

        #region Data

        IProfile _profile;
        IProfile _profileEx;
        IProfile _profileSkyDrive;

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

        public ObservableCollection<IEntryModel> RootModels { get { return _rootModels; } }
        public bool ExpandRootDirectories { get { return _expandRootDirectories; } set { _expandRootDirectories = value; NotifyOfPropertyChange(() => ExpandRootDirectories); } }
        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }
        public bool EnableMultiSelect { get { return _enableMultiSelect; } set { _enableMultiSelect = value; NotifyOfPropertyChange(() => EnableMultiSelect); } }
        public string FileFilter { get { return _fileFilter; } set { _fileFilter = value; NotifyOfPropertyChange(() => FileFilter); } }

        #endregion



    }
}
