using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
  

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
      

        public void ClearFilters()
        {
            All.Filter = null;
        }

        public void AppendFilters(params ColumnFilter[] filters)
        {
            if (All.Filter == null)
                All.Filter = (e) => e != null && ColumnFilter.Match(filters, (e as IEntryViewModel).EntryModel);
            else
                All.Filter = (e) =>
                    All.Filter(e) &&
                    ColumnFilter.Match(filters, (e as IEntryViewModel).EntryModel);
        }

        public void SetFilters(params ColumnFilter[] filters)
        {
            ClearFilters();
            AppendFilters(filters);
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
        public IColumnsHelper ColumnHelper { get; private set; }

      
        #endregion


     
    }
}
