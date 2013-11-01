using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
#if !WINRT
    [Export(typeof(StatusbarViewModel))]
#endif
    public class StatusbarViewModel : Screen, IStatusbarViewModel,
        IHandle<SelectionChangedEvent>, IHandle<ListCompletedEvent>, IHandle<ViewChangedEvent>
    {
        #region Cosntructor

        public StatusbarViewModel(IExplorerViewModel explorerModel, IEventAggregator events)
        {
            _events = events;
            _explorerModel = explorerModel;
            _displayItems = new BindableCollection<IEntryViewModel>();
            _metadataItems = new BindableCollection<IMetadataViewModel>();
            _viewModes = new BindableCollection<ViewModeViewModel>();
            _viewModes.Add(new ViewModeViewModel("IconView"));
            _viewModes.Add(new ViewModeViewModel("SmallIconView"));
            _viewModes.Add(new ViewModeViewModel("GridView"));
            events.Subscribe(this);
        }

        #endregion

        #region Methods

        public static string NoneSelected = "{0}";
        public static string OneSelected = "{0}";
        public static string ManySelected = "{0} items selected";

        private void updateDisplayItemsAndCaption(IFileListViewModel flvm)
        {
            DisplayItems.Clear();
            MetadataItems.Clear();
            SelectionCount = flvm.SelectedItems.Count();

            MetadataItems.Add(MetadataViewModel.FromText("", String.Format("{0} items", flvm.Items.Count())));
            if (SelectionCount > 0)
                MetadataItems.Add(MetadataViewModel.FromText("", String.Format("{0} items selected", SelectionCount)));

            //MetadataItems.Add(MetadataViewModel.FromTotalItems(flvm.Items.Count()));
            //MetadataItems.Add(MetadataViewModel.FromMetadata(new Metadata(DisplayType.Percent, "abc", 50)));

            switch (SelectionCount)
            {
                case 0:
                    //Caption = String.Format(NoneSelected, flvm.CurrentDirectory.EntryModel.Label);

                    DisplayItems.Add(flvm.CurrentDirectory);
                    break;
                case 1:
                    //Caption = String.Format(OneSelected, flvm.SelectedItems.First().EntryModel.Label);
                    DisplayItems.Add(flvm.SelectedItems.First());
                    break;
                default:
                    //Caption = String.Format(ManySelected, flvm.SelectedItems.Count.ToString());
                    DisplayItems.AddRange(flvm.SelectedItems.Take(5));
                    break;
            }
        }

        public void Handle(SelectionChangedEvent message)
        {
            if (message.Sender.Equals(_explorerModel.FileListModel))
            {

                updateDisplayItemsAndCaption(message.Sender as IFileListViewModel);


                //if (SelectionCount == 1)
                //    Caption = message.SelectedViewModels.First()
            }
        }

        public void Handle(ListCompletedEvent message)
        {
            if (message.Sender.Equals(_explorerModel.FileListModel))
            {
                updateDisplayItemsAndCaption(message.Sender as IFileListViewModel);
            }
        }

        public void Handle(ViewChangedEvent message)
        {
            if (!(message.Sender.Equals(this)))
                this.SelectedViewMode = message.ViewMode;
        }
        #endregion

        #region Data

        IExplorerViewModel _explorerModel;
        int _selectionCount;
        string _caption;
        IObservableCollection<IEntryViewModel> _displayItems;
        IObservableCollection<IMetadataViewModel> _metadataItems;
        IObservableCollection<ViewModeViewModel> _viewModes;
        string _selectedViewMode = "Icon";
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        public string SelectedViewMode
        {
            get { return _selectedViewMode; }
            set
            {
                string orgViewMode = _selectedViewMode;
                _selectedViewMode = value;
                NotifyOfPropertyChange(() => SelectedViewMode);
                _events.Publish(new ViewChangedEvent(this, value, orgViewMode));
            }
        }
        public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }
        public IObservableCollection<IEntryViewModel> DisplayItems { get { return _displayItems; } }
        public IObservableCollection<IMetadataViewModel> MetadataItems { get { return _metadataItems; } }
        public IObservableCollection<ViewModeViewModel> ViewModes { get { return _viewModes; } }
        public string Caption { get { return _caption; } set { _caption = value; NotifyOfPropertyChange(() => Caption); } }

        #endregion



      
    }
}
