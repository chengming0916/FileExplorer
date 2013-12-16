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
    public class ExplorerViewModel : ViewAware, IExplorerViewModel, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModel(IEventAggregator events, params IEntryModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;

            Breadcrumb = new BreadcrumbViewModel(_internalEvents, rootModels);
            FileList = new FileListViewModel(_internalEvents);
            DirectoryTree = new DirectoryTreeViewModel(_internalEvents, rootModels);
            Statusbar = new StatusbarViewModel(_internalEvents);
            Navigation = new NavigationViewModel(_internalEvents);            

            _internalEvents.Subscribe(this);
        }
            

        #endregion

        #region Methods


        public void Go(string gotoPath)
        {
            foreach (var evm in _rootModels)
            {
                var model = evm.Profile.ParseAsync(gotoPath).Result;
                if (model != null)
                {                    
                    DirectoryTree.SelectAsync(model);
                    FileList.SetCurrentDirectoryAsync(model);
                    Breadcrumb.Selection.AsRoot().SelectAsync(model);
                    return;
                }
            }
        }

        public void ChangeView(string viewMode)
        {
            FileList.ViewMode = viewMode;
        }

        public void Handle(SelectionChangedEvent message)
        {
           
        }


        #endregion

        #region Data

        private IEntryModel[] _rootModels;
        private IEventAggregator _events;
        private IEventAggregator _internalEvents = new EventAggregator();

        #endregion

        #region Public Properties

        public IBreadcrumbViewModel Breadcrumb { get; private set; }
        public IDirectoryTreeViewModel DirectoryTree { get; private set; }
        public IFileListViewModel FileList { get; private set; }
        public IStatusbarViewModel Statusbar { get; private set; }
        public INavigationViewModel Navigation { get; private set; }
        

        #endregion
    }
}
