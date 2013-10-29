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
using FileExplorer.ViewModels;

namespace TestApp.WPF
{
    [Export(typeof(IScreen))]
    public class AppViewModel : Screen, IHandle<SelectionChangedEvent>
    {
        static string rootPath = @"C:\";
        static string lookupPath = @"C:\Temp\COFE3\DB";

        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events)
        {
            

            FileListModel = new FileListViewModel(events,  new FileSystemInfoProfile());
            IProfile profile = new FileSystemInfoProfile();
            DirectoryTreeModel = new DirectoryTreeViewModel(events,
                EntryViewModel.FromEntryModel(profile, profile.ParseAsync(rootPath).Result));

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

        public IEnumerable<IResult> Load()
        {
            IProfile profile = new FileSystemInfoProfile();
            var parentModel = profile.ParseAsync(rootPath).Result;
            return FileListModel.Load(parentModel, null);
        }

        public void Go()
        {
            IProfile profile = new FileSystemInfoProfile();
            var model = profile.ParseAsync(this._gotoPath).Result;
            DirectoryTreeModel.Select(model);
        }
     
        public void ChangeView(string viewMode)
        {
            FileListModel.ViewMode = viewMode;
        }

        public void Handle(SelectionChangedEvent message)
        {
            if (message.Sender.Equals(FileListModel)) //From file list.
                SelectionCount = message.SelectedViewModels.Count();
            if (message.Sender.Equals(DirectoryTreeModel))
            {
                FileListModel.Load(message.SelectedModels.First(), null).ExecuteAsync();
            }
        }

        

        #endregion

        #region Data
        private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };
        private int _selectionCount = 0;
        private string _gotoPath = lookupPath;
        #endregion

        #region Public Properties

        
        public IEventAggregator Events { get; private set; }
        public DirectoryTreeViewModel DirectoryTreeModel { get; private set; }
        public FileListViewModel FileListModel { get; private set; }
        public List<string> ViewModes { get { return _viewModes; } }
        public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }
        public string GotoPath { get { return _gotoPath; } set { _gotoPath = value; NotifyOfPropertyChange(() => GotoPath); } }

        #endregion



    }
}
