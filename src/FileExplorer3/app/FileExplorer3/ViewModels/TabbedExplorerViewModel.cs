using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FileExplorer.ViewModels
{

    public class TabbedExplorerViewModel : Conductor<IScreen>.Collection.OneActive,
        ITabbedExplorerViewModel, ISupportDragHelper, IHandle<RootChangedEvent>
    {


        #region Constructors

        public TabbedExplorerViewModel(IExplorerInitializer initializer,
            IConfiguration defaultConfig)
        {
            _defaultConfig = defaultConfig;
            _initializer = initializer.Clone();
            Commands = new TabbedExplorerCommandManager(this, initializer.Events);
            DragHelper = new TabControlDragHelper<IExplorerViewModel>(this);
            ConfigurationHelper = new ConfigurationHelper();
            _initializer.Events.Subscribe(this);
        }

        #endregion

        #region Methods

        public async Task LoadConfiguration()
        {
            if (_initializer.ConfigurationPath != null &&
                System.IO.File.Exists(_initializer.ConfigurationPath))
                using (var fs = System.IO.File.OpenRead(_initializer.ConfigurationPath))
                    await ConfigurationHelper.LoadAsync(fs);

            if (ConfigurationHelper.Configurations.AllNonBindable.Count() == 0)
            {
                ConfigurationHelper.Add(_defaultConfig);
                await SaveConfiguration();
            }
        }

        public async Task SaveConfiguration()
        {
            if (_initializer.ConfigurationPath != null &&
                ConfigurationHelper.Configurations.AllNonBindable.Count() >= 0)
            {
                using (var fs = System.IO.File.OpenWrite(_initializer.ConfigurationPath))
                    await ConfigurationHelper.SaveAsync(fs);
            }
        }

        public void OpenTab(IEntryModel model = null, IConfiguration configuration = null)
        {
            var initializer = _initializer.Clone();
            initializer.Initializers.Add(ExplorerInitializers.Configuration(
                configuration ?? _defaultConfig));
            if (model != null)
                initializer.Initializers.Add(ExplorerInitializers.StartupDirectory(model));
            ExplorerViewModel expvm = new ExplorerViewModel(initializer);
            expvm.DropHelper = new TabDropHelper<IExplorerViewModel>(expvm, this);

            expvm.Commands.ScriptCommands.CloseTab =
                ScriptCommands.AssignVariableToParameter("Explorer", TabbedExplorer.CloseTab(this));
            expvm.FileList.Commands.ScriptCommands.OpenTab =
                FileList.IfSelection(evm => evm.Count() >= 1,
                    ScriptCommands.RunInSequence(
                    FileList.AssignSelectionToParameter(TabbedExplorer.OpenTab(this))),
                    NullScriptCommand.Instance);
            expvm.DirectoryTree.Commands.ScriptCommands.OpenTab =
                    ScriptCommands.RunInSequence(
                    DirectoryTree.AssignSelectionToParameter(TabbedExplorer.OpenTab(this)));

            ActivateItem(expvm);
        }

        public void CloseTab(IExplorerViewModel evm)
        {
            DeactivateItem(evm, true);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var uiEle = view as System.Windows.UIElement;
            this.Commands.RegisterCommand(uiEle, ScriptBindingScope.Application);
            
            //LoadConfiguration();
            ConfigurationHelper.Add(_defaultConfig);
            ConfigurationHelper.Add(new Configuration("Test"));

            uiEle.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() => OpenTab()));

            //uiEle.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, 
            //    delegate () =>
            //    {
            //        OpenTab();
            //    });
            //OpenTab();
        }

        public int GetTabIndex(IExplorerViewModel evm)
        {
            return Items.IndexOf(evm);
        }

        public void MoveTab(int srcIdx, int targetIdx)
        {
            if (srcIdx < Items.Count())
            {
                IExplorerViewModel srcTab = Items[srcIdx] as IExplorerViewModel;
                if (srcTab != null)
                {
                    Items.RemoveAt(srcIdx);
                    Items.Insert(targetIdx, srcTab);
                }
            }
        }

        public void Handle(RootChangedEvent message)
        {
            switch (message.ChangeType)
            {
                case ChangeType.Changed :
                    _initializer.RootModels = message.AppliedRootDirectories;
                    break;
                case ChangeType.Created :
                    List<IEntryModel> rootModels = _initializer.RootModels.ToList();
                    rootModels.AddRange(message.AppliedRootDirectories);
                    _initializer.RootModels = rootModels.ToArray();
                    break;
                case ChangeType.Deleted : 
                    List<IEntryModel> rootModels2 = _initializer.RootModels.ToList();
                    foreach (var d in message.AppliedRootDirectories)
                        if (rootModels2.Contains(d))
                            rootModels2.Remove(d);
                    _initializer.RootModels = rootModels2.ToArray();
                    break;
            }
        }

        public override void NotifyOfPropertyChange(string propertyName = "")
        {
            if (propertyName == "ActiveItem")
            {
                NotifyOfPropertyChange(() => SelectedIndex); 
                NotifyOfPropertyChange(() => SelectedItem);
            }
            base.NotifyOfPropertyChange(propertyName);
        }

        #endregion

        #region Data

        private IExplorerInitializer _initializer;
        private IConfiguration _defaultConfig;

        #endregion

        #region Public Properties

        public ICommandManager Commands { get; private set; }

        public ISupportDrag DragHelper { get; private set; }
        public IConfigurationHelper ConfigurationHelper { get; private set; }

        public int SelectedIndex
        {
            get { return Items.IndexOf(ActiveItem); }
            set { ActivateItem(Items[value]); NotifyOfPropertyChange(() => SelectedIndex); NotifyOfPropertyChange(() => SelectedItem); }
        }

        public IExplorerViewModel SelectedItem
        {
            get { return ActiveItem as IExplorerViewModel; }
            set { ActivateItem(value); NotifyOfPropertyChange(() => SelectedIndex); NotifyOfPropertyChange(() => SelectedItem); }
        }

        #endregion


       
    }
}
