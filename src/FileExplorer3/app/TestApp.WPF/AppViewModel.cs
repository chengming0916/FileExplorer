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

namespace TestApp.WPF
{
    [Export(typeof(IScreen))]
    public class AppViewModel : Screen//, IHandle<SelectionChangedEvent>
    {
        static string rootPath = @"C:\";
        static string rootPath2 = @"C:\Temp";
        static string lookupPath = @"C:\";

        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events, IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _events = events;

            RootModels.Add(_profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result);
            //ExplorerModel = processExplorerModel(new ExplorerViewModel(events, windowManager)
            //{
            //    RootModels = new[] 
            //    {
            //        _profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result,
            //        //profile.ParseAsync(rootPath).Result
            //    }
            //});
        }

        #endregion

        #region Methods

        private IExplorerViewModel initExplorerModel(IExplorerViewModel explorerModel)
        {

            explorerModel.FileList.ScriptCommands.Open =
                new IfFileListSelection(evm => evm.Count == 1,
                    new IfFileListSelection(evm => evm[0].EntryModel.IsDirectory,
                        new OpenSelectedDirectory(), //Selected directory                        
                        new AssignSelectionToParameterAsEntryModelArray(
                            new OpenWithScriptCommand(null))),  //Selected non-directory
                    ResultCommand.NoError //Selected more than one item, ignore.
                    );

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
        
        public async Task AddSkyDrive()
        {
            if (_profileSkyDrive == null)
                _profileSkyDrive = new SkyDriveProfile(Properties.Settings.Default.skydrive_client_id, loginSkyDrive);
            var rootModel = new[] { await _profileSkyDrive.ParseAsync("/me/skydrive") };            
            IEntryModel selectedModel = showDirectoryPicker(rootModel);
            if (selectedModel != null)
                RootModels.Add(selectedModel);
        }

        #endregion

        #region Data

        IProfile _profile = new FileSystemInfoProfile();
        IProfile _profileEx = new FileSystemInfoExProfile();
        IProfile _profileSkyDrive;

        //private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };
        //private string _addPath = lookupPath;
        private IEventAggregator _events;
        private IWindowManager _windowManager;
        private IExplorerViewModel _explorer = null;
        private bool _expandRootDirectories = false;
        private bool _enableDrag, _enableDrop, _enableMultiSelect;

        private ObservableCollection<IEntryModel> _rootModels = new ObservableCollection<IEntryModel>();
        #endregion

        #region Public Properties

        public ObservableCollection<IEntryModel> RootModels { get { return _rootModels; } }
        public bool ExpandRootDirectories { get { return _expandRootDirectories; } set { _expandRootDirectories = value; NotifyOfPropertyChange(() => ExpandRootDirectories); } }
        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }
        public bool EnableMultiSelect { get { return _enableMultiSelect; } set { _enableMultiSelect = value; NotifyOfPropertyChange(() => EnableMultiSelect); } }


        #endregion



    }
}
