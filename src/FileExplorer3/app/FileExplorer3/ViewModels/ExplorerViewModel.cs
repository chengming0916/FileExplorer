﻿using System;
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

            WindowTitleMask = "{0}";
            //Toolbar = new ToolbarViewModel(events);
            Breadcrumb = new BreadcrumbViewModel(_internalEvents);
            FileList = new FileListViewModel(_internalEvents);
            DirectoryTree = new DirectoryTreeViewModel(_internalEvents);
            Statusbar = new StatusbarViewModel(_internalEvents);
            Navigation = new NavigationViewModel(_internalEvents);

            Commands = new ExplorerCommandManager(this, events, FileList, DirectoryTree, Navigation);
            setRootModels(_rootModels);

            if (_events != null)
                _events.Subscribe(this);
            _internalEvents.Subscribe(this);
        }


        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var uiEle = view as System.Windows.UIElement;
            this.Commands.RegisterCommand(uiEle, ScriptBindingScope.Explorer);
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

        public string WindowTitleMask { get; set; }

        public IEntryModel[] RootModels { get { return _rootModels; } set { setRootModels(value); } }

        public ICommandManager Commands { get; private set; }

        public IBreadcrumbViewModel Breadcrumb { get; private set; }
        public IDirectoryTreeViewModel DirectoryTree { get; private set; }
        public IFileListViewModel FileList { get; private set; }
        public IStatusbarViewModel Statusbar { get; private set; }
        public INavigationViewModel Navigation { get; private set; }
        public IToolbarViewModel Toolbar { get; private set; }

        #endregion


    }
}
