﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels.Helpers
{
    public class EntriesHelper<VM> : NotifyPropertyChanged, IEntriesHelper<VM>
    {
        #region Constructor

        public EntriesHelper(Func<Task<IEnumerable<VM>>> loadSubEntryFunc)
        {
            _loadSubEntryFunc = loadSubEntryFunc;

            All = new FastObservableCollection<VM>();
            All.Add(default(VM));
        }

        public EntriesHelper(params VM[] entries)
        {
            _isLoaded = true;
            All = new FastObservableCollection<VM>();
            _subItemList = entries;
            (All as FastObservableCollection<VM>).AddItems(entries);
            //foreach (var entry in entries)
            //    All.Add(entry);
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<VM>> LoadAsync(Func<Task<IEnumerable<VM>>> loadFunc)
        {
            _loadSubEntryFunc = loadFunc;
            return await LoadAsync(true);
        }

        public async Task<IEnumerable<VM>> LoadAsync(bool force = false)
        {
            using (var releaser = await loadingLock.LockAsync())
            {
                if (!_isLoaded || force) //NotLoaded
                {
                    _isLoaded = true;    
                    All.Clear();                                        
                    await _loadSubEntryFunc().ContinueWith(prevTask =>
                        {
                            _subItemList = prevTask.Result.ToList();
                            //bool uiThread = System.Threading.Thread.CurrentThread == System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread;
                            (All as FastObservableCollection<VM>).AddItems(_subItemList.ToList());
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    if (EntriesChanged != null)
                        EntriesChanged(this, EventArgs.Empty);
                }
            }
            return _subItemList;
        }

        public void SetEntries(params VM[] viewModels)
        {
            All.Clear();
            foreach (var vm in viewModels)
                All.Add(vm);
            _subItemList = viewModels.ToList();
            if (EntriesChanged != null)
                EntriesChanged(this, EventArgs.Empty);
            _isExpanded = true;
        }

        #endregion

        #region Data

        private readonly AsyncLock loadingLock = new AsyncLock();
        //private bool _isLoading = false;
        private bool _isLoaded = false;
        private bool _isExpanded = false;
        private IEnumerable<VM> _subItemList;
        private Func<Task<IEnumerable<VM>>> _loadSubEntryFunc;
        private ObservableCollection<VM> _subItems;

        #endregion

        #region Public Properties

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value && !_isExpanded) LoadAsync();
                _isExpanded = value;
                NotifyOfPropertyChanged(() => IsExpanded);
            }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; NotifyOfPropertyChanged(() => IsLoaded); }
        }

        public event EventHandler EntriesChanged;

        public IEnumerable<VM> AllNonBindable { get { return _subItemList; } }

        public ObservableCollection<VM> All { get { return _subItems; } private set { _subItems = value; } }

        #endregion





    }

}
