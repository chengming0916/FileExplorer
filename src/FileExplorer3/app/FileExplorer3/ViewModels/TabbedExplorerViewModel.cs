using Caliburn.Micro;
using Cofe.Core.Script;
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

    public class TabbedExplorerViewModel : Conductor<IScreen>.Collection.OneActive, ITabbedExplorerViewModel
    {


        #region ExplorerDrag/DropHelper

        private class TabArrangeDropHelpper : ISupportDrop
        {
            private IExplorerViewModel _evm;
            private ITabbedExplorerViewModel _tevm;
            public TabArrangeDropHelpper(ITabbedExplorerViewModel tevm, IExplorerViewModel evm)
            {
                _tevm = tevm;
                _evm = evm;
            }

            public bool IsDraggingOver
            {
                set
                {
                    if (!(_tevm.ActiveItem.Equals(_evm)))
                        _tevm.ActivateItem(_evm);
                }
            }
            public bool IsDroppable
            {
                get { return true; }
            }
            public string DropTargetLabel
            {
                get
                {
                    return _evm.CurrentDirectory == null ? "" :
                        _evm.CurrentDirectory.EntryModel.Label;
                }
            }

            public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
            {
                return QueryDropResult.None;
            }

            public IEnumerable<IDraggable> QueryDropDraggables(System.Windows.IDataObject da)
            {
                Debug.WriteLine("QueryDropDraggables " + DropTargetLabel);
                return new List<IDraggable>();
            }

            public DragDropEffects Drop(IEnumerable<IDraggable> draggables,
                IDataObject da, DragDropEffects allowedEffects)
            {
                return DragDropEffects.None;
            }
        }

        #endregion


        #region Constructors

        public TabbedExplorerViewModel(IExplorerInitializer initializer)
        {
            _initializer = initializer.Clone();
            Commands = new TabbedExplorerCommandManager(this, initializer.Events);

            ////_tabs = new ObservableCollection<ITabItemViewModel>();
        }

        #endregion

        #region Methods

        public void OpenTab(IEntryModel model = null)
        {
            var initializer = _initializer.Clone();
            if (model != null)
                initializer.Initializers.Add(ExplorerInitializers.StartupDirectory(model));
            ExplorerViewModel expvm = new ExplorerViewModel(initializer);
            expvm.DropHelper = new TabArrangeDropHelpper(this, expvm);

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
            uiEle.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() => OpenTab()));

            //uiEle.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, 
            //    delegate () =>
            //    {
            //        OpenTab();
            //    });
            //OpenTab();
        }

        #endregion

        #region Data

        //private ObservableCollection<ITabItemViewModel> _tabs;
        //private ITabItemViewModel _selectedTab;
        private IExplorerInitializer _initializer;

        #endregion

        #region Public Properties

        //public ObservableCollection<ITabItemViewModel> Tabs { get { return _tabs; } }
        public ICommandManager Commands { get; private set; }
        //public ITabItemViewModel SelectedTab { get { return _selectedTab; } 
        //    set { _selectedTab = value; NotifyOfPropertyChange(() => SelectedTab); } }

        #endregion




    }
}
