using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels
{
    public class ExplorerViewModel : ViewAware, IExplorerViewModel, IHandle<SelectionChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModel(IEventAggregator events, params IEntryModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;
            
            //Toolbar = new ToolbarViewModel(events);
            Breadcrumb = new BreadcrumbViewModel(_internalEvents);
            FileList = new FileListViewModel(_internalEvents);
            DirectoryTree = new DirectoryTreeViewModel(_internalEvents);
            Statusbar = new StatusbarViewModel(_internalEvents);
            Navigation = new NavigationViewModel(_internalEvents);

            setRootModels(_rootModels);

            _internalEvents.Subscribe(this);
        }


        #endregion

        #region Methods

        private IEnumerable<IScriptCommandBinding> getExportedCommands()
        {
            return FileList.ExportedCommandBindings
                .Union(Navigation.ExportedCommandBindings);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var uiEle = view as System.Windows.UIElement;
            this.RegisterCommand(uiEle, ScriptBindingScope.Explorer);
        }

        public void Go(string gotoPath)
        {
            foreach (var evm in _rootModels)
            {
                var model = evm.Profile.ParseAsync(gotoPath).Result;
                if (model != null)
                {
                    DirectoryTree.SelectAsync(model);
                    FileList.CurrentDirectory = model;
                    Breadcrumb.Selection.AsRoot().SelectAsync(model);
                    return;
                }
            }
        }

      
        public void ChangeView(string viewMode)
        {
            FileList.ViewMode = viewMode;
        }

        private void setRootModels(IEntryModel[] rootModels)
        {
            _rootModels = rootModels;
             var rootProfiles = rootModels.Select(m => m.Profile).Distinct().ToArray();

             Breadcrumb.Profiles = rootProfiles; Breadcrumb.RootModels = rootModels;
             DirectoryTree.Profiles = rootProfiles; DirectoryTree.RootModels = rootModels;             
             FileList.Profiles = rootProfiles;
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

        public IEntryModel[] RootModels { get { return _rootModels; } set { setRootModels(value); } }

     

        public IBreadcrumbViewModel Breadcrumb { get; private set; }
        public IDirectoryTreeViewModel DirectoryTree { get; private set; }
        public IFileListViewModel FileList { get; private set; }
        public IStatusbarViewModel Statusbar { get; private set; }
        public INavigationViewModel Navigation { get; private set; }
        public IToolbarViewModel Toolbar { get; private set; }

        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get { return getExportedCommands(); } }        

        #endregion
    }
}
