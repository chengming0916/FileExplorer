using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.WPF.Models
{
    public class ColumnFilter : PropertyChangedBase, IColumnFilter
    {

        #region Cosntructor
        public static ColumnFilter CreateNew(string header, string valuePath, Func<IEntryModel, bool> filter = null)
        {
            return new ColumnFilter()
            {
                Header = header,
                ValuePath = valuePath,
                Matches = filter == null ? (e) => true : filter
            };
        }

        #endregion

        #region Methods

        public static bool Match(IColumnFilter[] filters, IEntryModel model)
        {
            foreach (var filterGroup in filters.GroupBy(f => f.ValuePath))
            {
                bool match = false;
                foreach (var f in filterGroup)
                    if (f.Matches(model))
                        match = true;
                if (!match) return false;
            }
            return true;
        }

        #endregion

        #region Data

        private string _header;
        private bool _isChecked = false;
        private string _valuePath = "";
        private Func<IEntryModel, bool> _match;
        private long _matchCount = 0;

        #endregion

        #region Public Properties

        public string Header
        {
            get { return _header; }
            set { _header = value; NotifyOfPropertyChange(() => Header); }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; NotifyOfPropertyChange(() => IsChecked); }
        }

        public string ValuePath
        {
            get { return _valuePath; }
            set { _valuePath = value; NotifyOfPropertyChange(() => ValuePath); }
        }

        public Func<IEntryModel, bool> Matches
        {
            get { return _match; }
            set { _match = value; NotifyOfPropertyChange(() => Matches); }
        }

        public long MatchedCount
        {
            get { return _matchCount; }
            set { _matchCount = value; NotifyOfPropertyChange(() => MatchedCount); }
        }

        #endregion





       
    }
}
