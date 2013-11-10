﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{

    public class ExplorerViewModel : Screen, IExplorerViewModel, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModel(IEventAggregator events, params IEntryViewModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;

            FileListModel = new FileListViewModel(events, true);
            DirectoryTreeModel = new DirectoryTreeViewModel(events, rootModels);
            StatusbarModel = new StatusbarViewModel(this, events);
        
            events.Subscribe(this);

        }

        #endregion

        #region Methods


        public void Go(string gotoPath)
        {
            foreach (var evm in _rootModels)
            {
                var model = evm.EntryModel.Profile.ParseAsync(gotoPath).Result;
                if (model != null)
                {
                    DirectoryTreeModel.Select(model);
                    return;
                }
            }
        }

        public void ChangeView(string viewMode)
        {
            FileListModel.ViewMode = viewMode;
        }

        public void Handle(SelectionChangedEvent message)
        {
            //if (message.Sender.Equals(FileListModel)) //From file list.
            //    SelectionCount = message.SelectedViewModels.Count();
            if (message.Sender.Equals(DirectoryTreeModel))
            {
                var selectedDirectory = message.SelectedModels.First();
                FileListModel.LoadAsync(selectedDirectory, null);
            }

        }


        #endregion

        #region Data

        private IEntryViewModel[] _rootModels;
        private IEventAggregator _events;

        #endregion

        #region Public Properties
        
        public IDirectoryTreeViewModel DirectoryTreeModel { get; private set; }
        public IFileListViewModel FileListModel { get; private set; }
        public IStatusbarViewModel StatusbarModel { get; private set; }
        

        #endregion
    }
}
