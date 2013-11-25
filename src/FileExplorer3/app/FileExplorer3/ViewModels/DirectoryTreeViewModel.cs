﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class DirectoryTreeViewModel : Screen, IDirectoryTreeViewModel
    {
        #region Cosntructor

        public DirectoryTreeViewModel(IEventAggregator events, params IEntryModel[] rootModels)
        {
            _profiles = rootModels.Select(rm => rm.Profile).Distinct();
            _events = events;

            Entries = new SubEntriesHelper<IDirectoryNodeViewModel>();
            var selection = new TreeSelectionHelper<IDirectoryNodeViewModel, IEntryModel>(Entries,
                _profiles.First().HierarchyComparer.CompareHierarchy);
            selection.SelectionChanged += (o, e) =>
            {
                BroadcastDirectoryChanged(EntryViewModel.FromEntryModel(selection.SelectedValue));
            };
            Selection = selection;

            Entries.SetEntries(rootModels
                .Select(r => new DirectoryNodeViewModel(events, this, r, null)).ToArray());   
        }

        #endregion

        #region Methods

        protected void BroadcastDirectoryChanged(IEntryViewModel viewModel)
        {
            _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { viewModel }));
        }

        public async Task SelectAsync(IEntryModel value)
        {
            await Selection.AsRoot().SelectAsync(value,
                RecrusiveSearchUntilFound<IDirectoryNodeViewModel, IEntryModel>.Instance,
                SetSelected<IDirectoryNodeViewModel, IEntryModel>.Instance,
                SetExpanded<IDirectoryNodeViewModel, IEntryModel>.Instance);

        }

        
        #endregion

        #region Data

           
        private IEnumerable<IProfile> _profiles;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        public ITreeSelector<IDirectoryNodeViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }


        #endregion




    }
}
