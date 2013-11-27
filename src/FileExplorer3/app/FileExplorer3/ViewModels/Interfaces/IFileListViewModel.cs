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

namespace FileExplorer.ViewModels
{
    public interface IFileListViewModel
    {
       

        Task<IList<IEntryModel>> LoadAsync(IEntryModel em, Func<IEntryModel, bool> filter = null);

        ObservableCollection<IEntryViewModel> Items { get; }
        IList<IEntryViewModel> SelectedItems { get; }
        CollectionView ProcessedItems { get; }

        IEventAggregator Events { get; }

        ColumnInfo[] ColumnList { get; set; }

        ColumnFilter[] ColumnFilters { get; set; }

        IEntryViewModel CurrentDirectory { get; }

        string ViewMode { get; set; }
        int ItemSize { get; set; }
        
    }
}
