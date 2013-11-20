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
    public class TreeEntryHelper<T> : INotifyPropertyChanged
    {
        #region Constructor

        public TreeEntryHelper(Func<Task<IList<T>>> loadSubEntryFunc)
        {
            _loadSubEntryFunc = loadSubEntryFunc;
            Subitems.Add(default(T));
        }

        #endregion

        #region Methods

        public async Task LoadAsync()
        {
            if (!_loaded) //NotLoaded
            {
                Subitems.Clear();
                foreach (T item in await _loadSubEntryFunc())
                    Subitems.Add(item);
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Data

        private bool _loaded = false;
        private bool _isExpanded = false;
        private Func<Task<IList<T>>> _loadSubEntryFunc;
        private ObservableCollection<T> _subItems = new ObservableCollection<T>();
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

        public ObservableCollection<T> Subitems { get { return _subItems; } }

        #endregion


    }

}
