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
    
    public abstract class ExplorerViewModelBase : Screen, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModelBase(IEventAggregator events, params IEntryViewModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;

            FileListModel = new FileListViewModel(events);
            DirectoryTreeModel = new DirectoryTreeViewModel(events, rootModels);

            FileListModel.ColumnList = new ColumnInfo[] 
            {
                ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", 200),   
                ColumnInfo.FromBindings("Description", "EntryModel.Description", "", 200),
                ColumnInfo.FromBindings("FSI.Attributes", "EntryModel.Attributes", "", 200)   
            };

            FileListModel.ColumnFilters = new ColumnFilter[]
            {
                ColumnFilter.CreateNew("0 - 9", "EntryModel.Label", e => Regex.Match(e.Label, "^[0-9]").Success),
                ColumnFilter.CreateNew("A - H", "EntryModel.Label", e => Regex.Match(e.Label, "^[A-Ha-h]").Success),
                ColumnFilter.CreateNew("I - P", "EntryModel.Label", e => Regex.Match(e.Label, "^[I-Pi-i]").Success),
                ColumnFilter.CreateNew("Q - Z", "EntryModel.Label", e => Regex.Match(e.Label, "^[Q-Zq-z]").Success),
                ColumnFilter.CreateNew("The rest", "EntryModel.Label", e => Regex.Match(e.Label, "^[^A-Za-z0-9]").Success),
                ColumnFilter.CreateNew("Directories", "EntryModel.Description", e => e.IsDirectory),
                ColumnFilter.CreateNew("Files", "EntryModel.Description", e => !e.IsDirectory),
            };
            events.Subscribe(this);

        }

        #endregion

        #region Methods


        public void Go()
        {
            foreach (var evm in _rootModels)
            {
                var model = evm.Profile.ParseAsync(_gotoPath).Result;
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
                FileListModel.Load(message.SelectedModels.First(), null).ExecuteAsync();
            }
        }


        #endregion

        #region Data

        private IEntryViewModel[] _rootModels;
        private IEventAggregator _events;
        private string _gotoPath;

        #endregion

        #region Public Properties
        
        public DirectoryTreeViewModel DirectoryTreeModel { get; private set; }
        public FileListViewModel FileListModel { get; private set; }
        public string GotoPath { get { return _gotoPath; } set { _gotoPath = value; NotifyOfPropertyChange(() => GotoPath); } }

        #endregion
    }
}
