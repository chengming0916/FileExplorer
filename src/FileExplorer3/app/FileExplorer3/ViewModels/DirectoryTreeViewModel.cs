using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{





    public class DirectoryTreeViewModel : ViewAware, IDirectoryTreeViewModel,
        IHandle<DirectoryChangedEvent>, ISupportDragHelper
    {
        #region Cosntructor

        #region DirectoryTreeDragHelper
        private class DirectoryTreeDragHelper : TreeDragHelper<IEntryModel>
        {
            public DirectoryTreeDragHelper(IEntriesHelper<IDirectoryNodeViewModel> entries,
                ITreeSelector<IDirectoryNodeViewModel, IEntryModel> selection)
                : base(
                () => new[] { selection.RootSelector.SelectedViewModel },
                ems => ems.First().Profile.DragDrop.QueryDrag(ems),
                ems => AsyncUtils.RunSync(() => ems.First().Profile.DragDrop.GetDataObject(ems)),
                (ems, da, eff) => ems.First().Profile.DragDrop.OnDragCompleted(ems, da, eff)
                , d => (d as IEntryViewModel).EntryModel)
            { }
        }



        #endregion

        public DirectoryTreeViewModel(IEventAggregator events)
        {
            _events = events;

            if (events != null)
                events.Subscribe(this);

            Entries = new EntriesHelper<IDirectoryNodeViewModel>();
            var selection = new TreeRootSelector<IDirectoryNodeViewModel, IEntryModel>(Entries) 
            { Comparers = new[] { PathComparer.LocalDefault } };
            selection.SelectionChanged += (o, e) =>
            {
                BroadcastDirectoryChanged(EntryViewModel.FromEntryModel(selection.SelectedValue));
            };
            Selection = selection;

            Commands = new DirectoryTreeCommandManager(this, events);

            DragHelper = new DirectoryTreeDragHelper(Entries, Selection);
        }

        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var uiEle = view as System.Windows.UIElement;
            this.Commands.RegisterCommand(uiEle, ScriptBindingScope.Local);
        }

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

        void setRootModels(IEntryModel[] rootModels)
        {
            DirectoryNodeViewModel[] rootViewModels = rootModels
               .Select(r => new DirectoryNodeViewModel(_events, this, r, null)).ToArray();
            Entries.SetEntries(rootViewModels);            
        }

        public void ExpandRootEntryModels()
        {
            foreach (var rvm in Entries.AllNonBindable)
                rvm.Entries.IsExpanded = true;
        }

        private void setProfiles(IProfile[] profiles)
        {
            _profiles = profiles;
            if (profiles != null && profiles.Length > 0)
            {
                (Selection as ITreeRootSelector<IDirectoryNodeViewModel, IEntryModel>)
                    .Comparers = profiles.Select(p => p.HierarchyComparer);
            }
        }     

        #endregion

        #region Data


        private IEnumerable<IProfile> _profiles = new List<IProfile>();
        private IEventAggregator _events;
        private bool _enableDrag = true, _enableDrop = true;

        #endregion

        #region Public Properties

        public ICommandManager Commands { get; private set; }

        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }

        public IEntryModel[] RootModels { set { setRootModels(value); } }
        public IProfile[] Profiles { set { setProfiles(value); } }

        public ITreeSelector<IDirectoryNodeViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }
        public ISupportDrag DragHelper { get; set; }

        #endregion






    }
}
