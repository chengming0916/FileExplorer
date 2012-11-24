﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickZip.UserControls.MVVM;
using QuickZip.UserControls.MVVM.ViewModel;
using QuickZip.UserControls.MVVM.Model;
using System.Collections;
//using QuickZip.IO.COFE.UserControls.ViewModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Diagnostics;
using Cinch;
using QuickZip.UserControls.Input;
using System.ComponentModel;
using System.Windows.Media;
using QuickZip.MVVM;
using System.IO;
using System.Windows;

namespace QuickZip.UserControls.MVVM
{
    public enum OpenMode { OpenInner, OpenOuter, None }

    public abstract class ViewerBaseVM : BaseToolbarViewModel
    {

        public ViewerBaseVM()
        {

            IsSimpleStatusbar = true;
            StatusText = this.GetType().ToString();
            setupCommands();
            //_statusbarViewModel = new SimpleStatusbarViewModel() { Status = "ViewerBaseViewModel" };
        }

        #region Methods
        /// <summary>
        /// Invoked when the viewmodel unloaded (e.g. changed to another view model)
        /// </summary>
        public abstract void OnUnload();

        /// <summary>
        /// 
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Invoked when ExplorerViewModel.ExpandCommand triggered
        /// </summary>
        public abstract void Expand();

        /// <summary>
        /// Invoked when ExplorerViewModel.ContextMenuCommand triggered, null means no further action required.
        /// </summary>
        public virtual string ContextMenu(Point pos) { return null; }

        protected abstract string getToolTip();
        protected abstract string getLabel();
        protected abstract ImageSource getSmallIcon();



        #region Broadcast FileSystem Changes

        public virtual void BroadcastChange(string parseName, WatcherChangeTypesEx changeType)
        {

        }

        #endregion



        protected virtual void setupCommands()
        {
            ContextMenuCommand = new SimpleCommand()
            {
                CanExecuteDelegate = (x) =>
                {
                    ViewerBaseVM vm = (x as ViewerBaseVM);
                    return vm.IsContextMenuEnabled;
                },
                ExecuteDelegate = (x) =>
                {
                    ViewerBaseVM vm = (x as ViewerBaseVM);
                    if (vm.IsContextMenuEnabled)
                    {
                        Point pt = UITools.GetScreenMousePosition();
                        switch (vm.ContextMenu(pt))
                        {
                            case "open": if (ExpandCommand != null) ExpandCommand.Execute(vm); break;
                            case "rename": if (RenameCommand != null) RenameCommand.Execute(vm); break;
                            case "refresh": if (RefreshCommand != null) RefreshCommand.Execute(vm); break;
                        }
                    }
                }
            };

            RefreshCommand = new SimpleCommand()
            {
                ExecuteDelegate = (x) =>
                    {
                        Refresh();
                    }
            };

            //ExpandCommand = new SimpleCommand()
            //{
            //    CanExecuteDelegate =
            //        (x) =>
            //        {
            //            ViewerBaseViewModel<FI, DI, FSI> vm = (x as ViewerBaseViewModel<FI, DI, FSI>);
            //            return vm == null ? false : vm.CanExpand;
            //        },
            //    ExecuteDelegate =
            //       (x) =>
            //       {
            //           ViewerBaseViewModel<FI, DI, FSI> vm = (x as ViewerBaseViewModel<FI, DI, FSI>);
            //           vm.Expand();
            //       }
            //};
        }
        #endregion

        public EventHandler<DirectoryChangedEventArgs> DirectoryChanged = (o, args) => { };

        #region Data
        private bool _isBreadcrumbVisible = true;
        private bool _showDirectoryTree = false, _enableContextMenu = false;
        public string _mediaFile = null;
        private ViewerMode _viewerMode = ViewerMode.None;
        //private BaseStatusbarViewModel _statusbarViewModel;        


        //Statusbar        
        private bool _isSelectedViewModelsEmpty = false;
        #endregion

        #region Public Properties
        //public BaseStatusbarViewModel StatusbarViewModel { get { return _statusbarViewModel; } protected set { _statusbarViewModel = value; NotifyPropertyChanged("StatusbarViewModel"); } }                    

        public bool IsContextMenuEnabled
        {
            get { return _enableContextMenu; }
            protected set { _enableContextMenu = value; NotifyPropertyChanged("IsContextMenuEnabled"); }
        }

        public bool IsDirectoryTreeEnabled
        {
            get { return _showDirectoryTree; }
            protected set { _showDirectoryTree = value; NotifyPropertyChanged("IsDirectoryTreeEnabled"); }
        }

        /// <summary>
        /// On demand, return a label that represent the current view model.
        /// </summary>        
        public string Label { get { return getLabel(); } }

        /// <summary>
        /// On demand, return a text tooltip that represent the current view model.
        /// </summary>        
        public string ToolTip { get { return getToolTip(); } }

        /// <summary>
        /// On demand, return a Small Icon that represent the current view model.
        /// </summary>        
        public ImageSource SmallIcon { get { return getSmallIcon(); } }


