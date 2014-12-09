using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.Models.Bookmark;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    public class AddBookmarksViewModel : Screen, IHandle<DirectoryChangedEvent>
    {
        #region fields

        private IEntryModel _currentDirectory;
        private IEntryModel _currentBookmarkDirectory;
        private bool _isVisible;
        private string _bookmarkLabel;
        private bool _isBookmarkEnabled;
        private BookmarkModel _lastAddedLink;

        #endregion

        #region constructors

        public AddBookmarksViewModel(BookmarkProfile bProfile, IWindowManager windowManager, IEventAggregator events)
        {
            Profile = bProfile;
            events.Subscribe(this);

            AddBookmarkCommand = new RelayCommand(e => AddBookmark());
            CurrentBookmarkDirectory = Profile.RootModel;
        }

        #endregion

        #region events

        #endregion

        #region properties

        public RelayCommand AddBookmarkCommand { get; private set; }
        public BookmarkProfile Profile { get; set; }
        public bool IsVisible { get { return _isVisible; } set { setIsVisible(value); } }

        public bool IsBookmarkEnabled { get { return _isBookmarkEnabled; } set { _isBookmarkEnabled = value; NotifyOfPropertyChange(() => IsBookmarkEnabled); } }


        public ICommandManager Commands { get; private set; }

        public IEntryModel CurrentDirectory
        {
            get { return _currentDirectory; }
            set
            {
                _currentDirectory = value;
                NotifyOfPropertyChange(() => CurrentDirectory);
            }
        }

        public IEntryModel CurrentBookmarkDirectory
        {
            get { return _currentBookmarkDirectory; }
            set
            {
                _currentBookmarkDirectory = value;
                NotifyOfPropertyChange(() => CurrentBookmarkDirectory);
            }
        }



        public string BookmarkLabel
        {
            get { return _bookmarkLabel; }
            set
            {
                _bookmarkLabel = value;
                NotifyOfPropertyChange(() => BookmarkLabel);
            }
        }


        #endregion

        #region methods

        public void Handle(DirectoryChangedEvent message)
        {
            _lastAddedLink = null;
            CurrentDirectory = message.NewModel;
            if (CurrentDirectory != null)
            {
                BookmarkLabel = CurrentDirectory.Label;
                IsBookmarkEnabled = !(CurrentDirectory is BookmarkModel);
            }
            else IsBookmarkEnabled = false;
        }

        private void setIsVisible(bool value)
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);

                if (value)
                    AddBookmark();
            }
        }

        public async Task AddBookmark(string label = null)
        {
            if (CurrentBookmarkDirectory == null)
                CurrentBookmarkDirectory = Profile.RootModel;

            if (CurrentDirectory != null)
            {
                if (label == null)
                {
                    var allBookmarkLink = await Profile.ListRecursiveAsync(Profile.RootModel, CancellationToken.None,
                       em => !(em as BookmarkModel).IsDirectory && (em as BookmarkModel).LinkPath == CurrentDirectory.FullPath,
                       em => (em as BookmarkModel).IsDirectory, false);
                    if (allBookmarkLink.Count() > 0)
                    {
                        _lastAddedLink = (allBookmarkLink.First() as BookmarkModel);
                        CurrentBookmarkDirectory = _lastAddedLink.Parent;
                        BookmarkLabel = _lastAddedLink.Label;
                    }
                    else
                        _lastAddedLink = (CurrentBookmarkDirectory as BookmarkModel)
                            .AddLink(label ?? CurrentDirectory.Label, CurrentDirectory.FullPath);
                }
                else _lastAddedLink = (CurrentBookmarkDirectory as BookmarkModel)
                            .AddLink(label ?? CurrentDirectory.Label, CurrentDirectory.FullPath);
            }
        }

        public void UpdateBookmark()
        {
            if (_lastAddedLink != null)
            {
                RemoveBookmark();
                AddBookmark(BookmarkLabel);
            }
        }

        public void RemoveBookmark()
        {
            if (_lastAddedLink != null)
                Profile.RootModel.Remove(_lastAddedLink.Label);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        #endregion


    }
}
