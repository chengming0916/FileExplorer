using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;

namespace FileExplorer.WPF.ViewModels.Helpers
{
    public interface IColumnsHelper
    {

        ColumnInfo[] ColumnList { get; set; }
        ColumnFilter[] ColumnFilters { get; set; }

        event EventHandler FilterChanged;
        event EventHandler SortChanged;
        void CalculateColumnHeaderCount(IEnumerable<IEntryModel> entryModels);
        IComparer GetComparer(ColumnInfo colInfo, ListSortDirection direction);
    }

}
