using System;
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
    
    public class ExplorerViewModel : Screen, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModel(IEventAggregator events, params IEntryViewModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;

            FileListModel = new FileListViewModel(events);
            DirectoryTreeModel = new DirectoryTreeViewModel(events, rootModels);

        
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
                FileListModel.Load(selectedDirectory, null).ExecuteAsync();
            }
            else
                if (message.Sender.Equals(FileListModel))
                {
                    SelectionCount = message.SelectedViewModels.Count();
                }
        }


        #endregion

        #region Data

        private IEntryViewModel[] _rootModels;
        private IEventAggregator _events;
        
        private int _selectionCount = 0;

        #endregion

        #region Public Properties
        
        public DirectoryTreeViewModel DirectoryTreeModel { get; private set; }
        public FileListViewModel FileListModel { get; private set; }

        public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }

        #endregion
    }
}
