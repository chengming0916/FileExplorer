using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
  
    public class ColumnsHelper : NotifyPropertyChanged, IColumnsHelper
    {
        #region Constructor

        public ColumnsHelper(IEntriesProcessor processor)
        {
            _processor = processor;
        }

        #endregion

        #region Methods

        public void OnFilterChanged()
        {
            var checkedFilters = ColumnFilters.Where(f => f.IsChecked).ToArray();
            _processor.SetFilters(checkedFilters);
        }

        public void CalculateColumnHeaderCount(IEnumerable<IEntryModel> entryModels)
        {
            foreach (var f in _colFilters)
                f.MatchedCount = 0;

            foreach (var em in entryModels)
                foreach (var f in _colFilters)
                    if (f.Matches(em))
                        f.MatchedCount++;
        }

        #endregion

        #region Data


        private IEntriesProcessor _processor;
        private ColumnFilter[] _colFilters;
        private ColumnInfo[] _colList = new ColumnInfo[]
        {
            ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", null, 200),   
            ColumnInfo.FromBindings("Description", "EntryModel.Description", "", null, 200)   
        };

        #endregion

        #region Public Properties

        public ColumnInfo[] ColumnList
        {
            get { return _colList; }
            set { _colList = value; NotifyOfPropertyChanged(() => ColumnList); }
        }

        public ColumnFilter[] ColumnFilters
        {
            get { return _colFilters; }
            set { _colFilters = value; NotifyOfPropertyChanged(() => ColumnFilters); }
        }


        #endregion
    }
}
