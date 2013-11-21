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
    public class ExplorerViewModel : Screen, IExplorerViewModel, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModel(IEventAggregator events, params IEntryModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;

            BreadcrumbModel = new BreadcrumbViewModel(this, events, rootModels);
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
                var model = evm.Profile.ParseAsync(gotoPath).Result;
                if (model != null)
                {
                    DirectoryTreeModel.SelectAsync(model);
                    FileListModel.LoadAsync(model, null);
                    BreadcrumbModel.Selection.SelectAsync(model);
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
                BreadcrumbModel.Selection.SelectAsync(selectedDirectory);
            }
            else if (message.Sender.Equals(BreadcrumbModel))
            {
                var selectedDirectory = message.SelectedModels.FirstOrDefault();
                FileListModel.LoadAsync(selectedDirectory, null);
                if (selectedDirectory != null)
                    DirectoryTreeModel.SelectAsync(selectedDirectory);
            }

        }


        #endregion

        #region Data

        private IEntryModel[] _rootModels;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        public IBreadcrumbViewModel BreadcrumbModel { get; private set; }
        public IDirectoryTreeViewModel DirectoryTreeModel { get; private set; }
        public IFileListViewModel FileListModel { get; private set; }
        public IStatusbarViewModel StatusbarModel { get; private set; }


        #endregion
    }
}
