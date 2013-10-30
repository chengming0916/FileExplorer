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

        #region Actions
        
        /// <summary>
        /// Load sub entries in the specified entry model to Items, the filter is used to filter entries out 
        /// before added to items, while ColumnFilter update ProcessedItems so some added entries are not displayed.
        /// </summary>
        /// <param name="em"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<IResult> Load(IEntryModel em, Func<IEntryModel, bool> filter = null)
        {                        
            var parentEVm = EntryViewModel.FromEntryModel(em);
            yield return Loader.Show("Loading");
            yield return new DoSomething((c) => {  Items.Clear(); });
            yield return new LoadEntryList(parentEVm, filter);
            yield return new AppendEntryList(parentEVm, this);
            yield return new CalculateColumnHeaderCount(ColumnFilters);
            yield return Loader.Show();            
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
            if (_profile == null)
                return;
            var comparer = new EntryViewModelComparer(_profile.GetComparer(col), direction);
            _processedVms.CustomSort = comparer;
            _processedVms.GroupDescriptions.Add(new PropertyGroupDescription(col.ValuePath));
        }

        public void OnSelectionChanged(IList selectedItems)
        {
            SelectedItems = selectedItems.Cast<IEntryViewModel>().ToList();
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
        private ColumnInfo[] _colList = new ColumnInfo[]
        {
            ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", 200),   
            ColumnInfo.FromBindings("Description", "EntryModel.Description", "", 200)   
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

        public IObservableCollection<IEntryViewModel> Items { get { return _items; } }

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



        #endregion


    }
}


