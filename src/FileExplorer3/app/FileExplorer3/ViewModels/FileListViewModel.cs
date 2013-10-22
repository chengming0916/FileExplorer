using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    [Export(typeof(FileListViewModel))]
    public class FileListViewModel : Screen, IEntryListViewModel
    {
        #region Cosntructor

        public FileListViewModel(IEventAggregator events)
        {
            Events = events;

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

        public IEnumerable<IResult> Load(IProfile profile, IEntryModel em, Func<IEntryModel, bool> filter = null)
        {
            var parentEVm = EntryViewModel.FromEntryModel(profile, em);
            yield return Loader.Show("Loading");

            yield return new LoadEntryList(parentEVm, filter);
            yield return new AppendEntryList(parentEVm, this);
            SelectedEntries.Add(Items.First());
            yield return Loader.Show();

        }

        public IEnumerable<IResult> ChangeView(string viewMode)
        {
            yield return new ChangeView(this, viewMode);
            yield return new AddLabelColumn(_colList.ToArray());
        }


        #endregion

        #region Data

        private IObservableCollection<IEntryViewModel> _entryVms = new BindableCollection<IEntryViewModel>();
        private IObservableCollection<IEntryViewModel> _selectedVms = new BindableCollection<IEntryViewModel>();
        private int _itemSize = 60, _cacheCount = 5;
        private string _viewMode;
        private Orientation _orientation = Orientation.Vertical;
        private TimeSpan _itemAnimateDuration = TimeSpan.FromSeconds(5);
        private List<ListViewColumnInfo> _colList = new List<ListViewColumnInfo>()
        {
            ListViewColumnInfo.FromTemplate("Name", "GridLabelTemplate", 200),   
            ListViewColumnInfo.FromBindings("Description", "EntryModel.Description", "", 200)   
        };

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; private set; }

        #region Orientation, CacheCount
        public Orientation Orientation { get { return _orientation; } set { _orientation = value; NotifyOfPropertyChange(() => Orientation); } }
        public int CacheCount { get { return _cacheCount; } set { _cacheCount = value; NotifyOfPropertyChange(() => CacheCount); } }
        #endregion

        #region ItemSize - ItemHeight
        public int ItemSize
        {
            get { return _itemSize; }
            set
            {
                _itemSize = value;
                NotifyOfPropertyChange(() => ItemSize);
                NotifyOfPropertyChange(() => ItemWidth);
                NotifyOfPropertyChange(() => ItemHeight);

            }
        }

        public int ItemWidth { get { return _itemSize; } }
        public int ItemHeight { get { return _itemSize; } }

        #endregion

        public string ViewMode
        {
            get { return _viewMode; }
            set
            {
                NotifyOfPropertyChange(() => ViewMode);
                Events.Publish(new ViewChangedEvent(value, _viewMode));
                _viewMode = value;
            }
        }
        public TimeSpan ItemAnimateDuration
        {
            get { return _itemAnimateDuration; }
            set { _itemAnimateDuration = value; NotifyOfPropertyChange(() => ItemAnimateDuration); }
        }

        public IObservableCollection<IEntryViewModel> Items { get { return _entryVms; } }
        public IObservableCollection<IEntryViewModel> SelectedEntries { get { return _selectedVms; } }



        #endregion


    }
}
