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
        ITabbedExplorerViewModel, ISupportDragHelper
    {


        #region Constructors

        public TabbedExplorerViewModel(IExplorerInitializer initializer, 
            IConfiguration defaultConfig,
            params IConfiguration[] otherConfigs)
        {
            _defaultConfig = defaultConfig;
            _otherConfigs = otherConfigs;
            _initializer = initializer.Clone();
            Commands = new TabbedExplorerCommandManager(this, initializer.Events);
            DragHelper = new TabControlDragHelper<IExplorerViewModel>(this);
        }

        #endregion

        #region Methods

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

        #endregion

        #region Data

        private IExplorerInitializer _initializer;
        private IConfiguration _defaultConfig;
        private IConfiguration[] _otherConfigs;

        #endregion

        #region Public Properties

        public ICommandManager Commands { get; private set; }

        public ISupportDrag DragHelper { get; private set; }

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
