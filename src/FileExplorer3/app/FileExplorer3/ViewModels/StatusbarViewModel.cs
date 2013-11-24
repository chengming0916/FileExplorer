using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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
            _allMetadataItems = new BindableCollection<IMetadataViewModel>();


            _viewModes = new BindableCollection<ViewModeViewModel>();
            _viewModes.Add(new ViewModeViewModel("Icon"));
            _viewModes.Add(new ViewModeViewModel("SmallIcon"));
            _viewModes.Add(new ViewModeViewModel("Grid"));
            events.Subscribe(this);
        }

        #endregion

        #region Methods


        private void OnIsExpandedChanged(bool isExpanded)
        {
            Debug.WriteLine(isExpanded);
        }

        public static string NoneSelected = "{0}";
        public static string OneSelected = "{0}";
        public static string ManySelected = "{0} items selected";

        private void updateDisplayItemsAndCaption(IFileListViewModel flvm)
        {
           
            Items.Clear();
            SelectionCount = flvm.SelectedItems.Count();

            Items.AddRange(flvm.CurrentDirectory.EntryModel.Profile.MetadataProvider.GetMetadata(
                flvm.SelectedItems.Select(evm => evm.EntryModel), 
                flvm.ProcessedItems.Count, 
                flvm.CurrentDirectory.EntryModel).Select(m => MetadataViewModel.FromMetadata(m)));

            //Items.Add(MetadataViewModel.FromText("", String.Format("{0} items", flvm.Items.Count()), true));
            //if (SelectionCount > 0)
            //    Items.Add(MetadataViewModel.FromText("", String.Format("{0} items selected", SelectionCount), true));

            //Items.Add(MetadataViewModel.FromMetadata(new Metadata(DisplayType.Text, "text", "text")));
            //Items.Add(MetadataViewModel.FromMetadata(new Metadata(DisplayType.Percent, "percent", 50)));

            //MetadataItems.Add(MetadataViewModel.FromTotalItems(flvm.Items.Count()));
            //MetadataItems.Add(MetadataViewModel.FromMetadata(new Metadata(DisplayType.Percent, "abc", 50)));

            DisplayItems.Clear();
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
        bool _isExpanded = false;
        int _selectionCount;
        string _caption;
        IObservableCollection<IEntryViewModel> _displayItems;
        IObservableCollection<IMetadataViewModel> _allMetadataItems;
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

        public bool IsExpanded { get { return _isExpanded; } set { if (_isExpanded != value) { _isExpanded = value;
        NotifyOfPropertyChange(() => IsExpanded);
            OnIsExpandedChanged(_isExpanded); } } }

        public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }
        public IObservableCollection<IEntryViewModel> DisplayItems { get { return _displayItems; } }
        public IObservableCollection<IMetadataViewModel> Items { get { return _allMetadataItems; } }


        public IObservableCollection<ViewModeViewModel> ViewModes { get { return _viewModes; } }
        public string Caption { get { return _caption; } set { _caption = value; NotifyOfPropertyChange(() => Caption); } }

        #endregion



      
    }
}
