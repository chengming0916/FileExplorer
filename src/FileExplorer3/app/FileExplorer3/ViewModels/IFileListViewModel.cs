using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public interface IFileListViewModel
    {

        IEnumerable<IResult> Load(IEntryModel em, Func<IEntryModel, bool> filter = null);

        Task<IList<IEntryModel>> LoadAsync(IEntryModel em, Func<IEntryModel, bool> filter = null);

        IObservableCollection<IEntryViewModel> Items { get; }
        IList<IEntryViewModel> SelectedItems { get; }

        IEventAggregator Events { get; }

        ColumnInfo[] ColumnList { get; set; }

        ColumnFilter[] ColumnFilters { get; set; }

        IEntryViewModel CurrentDirectory { get; }

        string ViewMode { get; set; }
        int ItemSize { get; set; }
    }
}
