using System;
using System.Collections.Generic;
#if WINRT
using Windows.UI.Xaml.Controls;
#else
using System.ComponentModel.Composition;
using System.Windows.Controls;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using FileExplorer.ViewModels.Actions;
using FileExplorer.Utils;
using System.Collections.ObjectModel;
using FileExplorer.ViewModels.Helpers;


namespace FileExplorer.ViewModels
{
#if !WINRT
    [Export(typeof(FileListViewModel))]
#endif
    public class FileListViewModel : PropertyChangedBase, IFileListViewModel,
        IHandle<ViewChangedEvent>, IHandle<DirectoryChangedEvent>//, ISupportDrag, ISupportDrop
    {
        #region Cosntructor

        public FileListViewModel(IEventAggregator events)
        {
            Events = events;
            var entryHelper = new EntriesHelper<IEntryViewModel>(loadEntriesTask);
            ProcessedEntries = new EntriesProcessor<IEntryViewModel>(entryHelper);
            Columns = new ColumnsHelper(ProcessedEntries);
            if (events != null)
                events.Subscribe(this);
            #region Unused
            //var ec = ConventionManager.AddElementConvention<ListView>(
            //   ListView.ItemsSourceProperty, "ItemsSource", "SourceUpdated");
            //ec.ApplyBinding = (vmType, path, property, element, convention) =>
            //{
            //    ConventionManager.SetBinding(vmType, path, property, element,
            //        ec, ItemsControl.ItemsSourceProperty);
            //    return true;
            //};
            #endregion
        }

        #endregion

        #region Methods

        async Task<IEnumerable<IEntryViewModel>> loadEntriesTask()
        {
            if (CurrentDirectory == null)
                return new List<IEntryViewModel>();

            var subEntries = await CurrentDirectory.Profile.ListAsync(CurrentDirectory);
            return subEntries.Select(s => CreateSubmodel(s));
        }

        public IEntryViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return EntryViewModel.FromEntryModel(entryModel);
        }


        #region Actions

        public async Task LoadAsync(IEntryModel em)
        {
            CurrentDirectory = em;
            await ProcessedEntries.EntriesHelper.LoadAsync(true);
            Columns.CalculateColumnHeaderCount(from vm in ProcessedEntries.EntriesHelper.AllNonBindable select vm.EntryModel);            
        }

        /// <summary>
        /// Virtual panel require this to unselect all entries using view models.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IResult> UnselectAll()
        {
            yield return new UnselectAll(ProcessedEntries.EntriesHelper.AllNonBindable);
        }


        public IEnumerable<IResult> ToggleRename()
        {
            yield return new ToggleRename(this);
        }

        #endregion

        #region OnPropertyChanged

        protected virtual void OnSortDirectoryChanged(ColumnInfo col, ListSortDirection direction)
        {
            if (CurrentDirectory == null)
                return;
            var comparer = new EntryViewModelComparer(
                col.Comparer != null ? col.Comparer : Profile.GetComparer(col),
                direction);
            ProcessedEntries.Sort(comparer, col.ValuePath);
        }

        public void OnSelectionChanged(IList selectedItems)
        {
            return;

            SelectedItems = selectedItems.Cast<IEntryViewModel>().Distinct().ToList();
            Events.Publish(new SelectionChangedEvent(this, SelectedItems));
        }

        public void OnFilterChanged()
        {
            Columns.OnFilterChanged();
        }

        #endregion

        public void Handle(ViewChangedEvent message)
        {
            if (!(message.Sender.Equals(this)))
                this.ViewMode = message.ViewMode;
        }

        public void Handle(DirectoryChangedEvent message)
        {
            if (message.NewModel != null)
                LoadAsync(message.NewModel);
        }

        #region ISupportDrag

        public bool HasDraggables
        {
            get { return  _selectedVms.Any(); }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return _selectedVms;
        }

        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            return Profile.GetDataObject(draggables.Cast<IEntryViewModel>().Select(vm => vm.EntryModel));                        
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return Profile.QueryDrag(draggables.Cast<IEntryViewModel>().Select(vm => vm.EntryModel));
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        {
            Profile.OnDragCompleted(draggables.Cast<IEntryViewModel>().Select(vm => vm.EntryModel), da, effect);
        }

        public bool IsDroppable
        {
            get { return true; }
        }

        public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            return Profile.QueryDrop(Profile.GetEntryModels(da), CurrentDirectory, allowedEffects);
        }

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            return Profile.GetEntryModels(da).Select(em => EntryViewModel.FromEntryModel(em));
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            return Profile.OnDropCompleted(draggables.Cast<IEntryViewModel>().Select(vm => vm.EntryModel), da, 
                CurrentDirectory, allowedEffects);
        }

        #endregion

        #endregion

        #region Data

        private IEntryModel _parentVm = null;
        private IList<IEntryViewModel> _selectedVms = new List<IEntryViewModel>();
        private DragDropEffects _dragDropEffects = DragDropEffects.Copy | DragDropEffects.Link;

        private int _itemSize = 60;
        private string _viewMode = "Icon";
        private string _sortBy = "EntryModel.Label";
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;

        private bool _isDraggingOver;

        #endregion

        #region Public Properties

        public IEntriesProcessor<IEntryViewModel> ProcessedEntries { get; private set; }
        public IColumnsHelper Columns { get; private set; }
        public IEventAggregator Events { get; private set; }

        public IEntryModel CurrentDirectory
        {
            get { return _parentVm; }
            set { _parentVm = value; NotifyOfPropertyChange(() => CurrentDirectory); }
        }

        public IProfile Profile
        {
            get { return CurrentDirectory == null ? null : CurrentDirectory.Profile; }
        }

        #region SortBy, SortDirection

        public string SortBy
        {
            get { return _sortBy; }
            set
            {
                if (_sortBy != value)
                {
                    _sortBy = value;
                    OnSortDirectoryChanged(Columns.ColumnList.Find(_sortBy), _sortDirection);
                    NotifyOfPropertyChange(() => SortBy);
                }
            }
        }

        public ListSortDirection SortDirection
        {
            get { return _sortDirection; }
            set
            {
                if (_sortDirection != value)
                {
                    _sortDirection = value;
                    OnSortDirectoryChanged(Columns.ColumnList.Find(_sortBy), _sortDirection);
                    NotifyOfPropertyChange(() => SortDirection);
                }
            }
        }

        #endregion

        #region ViewMode, ItemSize

        public string ViewMode
        {
            get { return _viewMode; }
            set
            {
                _viewMode = value;
                NotifyOfPropertyChange(() => ViewMode);
            }
        }

        public int ItemSize
        {
            get { return _itemSize; }
            set
            {
                _itemSize = value;
                NotifyOfPropertyChange(() => ItemSize);
            }
        }

     

        #endregion

        #region Items, ProcessedItems, SelectedItems

        public IList<IEntryViewModel> SelectedItems
        {
            get { return _selectedVms; }
            set
            {
                _selectedVms = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }

        #endregion


        public bool IsDraggingOver
        {
            get { return _isDraggingOver; }
            set { _isDraggingOver = value; NotifyOfPropertyChange(() => IsDraggingOver); }
        }

        public DragDropEffects DragDropEffects { get { return _dragDropEffects; } set { _dragDropEffects = value; } }


        #endregion







        
    }
}


