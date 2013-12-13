using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    public interface IEntriesProcessor
    {
        void Sort(IComparer comparer, string groupDescription);
        void ClearFilters();
        void AppendFilters(params ColumnFilter[] filters);
        void SetFilters(params ColumnFilter[] filters);

        ListCollectionView All { get; }
    }

    public interface IEntriesProcessor<VM> : IEntriesProcessor
    {
        IEntriesHelper<VM> EntriesHelper { get; }
    }
}
