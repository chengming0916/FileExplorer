using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{


 


    public class DirectoryTreeViewModel : PropertyChangedBase, IDirectoryTreeViewModel,
        IHandle<DirectoryChangedEvent>, ISupportDragHelper
    {
        #region Cosntructor

        #region DirectoryTreeDragHelper
        private class DirectoryTreeDragHelper : TreeDragHelper<IEntryModel>
        {
            public DirectoryTreeDragHelper(IEntriesHelper<IDirectoryNodeViewModel> entries,
                ITreeSelector<IDirectoryNodeViewModel, IEntryModel> selection)
                : base(
                () => new [] { selection.RootSelector.SelectedViewModel },
                ems => ems.First().Profile.DragDrop.QueryDrag(ems),
                ems => ems.First().Profile.DragDrop.GetDataObject(ems),
                (ems, da, eff) => ems.First().Profile.DragDrop.OnDragCompleted(ems, da, eff) 
                , d => (d as IEntryViewModel).EntryModel)
            { }
        }

        

        #endregion

        public DirectoryTreeViewModel(IEventAggregator events, params IEntryModel[] rootModels)
        {
            _profiles = rootModels.Select(rm => rm.Profile).Distinct();
            _events = events;

            if (events != null)
                events.Subscribe(this);

            Entries = new EntriesHelper<IDirectoryNodeViewModel>();
            var selection = new TreeRootSelector<IDirectoryNodeViewModel, IEntryModel>(Entries,
                _profiles.First().HierarchyComparer.CompareHierarchy);
            selection.SelectionChanged += (o, e) =>
            {
                BroadcastDirectoryChanged(EntryViewModel.FromEntryModel(selection.SelectedValue));
            };
            Selection = selection;

            DirectoryNodeViewModel[] rootViewModels = rootModels
                .Select(r => new DirectoryNodeViewModel(events, this, r, null)).ToArray();
            foreach (var rvm in rootViewModels)
                rvm.Entries.IsExpanded = true;
            Entries.SetEntries(rootViewModels);
            DragHelper = new DirectoryTreeDragHelper(Entries, Selection);
        }

        #endregion

        #region Methods

        protected void BroadcastDirectoryChanged(IEntryViewModel viewModel)
        {
            _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { viewModel }));
        }

        public async Task SelectAsync(IEntryModel value)
        {
            await Selection.LookupAsync(value,
                RecrusiveSearch<IDirectoryNodeViewModel, IEntryModel>.LoadSubentriesIfNotLoaded,
                SetSelected<IDirectoryNodeViewModel, IEntryModel>.WhenSelected,
                SetExpanded<IDirectoryNodeViewModel, IEntryModel>.WhenChildSelected);

        }

        public void Handle(DirectoryChangedEvent message)
        {
            if (message.NewModel != null)
            {
                SelectAsync(message.NewModel);
            }
        }

        #endregion

        #region Data


        private IEnumerable<IProfile> _profiles;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        public ITreeSelector<IDirectoryNodeViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }
        public ISupportDrag DragHelper { get; set; }

        #endregion






    }
}
