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
    public interface IEntriesProcessor<VM>
    {
        IEntriesHelper<VM> EntriesHelper { get; }

        void Sort(IComparer comparer, string groupDescription);
        void ClearFilter();
        void AppendFilters(params ColumnFilter[] filters);

        ListCollectionView All { get; }
    }

    public class EntriesProcessor<VM> : IEntriesProcessor<VM>
    {
        #region Constructor

        public EntriesProcessor(IEntriesHelper<VM> entriesHelper)
        {
            EntriesHelper = entriesHelper;
        }

        #endregion

        #region Methods

        public void Sort(IComparer comparer, string groupDescription)
        {
            All.CustomSort = comparer;
            All.GroupDescriptions.Add(new PropertyGroupDescription(groupDescription));
        }

        public void ClearFilter()
        {
            All.Filter = null;
        }

        public void AppendFilters(params ColumnFilter[] filters)
        {
            if (All.Filter == null)
                All.Filter = (e) => ColumnFilter.Match(filters, (e as IEntryViewModel).EntryModel);
            else
                All.Filter = (e) =>
                    All.Filter(e) &&
                    ColumnFilter.Match(filters, (e as IEntryViewModel).EntryModel);
        }

        #endregion

        #region Data

        ListCollectionView _processedItems = null;

        #endregion

        #region Public Properties

        public ListCollectionView All
        {
            get
            {
                if (_processedItems == null)                
                    _processedItems = CollectionViewSource.GetDefaultView(EntriesHelper.All) as ListCollectionView;                
                return _processedItems;
            }
        }
        public IEntriesHelper<VM> EntriesHelper { get; private set; }

        #endregion

    }
}
