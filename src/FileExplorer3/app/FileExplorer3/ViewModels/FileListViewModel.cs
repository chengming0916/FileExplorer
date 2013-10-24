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


namespace FileExplorer.ViewModels
{
#if !WINRT
    [Export(typeof(FileListViewModel))]
#endif
    public class FileListViewModel : Screen, IEntryListViewModel
    {
        #region Cosntructor

        public FileListViewModel(IEventAggregator events)
        {
            Events = events;
            _processedVms = CollectionViewSource.GetDefaultView(Items) as ListCollectionView;
            //var ec = ConventionManager.AddElementConvention<ListView>(
            //   ListView.ItemsSourceProperty, "ItemsSource", "SourceUpdated");
            //ec.ApplyBinding = (vmType, path, property, element, convention) =>
            //{
            //    ConventionManager.SetBinding(vmType, path, property, element,
            //        ec, ItemsControl.ItemsSourceProperty);
            //    return true;
            //};

        }

        #endregion

        #region Methods

        //public async Task LoadAsync(IProfile profile, IEntryModel parentEm, Func<IEntryModel, bool> filter = null)
        //{

        //    var parentEVm = EntryViewModel.FromEntryModel(profile, parentEm);
        //    var result = await profile.ListAsync(parentEm, filter);

        //    foreach (var em in result)
        //    {
        //        var evm = EntryViewModel.FromEntryModel(profile, em);
        //        Items.Add(evm);
        //    }
        //}       

        public IEnumerable<IResult> Load(IEntryModel em, Func<IEntryModel, bool> filter = null)
        {            
            var parentEVm = EntryViewModel.FromEntryModel(Profile, em);
            yield return Loader.Show("Loading");
            yield return new LoadEntryList(parentEVm, filter);
            yield return new AppendEntryList(parentEVm, this);
            //SelectedEntries.Add(Items.First());
            yield return Loader.Show();

        }

        public void Rename()
        {
            if (SelectedItems.Count > 0)
                SelectedItems[0].IsEditing = true;
        }

        protected virtual void OnSortDirectoryChanged(ListViewColumnInfo col, ListSortDirection direction)
        {
            if (_profile == null)
                return;           
            var comparer = new EntryViewModelComparer(_profile.GetComparer(col), direction);
            _processedVms.CustomSort = comparer;

            _processedVms.GroupDescriptions.Add(new PropertyGroupDescription(col.ValuePath));

            //ProcessedItems.SortDescriptions.Clear();
            //ProcessedItems.SortDescriptions.Add(new SortDescription(col.ValuePath, direction ));
            //ProcessedItems = Items.OrderByDescending(evm => evm.EntryModel.Description);
        }
        
        public void OnSelectionChanged(IList selectedItems)
        {
            SelectedItems = selectedItems.Cast<IEntryViewModel>().ToList();
            Events.Publish(new SelectionChangedEvent(SelectedItems));
        }

        #endregion

        #region Data

        private IProfile _profile;

        private IObservableCollection<IEntryViewModel> _items = new BindableCollection<IEntryViewModel>();
        private IList<IEntryViewModel> _selectedVms = null;
        private ListCollectionView _processedVms = null;
        private int _itemSize = 60;
        private string _viewMode = "Icon";
        private string _sortBy = "EntryModel.Label";
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        //private Orientation _orientation = Orientation.Vertical;
        private ListViewColumnInfo[] _colList = new ListViewColumnInfo[]
        {
            ListViewColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", 200),   
            ListViewColumnInfo.FromBindings("Description", "EntryModel.Description", "", 200)   
        };

        #endregion

        #region Public Properties

        public IProfile Profile { get { return _profile; } set { _profile = value; NotifyOfPropertyChange(() => Profile); } }

        public IEventAggregator Events { get; private set; }

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

        #region ViewMode, ItemAnimateDuration
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

        public ListViewColumnInfo[] ColumnList
        {
            get { return _colList; }
            set { _colList = value; NotifyOfPropertyChange(() => ColumnList); }
        }
        //public List<ListViewColumnInfo> ColumnList { get { return _colList; } s

        #endregion


        public IObservableCollection<IEntryViewModel> Items { get { return _items; } }

        public CollectionView ProcessedItems
        {
            get { return _processedVms; }
        }

        public IList<IEntryViewModel> SelectedItems
        {
            get { return _selectedVms; }
            set { _selectedVms = value; 
                NotifyOfPropertyChange(() => SelectedItems); }
        } 



        #endregion


    }
}

//#region Orientation, CacheCount
//public Orientation Orientation { get { return _orientation; } set { _orientation = value; NotifyOfPropertyChange(() => Orientation); } }
//public int CacheCount { get { return _cacheCount; } set { _cacheCount = value; NotifyOfPropertyChange(() => CacheCount); } }

//#endregion

//#region ItemSize - ItemHeight
//public int ItemSize
//{
//    get { return _itemSize; }
//    set
//    {
//        _itemSize = value;
//        NotifyOfPropertyChange(() => ItemSize);
//        NotifyOfPropertyChange(() => ItemWidth);
//        NotifyOfPropertyChange(() => ItemHeight);

//    }
//}

//public int ItemWidth { get { return _itemSize; } }
//public int ItemHeight { get { return _itemSize; } }

//#endregion

