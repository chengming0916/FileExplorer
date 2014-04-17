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
using Cofe.Core.Utils;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;

namespace FileExplorer.ViewModels
{
    public class ExplorerViewModel : Screen, IExplorerViewModel,
        IHandle<DirectoryChangedEvent>,
        IHandle<EntryChangedEvent>,
        IHandle<RootChangedEvent>,
        IHandle<BroadcastEvent>, 
        ISupportDropHelper
    {

       

        #region Cosntructor

        public ExplorerViewModel(IExplorerInitializer initializer)
        {
            _events = initializer.Events;
            _rootModels = initializer.RootModels;
            _windowManager = initializer.WindowManager;
            _initializer = initializer;

            WindowTitleMask = "{0}";
            DisplayName = "";
            //Toolbar = new ToolbarViewModel(events);
            Breadcrumb = new BreadcrumbViewModel(_internalEvents);
            Statusbar = new StatusbarViewModel(_internalEvents);
            Sidebar = new SidebarViewModel(_internalEvents);
            FileList = new FileListViewModel(_internalEvents, Sidebar);
            DirectoryTree = new DirectoryTreeViewModel(_windowManager, _internalEvents);
            Navigation = new NavigationViewModel(_internalEvents);
            DropHelper = NullSupportDrop.Instance;

            Commands = new ExplorerCommandManager(this, _events, FileList, DirectoryTree, Navigation);
            setRootModels(_rootModels);

            if (_events != null)
                _events.Subscribe(this);
            _internalEvents.Subscribe(this);

        }

        public ExplorerViewModel(IEventAggregator events, IWindowManager windowManager, params IEntryModel[] rootModels)
            : this(new ExplorerInitializer(windowManager, events, rootModels))
        {

        }


        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            if (!_attachedView)
            {
                var uiEle = view as System.Windows.UIElement;
                this.Commands.RegisterCommand(uiEle, ScriptBindingScope.Explorer);

                _initializer.Initializers.Add(ExplorerInitializers.StartupDirectory(null));
                _initializer.Initializers.EnsureOneStartupDirectoryOnly();
                _initializer.Initializers.InitalizeAsync(this);

                _attachedView = true;
            }

            //if (_rootModels != null && _rootModels.Length > 0)
            //    uiEle.Dispatcher.BeginInvoke((System.Action)(() =>

            //Commands.Execute(
            //    new IScriptCommand[] { 
            //        Explorer.GoTo(_rootModels.First())
            //    })), System.Windows.Threading.DispatcherPriority.Background
            //    );


        }

        public async Task GoAsync(IEntryModel entryModel)
        {
            entryModel = entryModel ?? RootModels.FirstOrDefault();
            if (entryModel != null)
            {
                await DirectoryTree.SelectAsync(entryModel);
                FileList.CurrentDirectory = entryModel;
                await Breadcrumb.Selection.AsRoot().SelectAsync(entryModel);
            }

        }

        public async Task GoAsync(string gotoPath)
        {
            foreach (var evm in _rootModels)
            {
                var model = await evm.Profile.ParseAsync(gotoPath);
                if (model != null)
                {
                    await GoAsync(model);
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
            if (rootModels == null)
                return;
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
                string[] paths = message.ParseNames.Select(p => p.Contains('\\') ?
                        PathHelper.Disk.GetDirectoryName(p) : PathHelper.Web.GetDirectoryName(p))
                        .Distinct().ToArray();

                foreach (var path in paths)
                {
                    IEntryModel affectedParentEntry = await _rootProfiles.ParseAsync(path);
                    if (affectedParentEntry != null)
                    {
                        await DirectoryTree.Selection.AsRoot().BroascastAsync(affectedParentEntry);
                        await Breadcrumb.Selection.AsRoot().BroascastAsync(affectedParentEntry);
                        if (FileList.CurrentDirectory.Equals(affectedParentEntry))
                            await FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }

        public void Handle(DirectoryChangedEvent message)
        {
            this.DisplayName = String.Format(WindowTitleMask, message.NewModel.Label);
            _currentDirectoryViewModel = EntryViewModel.FromEntryModel(message.NewModel);
            NotifyOfPropertyChange("CurrentDirectory");
        }

        public void Handle(EntryChangedEvent message)
        {
            BroascastAsync(message);
        }

        public void Handle(BroadcastEvent message)
        {
            if (message.EventToBroadcast != null)
            {
                if (message.EventToBroadcast is ExplorerEvent)
                    (message.EventToBroadcast as ExplorerEvent).Sender = this;
                _events.Publish(message.EventToBroadcast);
            }
        }

        public void Handle(RootChangedEvent message)
        {
            Queue<IScriptCommand> cmds = new Queue<IScriptCommand>();

            cmds.Enqueue(Explorer.ChangeRoot(message.ChangeType, message.AppliedRootDirectories));
            if (message.Sender != this)
                cmds.Enqueue(Explorer.GoTo(CurrentDirectory.EntryModel));
            else
                switch (message.ChangeType)
                {
                    case ChangeType.Created:
                    case ChangeType.Changed:
                        cmds.Enqueue(Explorer.GoTo(message.AppliedRootDirectories.First()));
                        break;
                    case ChangeType.Deleted:
                        cmds.Enqueue(Explorer.GoTo(RootModels.FirstOrDefault()));
                        break;
                }

            Commands.ExecuteAsync(cmds.ToArray());
        }

        #endregion

        #region Data

        private bool _attachedView = false;
        private IEntryModel[] _rootModels;
        private IEventAggregator _events;
        private IEventAggregator _internalEvents = new EventAggregator();
        protected IWindowManager _windowManager = new WindowManager();
        private IProfile[] _rootProfiles = new IProfile[] { };
        private IExplorerInitializer _initializer;
        private float _uiScale = 1.0f;

        private IEntryViewModel _currentDirectoryViewModel;

        #endregion

        #region Public Properties


        public IExplorerInitializer Initializer { get; private set; }

        public string WindowTitleMask { get; set; }
        public IEntryViewModel CurrentDirectory { get { return _currentDirectoryViewModel; } }

        public float UIScale { get { return _uiScale; } set { _uiScale = value; NotifyOfPropertyChange(() => UIScale); } }

        public IEntryModel[] RootModels { get { return _rootModels; } set { setRootModels(value); } }

        public ICommandManager Commands { get; private set; }

        public IBreadcrumbViewModel Breadcrumb { get; private set; }
        public IDirectoryTreeViewModel DirectoryTree { get; private set; }
        public IFileListViewModel FileList { get; private set; }
        public ISidebarViewModel Sidebar { get; private set; }
        public IStatusbarViewModel Statusbar { get; private set; }
        public INavigationViewModel Navigation { get; private set; }
        public IToolbarViewModel Toolbar { get; private set; }
        public ISupportDrop DropHelper { get; set; }
        

        #endregion
    }
}
