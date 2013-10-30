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
    public class AppViewModel : Screen//, IHandle<SelectionChangedEvent>
    {
        static string rootPath = @"C:\";
        static string lookupPath = @"C:\Temp\COFE3\DB";

        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events)
        {

            IProfile profile = new FileSystemInfoProfile();
            ExplorerModel = new ExplorerViewModel(events,
                EntryViewModel.FromEntryModel(profile.ParseAsync(rootPath).Result));


            ExplorerModel.FileListModel.ColumnList = new ColumnInfo[] 
            {
                ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", 200),   
                ColumnInfo.FromBindings("Description", "EntryModel.Description", "", 200),
                ColumnInfo.FromBindings("FSI.Attributes", "EntryModel.Attributes", "", 200)   
            };

            ExplorerModel.FileListModel.ColumnFilters = new ColumnFilter[]
            {
                ColumnFilter.CreateNew("0 - 9", "EntryModel.Label", e => Regex.Match(e.Label, "^[0-9]").Success),
                ColumnFilter.CreateNew("A - H", "EntryModel.Label", e => Regex.Match(e.Label, "^[A-Ha-h]").Success),
                ColumnFilter.CreateNew("I - P", "EntryModel.Label", e => Regex.Match(e.Label, "^[I-Pi-i]").Success),
                ColumnFilter.CreateNew("Q - Z", "EntryModel.Label", e => Regex.Match(e.Label, "^[Q-Zq-z]").Success),
                ColumnFilter.CreateNew("The rest", "EntryModel.Label", e => Regex.Match(e.Label, "^[^A-Za-z0-9]").Success),
                ColumnFilter.CreateNew("Directories", "EntryModel.Description", e => e.IsDirectory),
                ColumnFilter.CreateNew("Files", "EntryModel.Description", e => !e.IsDirectory),
            };
        }

        #endregion

        #region Methods

        //public IEnumerable<IResult> Load()
        //{
        //    IProfile profile = new FileSystemInfoProfile();
        //    var parentModel = profile.ParseAsync(rootPath).Result;
        //    return FileListModel.Load(parentModel, null);
        //}

        public void Go()
        {
            ExplorerModel.Go(GotoPath);
        }
     
        public void ChangeView(string viewMode)
        {
            ExplorerModel.FileListModel.ViewMode = viewMode;
        }
     
        

        #endregion

        #region Data
        private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };
        //private int _selectionCount = 0;
        private string _gotoPath = lookupPath;
        #endregion

        #region Public Properties

        public ExplorerViewModel ExplorerModel { get; private set; }
        
        
        public List<string> ViewModes { get { return _viewModes; } }
        //public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }
        public string GotoPath { get { return _gotoPath; } set { _gotoPath = value; NotifyOfPropertyChange(() => GotoPath); } }

        #endregion



    }
}
