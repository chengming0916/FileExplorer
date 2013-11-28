using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels
{
    public class NavigationViewModel : PropertyChangedBase, INavigationViewModel,
        IHandle<SelectionChangedEvent>
    {
        #region Constructor

        public NavigationViewModel(IEventAggregator events)
        {
            Events = events;
            if (events != null)
                events.Subscribe(this);
        }

        #endregion

        #region Methods       

        public void Handle(SelectionChangedEvent message)
        {
            if (message.Sender is IDirectoryTreeViewModel || message.Sender is IBreadcrumbViewModel)
            {
                //Is directory change                
                var destFolder = message.SelectedViewModels.First();
                if (!_updatingNavPosition && (_currentFolder == null || !(_currentFolder.Equals(destFolder))))
                {
                    Add(destFolder);
                    Events.Publish(new DirectoryChangedEvent(this, destFolder, _currentFolder));
                    _currentFolder = destFolder;
                }
            }
        }

        public void Add(IEntryViewModel item)
        {
            if (NavigationPosition != -1)
                for (int i = 0; i < NavigationPosition; i++)
                    NavigationHistory.RemoveAt(0);
            while (NavigationHistory.Count > 10)
                NavigationHistory.RemoveAt(NavigationHistory.Count - 1);

            if (NavigationHistory.IndexOf(item) != -1)
                NavigationHistory.Remove(item);
            NavigationHistory.Insert(0, item);
            NavigationPosition = 0;

            CanGoNext = NavigationPosition > 0;
            CanGoBack = NavigationPosition < NavigationHistory.Count - 1;
        }

        public void Clear()
        {
            NavigationHistory.Clear();
            NavigationPosition = -1;
        }

        internal void ChangeNavigationPosition(int newPosition)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _updatingNavPosition = true;
            try
            {
                int orgPosition = NavigationPosition;
                NavigationPosition = newPosition;
                if (newPosition != -1 && newPosition < NavigationHistory.Count)
                {
                    Events.Publish(new DirectoryChangedEvent(this,
                        NavigationHistory[newPosition], NavigationHistory[orgPosition]));
                }
            }
            finally
            {
                _updatingNavPosition = false;
                CanGoNext = newPosition > 0;
                CanGoBack = newPosition < NavigationHistory.Count - 1;

            }
        }

        public void GoBack()
        {
            ChangeNavigationPosition(NavigationPosition + 1);
        }

        public void GoNext()
        {
            ChangeNavigationPosition(NavigationPosition - 1);
        }


        #endregion

        #region Data

        bool _updatingNavPosition = false;
        int _position = 0;
        IEntryViewModel _currentFolder;
        IObservableCollection<IEntryViewModel> _navigationHistory = new BindableCollection<IEntryViewModel>();
        private bool _canGoBack;
        private bool _canGoNext;

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; set; }
        public int NavigationPosition { get { return _position; } private set { _position = value; NotifyOfPropertyChange(() => NavigationPosition); } }
        public IObservableCollection<IEntryViewModel> NavigationHistory { get { return _navigationHistory; } }
        public bool CanGoBack { get { return _canGoBack; } private set { _canGoBack = value; NotifyOfPropertyChange(() => CanGoBack); } }
        public bool CanGoNext { get { return _canGoNext; } private set { _canGoNext = value; NotifyOfPropertyChange(() => CanGoNext); } }

        #endregion



        
    }
}
