using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FileExplorer.WPF.ViewModels
{

    public class TabbedExplorerViewModel : Conductor<IScreen>.Collection.OneActive,
        ITabbedExplorerViewModel, ISupportDragHelper, IHandle<RootChangedEvent>
    {


        #region Constructors

        public TabbedExplorerViewModel()
        {
            DragHelper = new TabControlDragHelper<IExplorerViewModel>(this);            
        }

        #endregion

        #region Methods

        private void setInitializer(IExplorerInitializer value)
        {
            if (_initializer != null)
                _initializer.Events.Unsubscribe(this);

            _initializer = value.Clone();
            value.Events.Subscribe(this);
            Commands = new TabbedExplorerCommandManager(this, value.Events);           
        }

        public IExplorerViewModel OpenTab(IEntryModel model = null)
        {
            var initializer = _initializer.Clone();
            if (model != null)
                (initializer as ExplorerInitializer).Initializers.Add(ExplorerInitializers.StartupDirectory(model));
            ExplorerViewModel expvm = new ExplorerViewModel(initializer);
            expvm.DropHelper = new TabDropHelper<IExplorerViewModel>(expvm, this);

            //expvm.FileList.Commands.CommandDictionary.CloseTab =
            //    UIScriptCommands.TabExplorerCloseTab("{TabbedExplorer}", "{Explorer}");
            //    //ScriptCommands.ReassignToParameter("{Explorer}", TabbedExplorer.CloseTab(this));
            //expvm.FileList.Commands.CommandDictionary.OpenTab =
            //    ScriptCommands.Assign("{TabbedExplorer}", this,false, 
            //    FileList.IfSelection(evm => evm.Count() >= 1,
            //        FileList.AssignSelectionToParameter(
            //        UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", "{Parameter}", null)), ResultCommand.NoError));
            //expvm.DirectoryTree.Commands.CommandDictionary.OpenTab =
            //    ScriptCommands.Assign("{TabbedExplorer}", this,false,
            //        DirectoryTree.AssignSelectionToParameter(
            //        UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", "{Parameter}", null)));

            ActivateItem(expvm);

            return expvm;
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

        public void Handle(RootChangedEvent message)
        {
            switch (message.ChangeType)
            {
                case ChangeType.Changed:
                    _initializer.RootModels = message.AppliedRootDirectories;
                    break;
                case ChangeType.Created:
                    List<IEntryModel> rootModels = _initializer.RootModels.ToList();
                    rootModels.AddRange(message.AppliedRootDirectories);
                    _initializer.RootModels = rootModels.ToArray();
                    break;
                case ChangeType.Deleted:
                    List<IEntryModel> rootModels2 = _initializer.RootModels.ToList();
                    foreach (var d in message.AppliedRootDirectories)
                        if (rootModels2.Contains(d))
                            rootModels2.Remove(d);
                    _initializer.RootModels = rootModels2.ToArray();
                    break;
            }
        }


        #endregion

        #region Data

        //private ObservableCollection<ITabItemViewModel> _tabs;
        //private ITabItemViewModel _selectedTab;
        private IExplorerInitializer _initializer;

        #endregion

        #region Public Properties

        public IExplorerInitializer Initializer
        {
            get { return _initializer; }
            set { setInitializer(value); }
        }        

        //public ObservableCollection<ITabItemViewModel> Tabs { get { return _tabs; } }
        public ICommandManager Commands { get; private set; }

        public ISupportDrag DragHelper { get; private set; }
        //public ITabItemViewModel SelectedTab { get { return _selectedTab; } 
        //    set { _selectedTab = value; NotifyOfPropertyChange(() => SelectedTab); } }

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