        public bool IsBreadcrumbVisible
        {
            get { return _isBreadcrumbVisible; }
            set { _isBreadcrumbVisible = value; NotifyPropertyChanged("IsBreadcrumbVisible"); }
        }

        public string MediaFile
        {
            get { return _mediaFile; }
            protected set { _mediaFile = value; NotifyPropertyChanged("MediaFile"); }
        }



        #region Statusbar



        public bool IsSelectedViewModelsEmpty
        {
            get { return _isSelectedViewModelsEmpty; }
            protected set { _isSelectedViewModelsEmpty = value; NotifyPropertyChanged("IsSelectedViewModelsEmpty"); }
        }

        #endregion

        #region UI, Current Browser (Directory, W3 or File)
        public ViewerMode CurrentViewerMode
        {
            get { return _viewerMode; }
            set
            {
                _viewerMode = value;
                NotifyPropertyChanged("CurrentViewerMode");
                NotifyPropertyChanged("IsDirectoryViewModel");
                NotifyPropertyChanged("IsWWWViewModel");
                NotifyPropertyChanged("IsMediaViewModel");
            }
        }

        /// <summary>
        /// Used by View to decide to show which control
        /// </summary>
        public bool IsDirectoryViewModel
        {
            get { return CurrentViewerMode == ViewerMode.Directory; }
        }

        /// <summary>
        /// Used by View to decide to show which control
        /// </summary>
        public bool IsWWWViewModel
        {
            get { return CurrentViewerMode == ViewerMode.W3; }
        }

        /// <summary>
        /// Used by View to decide to show which control
        /// </summary>
        public bool IsMediaViewModel
        {
            get { return CurrentViewerMode == ViewerMode.Media; }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Can be a file list view (CurrentDirectoryViewModel), or a file view (CurrentFileViewModel, e.g. Internal Image Viewer)
    /// </summary>
    /// <typeparam name="FI"></typeparam>
    /// <typeparam name="DI"></typeparam>
    /// <typeparam name="FSI"></typeparam>
    public abstract class ViewerBaseViewModel<FI, DI, FSI> : ViewerBaseVM
        where DI : FSI
        where FI : FSI
    {

        static ViewerBaseViewModel()
        {

        }

        public ViewerBaseViewModel(Profile<FI, DI, FSI> profile,
            EntryModel<FI, DI, FSI> embedEntryModel)
            : this(profile)
        {
            _embeddedEntryViewModel = profile.ConstructEntryViewModel(embedEntryModel);
            IsBookmarked = profile.GetIsBookmarked(embedEntryModel);
        }

        public ViewerBaseViewModel(Profile<FI, DI, FSI> profile,
            EntryViewModel<FI, DI, FSI> embedEntryViewModel)
            : this(profile)
        {
            _embeddedEntryViewModel = embedEntryViewModel;

        }

        public ViewerBaseViewModel(Profile<FI, DI, FSI> profile)
        {
            _profile = profile;
            setupCommands();
        }


        #region Methods

        protected override void OnDispose()
        {
            base.OnDispose();
            EmbeddedEntryViewModel = null;
        }

        protected virtual void OnSelectionChanged()
        {

        }



        protected override void setupCommands()
        {
            base.setupCommands();

            ExpandCommand = new SimpleCommand()
            {
                CanExecuteDelegate =
                    (x) =>
                    {
                        ViewerBaseViewModel<FI, DI, FSI> vm = (x as ViewerBaseViewModel<FI, DI, FSI>);
                        return vm == null ? IsExpandEnabled : vm.IsExpandEnabled;
                    },
                ExecuteDelegate =
                   (x) =>
                   {
                       ViewerBaseViewModel<FI, DI, FSI> vm = (x as ViewerBaseViewModel<FI, DI, FSI>);
                       if (vm != null) vm.Expand(); else Expand();
                   }
            };

            UnselectAllCommand = new SimpleCommand()
            {
                ExecuteDelegate =
                (x) =>
                {
                    unselectAll();
                }
            };


            ToggleBookmarkCommand = new SimpleCommand()
            {
                CanExecuteDelegate =
                (x) => { return _profile.GetBookmarkEnabled(EmbeddedEntryViewModel.EmbeddedModel); },
                ExecuteDelegate =
                (x) =>
                {
                    string newPath = null;
                    if (_isBookmarked)
                        newPath = _profile.RemoveBookmark(EmbeddedEntryViewModel.EmbeddedModel);
                    else newPath = _profile.AddBookmark(EmbeddedEntryViewModel.EmbeddedModel);

                    IsBookmarked = _profile.GetIsBookmarked(EmbeddedEntryViewModel.EmbeddedModel);

                    if (newPath != null)
                    {
                        var newModel = _profile.ConstructEntryModel(_profile.ConstructEntry(newPath));
                        if (newModel != null)
                            DirectoryChanged(this, new DirectoryChangedEventArgs<FI, DI, FSI>(newModel));
                    }
                }
            };


            //CopyCommand = new SimpleRoutedCommand(ApplicationCommands.Copy)
            //{
            //    CanExecuteDelegate = (x) =>
            //        {
            //            return true;
            //        },
            //    ExecuteDelegate =
            //    (x) =>
            //    {
            //        Debug.WriteLine("Copy");
            //    }                

            //}.RoutedCommand;

            //Memory Leak
            //SimpleRoutedCommand.RegisterClass(typeof(FileList2), _expandCommand);        
        }

        public override string ContextMenu(Point pos)
        {
            return _profile.ShowContextmenu(pos, SelectedModels.ToArray());
        }

        protected virtual void unselectAll() { }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //        return false;
        //    return base.ToString().Equals(obj.ToString());
        //}

        public override void UpdateStatusbar()
        {
            base.UpdateStatusbar();
            IsSelectedViewModelsEmpty = SelectedViewModels == null || SelectedViewModels.Count == 0;
            //Debug.WriteLine(IsSelectedViewModelsEmpty);
        }

        #endregion
        #region Data
        protected Profile<FI, DI, FSI> _profile;
        private OpenMode _directoryOpenMode = OpenMode.OpenInner;
        private OpenMode _fileOpenMode = OpenMode.OpenOuter;
        private EntryViewModel<FI, DI, FSI> _embeddedEntryViewModel;
        private List<EntryViewModel<FI, DI, FSI>> _selectedItems = new List<EntryViewModel<FI, DI, FSI>>();

        private int _viewSize = (int)ViewMode.vmGrid;
        private ViewMode _viewMode = ViewMode.vmGrid;
        private bool _isBookmarked = false;
        private bool _isLoading = false;

        #endregion

        #region Public Properties
        public bool IsExpandEnabled { get { return _selectedItems != null && _selectedItems.Count == 1; } }


        static PropertyChangedEventArgs isBookmarkedChangeArgs =
           ObservableHelper.CreateArgs<ViewerBaseViewModel<FI, DI, FSI>>(x => x.IsBookmarked);

        public bool IsBookmarked
        {
            get { return _isBookmarked; }
            set
            {
                _isBookmarked = value;
                NotifyPropertyChanged(isBookmarkedChangeArgs);
            }
        }

        static PropertyChangedEventArgs isLoadingChangeArgs =
        ObservableHelper.CreateArgs<ViewerBaseViewModel<FI, DI, FSI>>(x => x.IsLoading);

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged(isLoadingChangeArgs);
            }
        }

