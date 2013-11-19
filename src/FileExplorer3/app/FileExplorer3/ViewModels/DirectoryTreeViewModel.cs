﻿using System;
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

        protected void BroadcastDirectoryChanged(IEntryViewModel viewModel)
        {
            _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { viewModel }));
        }

        public virtual void NotifySelectionChanged(IEnumerable<IDirectoryNodeViewModel> path, bool selected)
        {
            if (selected)
            {
                var selectedNode = path.Last();

                if (SelectedViewModel != null)
                    SelectedViewModel.IsSelected = false;

                if (_selectingEntry == null ||
                    !(_selectingEntry.Equals(selectedNode.CurrentDirectory.EntryModel)))
                {
                    BroadcastDirectoryChanged(selectedNode.CurrentDirectory);
                }

                foreach (var item in _prevSelectedNodes)
                    item.IsChildSelected = path.Contains(item);
                foreach (var item in path)
                    item.IsChildSelected = true;


                _selectingEntry = null;
                SelectedViewModel = selectedNode;
                
            }
            else
            {
                _prevSelectedNodes = path;
            }
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
            //if (SelectedViewModel != null)
            //{
            //    SelectedViewModel.IsSelected = false;
            //    SelectedViewModel = null;
            //}

            if (model != null || _selectingEntry == null || !_selectingEntry.Equals(model))
            {
                _selectingEntry = model;
                var handlers = getBroadcastHandlers(model);
                foreach (var sub in Subdirectories)
                    await sub.BroadcastSelectAsync(this, model, handlers);
            }
        }

        #endregion

        #region Data

        IEnumerable<IDirectoryNodeViewModel> _prevSelectedNodes = new List<IDirectoryNodeViewModel>();
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
