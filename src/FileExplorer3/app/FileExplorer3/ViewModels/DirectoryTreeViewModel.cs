using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public class DirectoryTreeViewModel : Screen, IDirectoryTreeViewModel
    {
        #region Cosntructor

        public DirectoryTreeViewModel(IEventAggregator events, params IEntryModel[] rootModels)
        {
            _events = events;
            _rootViewModel = new BindableCollection<IDirectoryNodeViewModel>(rootModels
                .Select(r => new DirectoryNodeViewModel(events, this, r, null)));
        }

        #endregion

        #region Methods

        public virtual void NotifySelected(DirectoryNodeViewModel node)
        {
            if (SelectedViewModel != null)
                SelectedViewModel.IsSelected = false;

            if (_selectingEntry == null || !(_selectingEntry.Equals(node.CurrentDirectory.EntryModel)))
            {
                _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { node.CurrentDirectory }));
            }
            _selectingEntry = null;
            SelectedViewModel = node;
        }

        protected virtual IDirectoryNodeBroadcastHandler[] getBroadcastHandlers(IEntryModel model)
        {
            return new IDirectoryNodeBroadcastHandler[] {
                BroadcastSubEntry.FirstMatchedOnly(model, (nvm,hr) => hr == HierarchicalResult.Child ),
                        UpdateIsSelected.Instance, UpdateIsExpanded.Instance
            };
        }

        public virtual async Task SelectAsync(IEntryModel model)
        {
            if (model != null || _selectingEntry == null || !_selectingEntry.Equals(model))
            {
                _selectingEntry = model;
                foreach (var sub in Subdirectories)
                    await sub.BroadcastSelectAsync(this, model, getBroadcastHandlers(model));
            }
        }

        #endregion

        #region Data

        IDirectoryNodeViewModel _selectedViewModel = null;
        IEntryModel _selectingEntry = null, _currentBroadcast = null;
        IEventAggregator _events;
        IObservableCollection<IDirectoryNodeViewModel> _rootViewModel;

        #endregion

        #region Public Properties

        public IDirectoryNodeViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            protected set
            {
                _selectedViewModel = value;
                NotifyOfPropertyChange(() => SelectedViewModel);
                NotifyOfPropertyChange(() => SelectedEntry);
            }
        }

        public IEntryModel CurrentBroadcast
        {
            get { return _currentBroadcast; }
        }

        public IEntryModel SelectingEntry { get { return _selectingEntry; } set { _selectingEntry = value; } }

        public IEntryModel SelectedEntry
        {
            get { return _selectedViewModel == null ? null : _selectedViewModel.CurrentDirectory.EntryModel; }
            set
            {
               
                    SelectAsync(value);
            }
        }

        public virtual IObservableCollection<IDirectoryNodeViewModel> Subdirectories
        {
            get { return _rootViewModel; }
            protected set { _rootViewModel = value; NotifyOfPropertyChange(() => Subdirectories); }
        }

        #endregion




    }
}