        public OpenMode DirectoryOpenMode { get { return _directoryOpenMode; } set { _directoryOpenMode = value; } }
        public OpenMode FileOpenMode { get { return _fileOpenMode; } set { _fileOpenMode = value; } }

        /// <summary>
        /// This is for communicate with UI only, dont access this property!
        /// </summary>
        public IList UISelectedItems
        {
            get { return null; }
            set
            {
                List<EntryViewModel<FI, DI, FSI>> retVal = new List<EntryViewModel<FI, DI, FSI>>();
                if (value != null)
                    foreach (object item in value)
                        if (item is EntryViewModel<FI, DI, FSI>)
                            retVal.Add(item as EntryViewModel<FI, DI, FSI>);
                SelectedViewModels = retVal;
            }
        }

        public List<EntryModel<FI, DI, FSI>> SelectedModels
        { get { return new List<EntryModel<FI, DI, FSI>>(from vm in SelectedViewModels select vm.EmbeddedModel); } }

        public List<EntryViewModel<FI, DI, FSI>> SelectedViewModels
        {
            get { return _selectedItems; }
            protected set
            {
                lock (_selectedItems)
                    _selectedItems = value;
                OnSelectionChanged();
                NotifyPropertyChanged("SelectedViewModels");
                UpdateStatusbar();
            }
        }

        #region ViewSize, ViewMode

        static PropertyChangedEventArgs ViewSizeChangeArgs =
            ObservableHelper.CreateArgs<ViewerBaseViewModel<FI, DI, FSI>>(x => x.ViewSize);

        /// <summary>
        /// Status of subentries listing
        /// </summary>
        public int ViewSize
        {
            get { return _viewSize; }
            set
            {
                _viewSize = value;
                NotifyPropertyChanged(ViewSizeChangeArgs);
            }
        }


        static PropertyChangedEventArgs ViewModeChangeArgs =
         ObservableHelper.CreateArgs<ViewerBaseViewModel<FI, DI, FSI>>(x => x.ViewMode);

        /// <summary>
        /// Status of subentries listing
        /// </summary>
        public ViewMode ViewMode
        {
            get { return _viewMode; }
            set
            {
                _viewMode = value;
                NotifyPropertyChanged(ViewModeChangeArgs);
            }
        }

        #endregion







        public EntryViewModel<FI, DI, FSI> EmbeddedEntryViewModel
        {
            get { return _embeddedEntryViewModel; }
            set { _embeddedEntryViewModel = value; NotifyPropertyChanged("EmbeddedEntryViewModel"); }
        }

        #endregion

    }
}
