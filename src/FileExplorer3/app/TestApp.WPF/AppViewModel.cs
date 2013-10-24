using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
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
        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events)
        {
            FileListModel = new FileListViewModel(events) { Profile = new FileSystemInfoProfile() };
            FileListModel.ColumnList = new ListViewColumnInfo[] 
            {
                ListViewColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", 200),   
                ListViewColumnInfo.FromBindings("Description", "EntryModel.Description", "", 200),
                ListViewColumnInfo.FromBindings("FSI.Attributes", "EntryModel.Attributes", "", 200)   
            };
            events.Subscribe(this);
        }

        #endregion

        #region Methods

        public IEnumerable<IResult> Load()
        {
            IProfile profile = new FileSystemInfoProfile();
            var parentModel = profile.ParseAsync(@"c:\temp").Result;
            return FileListModel.Load(parentModel, null);
        }

        public void ChangeView(string viewMode)
        {
            FileListModel.ViewMode = viewMode;
        }

        public void Handle(SelectionChangedEvent message)
        {
            SelectionCount = message.SelectedViewModels.Count();
        }

        #endregion

        #region Data
        private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };
        private int _selectionCount = 0;

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; private set; }
        public FileListViewModel FileListModel { get; private set; }
        public List<string> ViewModes { get { return _viewModes; } }
        public int SelectionCount { get { return _selectionCount; } set { _selectionCount = value; NotifyOfPropertyChange(() => SelectionCount); } }

        #endregion


      
    }
}
