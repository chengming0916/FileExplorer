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
        IHandle<ViewChangedEvent>, IHandle<DirectoryChangedEvent>, ISupportDrag, ISupportDrop
    {
        #region Cosntructor

        public FileListViewModel(IEventAggregator events)
        {
            Events = events;
            _processedVms = CollectionViewSource.GetDefaultView(Items) as ListCollectionView;
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

        #region Utils

        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task<IList<IEntryModel>> listAsync(IEntryViewModel parentModel, Func<IEntryModel, bool> filter = null)
        {
            if (filter == null)
                filter = (em) => true;
            var result = await parentModel.EntryModel.Profile.ListAsync(parentModel.EntryModel, filter);
            var entryModels = from m in result
                              where filter(m)
                              select (IEntryModel)m;
            return entryModels.ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void appendEntryList<T>(ObservableCollection<T> collection, IList<IEntryModel> entryModels,
            Func<IEntryModel, T> conversion)
        {
            if (collection is FastObservableCollection<T>)
            {
                var fastCol = collection as FastObservableCollection<T>;
                fastCol.AddItems((from em in entryModels select conversion(em)).ToList());
            }
            else
                foreach (var em in entryModels)
                {
                    collection.Add(conversion(em));
                }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void replaceEntryList<T>(ObservableCollection<T> collection, IList<IEntryModel> entryModels,
            Func<IEntryModel, T> conversion)
        {
            collection.Clear();
            appendEntryList(collection, entryModels, conversion);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void calculateColumnHeaderCount(ColumnFilter[] filters, IList<IEntryModel> entryModels)
        {
            foreach (var f in filters)
                f.MatchedCount = 0;

            foreach (var em in entryModels)
                foreach (var f in filters)
                    if (f.Matches(em))
                        f.MatchedCount++;
        }

        protected T findMatched<T>(IEntryModel model, IObservableCollection<T> lookupList, Func<T, IEntryModel, bool> matchFunc)
        {
            foreach (var evm in lookupList)
                if (matchFunc(evm, model))
                {
                    return evm;
                }
            return default(T);
        }

        #endregion

        #region Actions

        public virtual Task<IList<IEntryModel>> ListAsync(Func<IEntryModel, bool> filter = null)
        {
            return listAsync(CurrentDirectory, filter);
        }

        public async Task<IList<IEntryModel>> LoadAsync(IEntryModel em, Func<IEntryModel, bool> filter = null)
        {
            CurrentDirectory = EntryViewModel.FromEntryModel(em);
            SelectedItems.Clear();
            var entryModels = await ListAsync(filter);
            replaceEntryList(this.Items, entryModels, (subem) => EntryViewModel.FromEntryModel(subem));
            calculateColumnHeaderCount(ColumnFilters, entryModels);
            Events.Publish(new ListCompletedEvent(this, Items));
            return entryModels;
        }

        /// <summary>
        /// Virtual panel require this to unselect all entries using view models.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IResult> UnselectAll()
        {
            yield return new UnselectAll(Items);
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
            _processedVms.CustomSort = comparer;
            _processedVms.GroupDescriptions.Add(new PropertyGroupDescription(col.ValuePath));
        }

        public void OnSelectionChanged(IList selectedItems)
        {
            SelectedItems = selectedItems.Cast<IEntryViewModel>().Distinct().ToList();
            Events.Publish(new SelectionChangedEvent(this, SelectedItems));
        }

        public void OnFilterChanged()
        {
            var allCheckedFilters = ColumnFilters.Where(f => f.IsChecked).ToArray();

            if (allCheckedFilters.Length == 0)
                _processedVms.Filter = null;
            else
                _processedVms.Filter = (e) => (ColumnFilter.Match(allCheckedFilters, (e as IEntryViewModel).EntryModel));
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
                LoadAsync(message.NewModel, null);
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

        public DragDropEffects QueryDrop(IDataObject da)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            throw new NotImplementedException();
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Data

        private IEntryViewModel _parentVm = null;

        private ObservableCollection<IEntryViewModel> _items = new FastObservableCollection<IEntryViewModel>();
        private IList<IEntryViewModel> _selectedVms = new List<IEntryViewModel>();
        private ListCollectionView _processedVms = null;
        private DragDropEffects _dragDropEffects = DragDropEffects.Copy | DragDropEffects.Link;
        private int _itemSize = 60;
        private string _viewMode = "Icon";
        private string _sortBy = "EntryModel.Label";
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        private ColumnInfo[] _colList = new ColumnInfo[]
        {
            ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", null, 200),   
            ColumnInfo.FromBindings("Description", "EntryModel.Description", "", null, 200)   
        };

        private ColumnFilter[] _colFilters = new ColumnFilter[] { };
        //new List<ListViewColumnFilter>() 
        //{
        //    new ListViewColumnFilter("Life", "EntryModel.Label"),
        //    new ListViewColumnFilter("Universe", "EntryModel.Label"), 
        //    new ListViewColumnFilter("Everything", "EntryModel.Label") { IsChecked = true }
        //}.ToArray();

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; private set; }

        public IEntryViewModel CurrentDirectory
        {
            get { return _parentVm; }
            set { _parentVm = value; NotifyOfPropertyChange(() => CurrentDirectory); }
        }

        public IProfile Profile
        {
            get { return CurrentDirectory == null ? null : CurrentDirectory.EntryModel.Profile; }
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
                    OnSortDirectoryChanged(ColumnList.Find(_sortBy), _sortDirection);
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
                    OnSortDirectoryChanged(ColumnList.Find(_sortBy), _sortDirection);
                    NotifyOfPropertyChange(() => SortDirection);
                }
            }
        }

        #endregion

        #region ViewMode, ItemSize, ColumnList, ColumnFilters

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

        public ColumnInfo[] ColumnList
        {
            get { return _colList; }
            set { _colList = value; NotifyOfPropertyChange(() => ColumnList); }
        }

        public ColumnFilter[] ColumnFilters
        {
            get { return _colFilters; }
            set { _colFilters = value; NotifyOfPropertyChange(() => ColumnFilters); }
        }

        #endregion

        #region Items, ProcessedItems, SelectedItems

        public ObservableCollection<IEntryViewModel> Items { get { return _items; } }

        public CollectionView ProcessedItems
        {
            get { return _processedVms; }
        }

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


        public DragDropEffects DragDropEffects { get { return _dragDropEffects; } set { _dragDropEffects = value; } }


        #endregion







        
    }
}


