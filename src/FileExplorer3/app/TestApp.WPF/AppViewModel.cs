using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.ViewModels;

namespace TestApp.WPF
{
    [Export(typeof(IScreen))]
    public class AppViewModel : Screen
    {
        #region Cosntructor

        [ImportingConstructor]
        public AppViewModel(IEventAggregator events)
        {
            FileListModel = new FileListViewModel(events);
        }

        #endregion

        #region Methods

        public IEnumerable<IResult> Load()
        {
            IProfile profile = new FileSystemInfoProfile();
            var parentModel = profile.ParseAsync(@"c:\temp").Result;
            return FileListModel.Load(new FileSystemInfoProfile(), parentModel, null);
        }

        public void ChangeView(string viewMode)
        {
            FileListModel.ViewMode = viewMode;
        }

        #endregion

        #region Data
        private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; private set; }
        public FileListViewModel FileListModel { get; private set; }
        public List<string> ViewModes { get { return _viewModes; } }

        #endregion

    }
}
