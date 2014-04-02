using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public interface IMetadataHelperViewModel
    {
        IObservableCollection<IMetadataViewModel> Metadata { get; }
    }

    public class MetadataHelperViewModel : PropertyChangedBase, IMetadataHelperViewModel
    {
        #region Cosntructor

        public MetadataHelperViewModel(IEventAggregator events)
        {

            _allMetadataItems = new BindableCollection<IMetadataViewModel>();
            events.Subscribe(this);
        }

        #endregion

        #region Methods

        protected async Task updateMetadaAsync(IFileListViewModel flvm)
        {
            Metadata.Clear();
            Metadata.AddRange((await flvm.CurrentDirectory.Profile.MetadataProvider.GetMetadataAsync(
                    flvm.Selection.SelectedItems.Select(evm => evm.EntryModel),
                    flvm.ProcessedEntries.All.Count,
                    flvm.CurrentDirectory)).Select(m => MetadataViewModel.FromMetadata(m)));
        }


        #endregion

        #region Data

        IObservableCollection<IMetadataViewModel> _allMetadataItems;


        #endregion

        #region Public Properties


        public IObservableCollection<IMetadataViewModel> Metadata { get { return _allMetadataItems; } }

        #endregion




    }
}

