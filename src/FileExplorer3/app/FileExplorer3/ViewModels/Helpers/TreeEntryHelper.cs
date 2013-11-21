using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeEntryHelper<VM> : INotifyPropertyChanged
    {
        #region Constructor

        public TreeEntryHelper(Func<Task<IEnumerable<VM>>> loadSubEntryFunc)
        {
            _loadSubEntryFunc = loadSubEntryFunc;
            All.Add(default(VM));
        }

        public TreeEntryHelper(params VM[] entries)
        {
            _isLoaded = true;
            All.Clear();
            _subItemList = entries;
            foreach (var entry in entries)
                All.Add(entry);
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<VM>> LoadAsync()
        {
            if (!_isLoaded) //NotLoaded
            {
                _isLoaded = true;
                All.Clear();
                _subItemList = await _loadSubEntryFunc();
                foreach (VM item in _subItemList)
                    All.Add(item);
            }
            return _subItemList;
        }        

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }

        public void SetEntries(params VM[] viewModels)
        {
            foreach (var vm in viewModels)
                All.Add(vm);
            _subItemList = viewModels.ToList();
            _isExpanded = true;
        }

        #endregion

        #region Data

        private bool _isLoaded = false;
        private bool _isExpanded = false;
        private IEnumerable<VM> _subItemList;
        private Func<Task<IEnumerable<VM>>> _loadSubEntryFunc;
        private ObservableCollection<VM> _subItems = new ObservableCollection<VM>();
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value) LoadAsync();
                _isExpanded = value; 
                NotifyPropertyChanged("IsExpanded");
            }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; NotifyPropertyChanged("IsLoaded"); }
        }


        public ObservableCollection<VM> All { get { return _subItems; } }        

        #endregion


    }

}
