using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    public interface IColumnsHelper
    {
        ColumnInfo[] ColumnList { get; set; }
        ColumnFilter[] ColumnFilters { get; set; }

        void OnFilterChanged();
        void CalculateColumnHeaderCount(IEnumerable<IEntryModel> entryModels);
    }

}
