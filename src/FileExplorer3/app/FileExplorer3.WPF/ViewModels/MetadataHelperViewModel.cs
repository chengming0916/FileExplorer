using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    //public interface IMetadataHelperViewModel : 
    //{
    //    IObservableCollection<IMetadataViewModel> All { get; }
    //    Task UpdateMetadaAsync(IFileListViewModel flvm);
    //}

    public class MetadataHelperViewModel : EntriesHelper<IMetadataViewModel>
    {
        #region Cosntructor

        public MetadataHelperViewModel(Func<IMetadata, bool> filter = null)
            : base()
        {
            _loadSubEntryFunc = (b,p) => loadEntriesTask(p as IFileListViewModel); 
            _filter = filter ?? (m => true);
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<IMetadataViewModel>> loadEntriesTask(IFileListViewModel flvm)
        {
            if (flvm == null)
                return new List<IMetadataViewModel>();

            return (await flvm.CurrentDirectory.Profile.MetadataProvider.GetMetadataAsync(
                    flvm.Selection.SelectedItems.Select(evm => evm.EntryModel),
                    flvm.ProcessedEntries.All.Count,
                    flvm.CurrentDirectory))
                    .Where(m => _filter(m))
                    .Distinct()
                    .Select(m => MetadataViewModel.FromMetadata(m));
        }


        #endregion

        #region Data

        private Func<IMetadata, bool> _filter;
        //private IFileListViewModel _flvm;


        #endregion

        #region Public Properties

        #endregion




    }
}

