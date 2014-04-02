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
    public class StatusbarViewModel : PropertyChangedBase, IStatusbarViewModel,
        IHandle<SelectionChangedEvent>, IHandle<ListCompletedEvent>
    {
        #region Cosntructor

        public StatusbarViewModel(IEventAggregator events)
        {
            _events = events;            
            _displayItems = new BindableCollection<IEntryViewModel>();
            _allMetadataItems = new BindableCollection<IMetadataViewModel>();


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
            SelectionCount = flvm.Selection.SelectedItems.Count();

            Items.AddRange(flvm.CurrentDirectory.Profile.MetadataProvider.GetMetadata(
                flvm.Selection.SelectedItems.Select(evm => evm.EntryModel), 
                flvm.ProcessedEntries.All.Count, 
                flvm.CurrentDirectory).Select(m => MetadataViewModel.FromMetadata(m)));

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

                    DisplayItems.Add(EntryViewModel.FromEntryModel(flvm.CurrentDirectory));
                    break;
                case 1:
                    //Caption = String.Format(OneSelected, flvm.SelectedItems.First().EntryModel.Label);
                    DisplayItems.Add(flvm.Selection.SelectedItems.First());
                    break;
                default:
                    //Caption = String.Format(ManySelected, flvm.SelectedItems.Count.ToString());
                    DisplayItems.AddRange(flvm.Selection.SelectedItems.Take(5));
                    break;
            }
        }

        public void Handle(SelectionChangedEvent message)
        {
            if (message.Sender is IFileListViewModel)
            {
                updateDisplayItemsAndCaption(message.Sender as IFileListViewModel);


                //if (SelectionCount == 1)
                //    Caption = message.SelectedViewModels.First()
            }
        }

        public void Handle(ListCompletedEvent message)
        {
            if (message.Sender is IFileListViewModel)
            {
                updateDisplayItemsAndCaption(message.Sender as IFileListViewModel);
            }
        }

        #endregion

        #region Data
        
        bool _isExpanded = false;
        int _selectionCount;
        string _caption;
        IObservableCollection<IEntryViewModel> _displayItems;
        IObservableCollection<IMetadataViewModel> _allMetadataItems;
        string _selectedViewMode = "Icon";
        private IEventAggregator _events;
        

        #endregion

        #region Public Properties

   
        public bool IsExpanded { get { return _isExpanded; } set { if (_isExpanded != value) { _isExpanded = value;
        NotifyOfPropertyChange(() => IsExpanded);
            OnIsExpandedChanged(_isExpanded); } } }

        public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }
        public IObservableCollection<IEntryViewModel> DisplayItems { get { return _displayItems; } }
        public IObservableCollection<IMetadataViewModel> Items { get { return _allMetadataItems; } }


        public string Caption { get { return _caption; } set { _caption = value; NotifyOfPropertyChange(() => Caption); } }

        #endregion



      
    }
}
