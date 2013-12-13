using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IFileListViewModel
    {
        IEntriesProcessor<IEntryViewModel> ProcessedEntries { get; }
        IColumnsHelper Columns { get; }
        IListSelector<IEntryViewModel, IEntryModel> Selection { get; }

        Task LoadAsync(IEntryModel em);        

        //ObservableCollection<IEntryViewModel> Items { get; }
        //IList<IEntryViewModel> SelectedItems { get; }
        //CollectionView ProcessedItems { get; }

        IEventAggregator Events { get; }


        IEntryModel CurrentDirectory { get; }

        string ViewMode { get; set; }
        int ItemSize { get; set; }
        
    }
}
