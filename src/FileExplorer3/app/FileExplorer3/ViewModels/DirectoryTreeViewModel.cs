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
            _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { node.CurrentDirectory }));
            SelectedViewModel = node;
        }


        public virtual void Select(IEntryModel model)
        {
            foreach (var sub in Subdirectories)
                sub.BroadcastSelectAsync(model, evm => { evm.IsSelected = true; });
        }

        #endregion

        #region Data

        IDirectoryNodeViewModel _selectedViewModel = null;
        IEntryModel _selectedEntry = null;
        IEventAggregator _events;
        protected IObservableCollection<IDirectoryNodeViewModel> _rootViewModel;

        #endregion

        #region Public Properties

        public IDirectoryNodeViewModel SelectedViewModel { 
            get { return _selectedViewModel; }
            protected set { _selectedViewModel = value; 
                NotifyOfPropertyChange(() => SelectedViewModel);
                NotifyOfPropertyChange(() => SelectedEntry);
            }
        }

        public IEntryModel SelectedEntry
        {
            get { return _selectedViewModel == null ? null : _selectedViewModel.CurrentDirectory.EntryModel; }
            set { Select(_selectedEntry); }
        }

        public virtual IObservableCollection<IDirectoryNodeViewModel> Subdirectories { get { return _rootViewModel; } }

        #endregion




    }
}
