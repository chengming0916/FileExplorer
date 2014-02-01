using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class ExplorerViewModel : Screen, IExplorerViewModel,
        IHandle<DirectoryChangedEvent>,
        IHandle<EntryChangedEvent>
    {
        #region Cosntructor

        public ExplorerViewModel(IEventAggregator events, IWindowManager windowManager, params IEntryModel[] rootModels)
        {
            _events = events;
            _rootModels = rootModels;
            _windowManager = windowManager;


            //Toolbar = new ToolbarViewModel(events);
            Breadcrumb = new BreadcrumbViewModel(_internalEvents);
            FileList = new FileListViewModel(_internalEvents);
            DirectoryTree = new DirectoryTreeViewModel(_internalEvents);
            Statusbar = new StatusbarViewModel(_internalEvents);
            Navigation = new NavigationViewModel(_internalEvents);



            setRootModels(_rootModels);

            _events.Subscribe(this);
            _internalEvents.Subscribe(this);
        }


        #endregion

        #region Methods

        private IEnumerable<IScriptCommandBinding> getExportedCommands()
        {
            return FileList.Commands.ExportedCommandBindings
                .Union(Navigation.ExportedCommandBindings)
                .Union(DirectoryTree.ExportedCommandBindings);
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
            _rootProfiles = rootModels.Select(m => m.Profile).Distinct().ToArray();

            Breadcrumb.Profiles = _rootProfiles; Breadcrumb.RootModels = rootModels;
            DirectoryTree.Profiles = _rootProfiles; DirectoryTree.RootModels = rootModels;
            FileList.Profiles = _rootProfiles;
        }

        public async Task BroascastAsync(EntryChangedEvent message)
        {
            try
            {
                switch (message.ChangeType)
                {
                    case ChangeType.Deleted:
                        string path = message.ParseName.Contains('\\') ?
                            PathHelper.Disk.GetDirectoryName(message.ParseName) :
                            PathHelper.Web.GetDirectoryName(message.ParseName);

                        IEntryModel affectedParentEntry = await _rootProfiles.ParseAsync(path);
                        if (affectedParentEntry != null)
                        {
                            await DirectoryTree.Selection.AsRoot().BroascastAsync(affectedParentEntry);
                            await Breadcrumb.Selection.AsRoot().BroascastAsync(affectedParentEntry);
                            if (FileList.CurrentDirectory.Equals(affectedParentEntry))
                                await FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                        }
                        break;                   
                    default:
                        IEntryModel affectedEntry = await _rootProfiles.ParseAsync(message.ParseName);
                        if (affectedEntry != null)
                        {
                            await DirectoryTree.Selection.AsRoot().BroascastAsync(affectedEntry.Parent);
                            await Breadcrumb.Selection.AsRoot().BroascastAsync(affectedEntry.Parent);
                            if (FileList.CurrentDirectory.Equals(affectedEntry) || FileList.CurrentDirectory.Equals(affectedEntry.Parent))
                                await FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }

        public void Handle(DirectoryChangedEvent message)
        {
            this.DisplayName = message.NewModel.Label;
        }

        public void Handle(EntryChangedEvent message)
        {
            BroascastAsync(message);
        }

        #endregion

        #region Data

        private IEntryModel[] _rootModels;
        private IEventAggregator _events;
        private IEventAggregator _internalEvents = new EventAggregator();
        protected IWindowManager _windowManager = new WindowManager();
        private IProfile[] _rootProfiles = new IProfile[] { };

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
