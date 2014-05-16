using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels.Helpers
{
    public class EntriesHelper<VM> : NotifyPropertyChanged, IEntriesHelper<VM>
    {
        #region Constructor

        public EntriesHelper(Func<bool, object, Task<IEnumerable<VM>>> loadSubEntryFunc)
        {
            _loadSubEntryFunc = loadSubEntryFunc;

            All = new FastObservableCollection<VM>();
            All.Add(default(VM));
        }

        public EntriesHelper(Func<bool, Task<IEnumerable<VM>>> loadSubEntryFunc)
            : this((b, __) => loadSubEntryFunc(b))
        {

        }

        public EntriesHelper(Func<Task<IEnumerable<VM>>> loadSubEntryFunc)
            : this(_ => loadSubEntryFunc())
        {
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

        public async Task UnloadAsync()
        {
            _lastCancellationToken.Cancel(); //Cancel previous load.                
            using (var releaser = await _loadingLock.LockAsync())
            {
                _subItemList = new List<VM>();
                All.Clear();
                _isLoaded = false;
            }
        }

        public async Task<IEnumerable<VM>> LoadAsync(bool force = false, object parameter = null)
        {
            if (_loadSubEntryFunc != null) //Ignore if contructucted using entries but not entries func
            {
                _lastCancellationToken.Cancel(); //Cancel previous load.                
                using (var releaser = await _loadingLock.LockAsync())
                {
                    _lastCancellationToken = new CancellationTokenSource();
                    if (!_isLoaded || force)
                    {
                        if (_clearBeforeLoad)
                            All.Clear();

                        IsLoading = true;
                        await _loadSubEntryFunc(_isLoaded, parameter).ContinueWith((prevTask, _) =>
                            {
                                IsLoaded = true;
                                IsLoading = false;
                                if (!prevTask.IsCanceled && !prevTask.IsFaulted)
                                {
                                    SetEntries(prevTask.Result.ToArray());
                                    _lastRefreshTimeUtc = DateTime.UtcNow;
                                }
                            }, _lastCancellationToken, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
            return _subItemList;
        }

        public void SetEntries(params VM[] viewModels)
        {
            _subItemList = viewModels.ToList();

            FastObservableCollection<VM> all = All as FastObservableCollection<VM>;
            all.SuspendCollectionChangeNotification();

            var removeItems = all.Where(vm => !viewModels.Contains(vm)).ToList();
            var addItems = viewModels.Where(vm => !all.Contains(vm)).ToList();

            foreach (var vm in removeItems)
                all.Remove(vm);
            foreach (var vm in addItems)
                all.Add(vm); 

            //all.Clear();
            //all.NotifyChanges();
            //foreach (var vm in viewModels)
            //    All.Add(vm);
            //all.AddItems(viewModels);
            all.NotifyChanges();

            if (EntriesChanged != null)
                EntriesChanged(this, EventArgs.Empty);
            //_isExpanded = true;
        }

        #endregion

        #region Data

        private CancellationTokenSource _lastCancellationToken = new CancellationTokenSource();
        private bool _clearBeforeLoad = false;
        private readonly AsyncLock _loadingLock = new AsyncLock();
        //private bool _isLoading = false;
        private bool _isLoaded = false;
        private bool _isExpanded = false;
        private bool _isLoading = false;
        private IEnumerable<VM> _subItemList = new List<VM>();
        protected Func<bool, object, Task<IEnumerable<VM>>> _loadSubEntryFunc;
        private ObservableCollection<VM> _subItems;
        private DateTime _lastRefreshTimeUtc = DateTime.MinValue;

        #endregion

        #region Public Properties

        public bool ClearBeforeLoad
        {
            get { return _clearBeforeLoad; }
            set { _clearBeforeLoad = value; }
        }

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

         public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; NotifyOfPropertyChanged(() => IsLoading); }
        }

        public DateTime LastRefreshTimeUtc { get { return _lastRefreshTimeUtc; } }

        public event EventHandler EntriesChanged;
      

        public IEnumerable<VM> AllNonBindable { get { return _subItemList; } }

        public ObservableCollection<VM> All { get { return _subItems; } private set { _subItems = value; } }

        public AsyncLock LoadingLock { get { return _loadingLock; } }

        #endregion







       
    }

}
