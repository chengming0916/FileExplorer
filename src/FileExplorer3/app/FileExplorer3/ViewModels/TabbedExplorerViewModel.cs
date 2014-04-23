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

        private class TabArrangeDropHelpper : ISupportDrag, ISupportDrop
        {

            #region Constructors

            public TabArrangeDropHelpper(ITabbedExplorerViewModel tevm, IExplorerViewModel evm)
            {
                _tevm = tevm;
                _evm = evm;
            }

            #endregion

            #region Methods

            private static IExplorerViewModel getExplorerViewModel(IEnumerable<IDraggable> draggables)
            {
                if (draggables.Count() == 1)
                    return draggables.FirstOrDefault() as IExplorerViewModel;
                return null;
            }

            public IEnumerable<IDraggable> GetDraggables()
            {
                return new List<IDraggable>() { _evm };
            }

            public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
            {
                return DragDropEffects.Move;
            }

            public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
            {
                var expVM = getExplorerViewModel(draggables);
                return expVM == _evm ? new DataObject(typeof(IExplorerViewModel), expVM) : null;
            }

            public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
            {
                var expVM = getExplorerViewModel(draggables);
                if (expVM == _evm)
                    _tevm.CloseTab(_evm);
            }



            public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
            {
                if (!(_tevm.ActiveItem.Equals(_evm)))
                    _tevm.ActivateItem(_evm);
                return QueryDropResult.None;
            }

            public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
            {
                if (da.GetDataPresent(typeof(IExplorerViewModel)))
                    return new List<IDraggable>() { da.GetData(typeof(IExplorerViewModel)) as IExplorerViewModel };
                else
                    return new List<IDraggable>();
            }


            public DragDropEffects Drop(IEnumerable<IDraggable> draggables,
                IDataObject da, DragDropEffects allowedEffects)
            {
                var expVM = getExplorerViewModel(draggables);
                if (expVM != null)
                    return DragDropEffects.Move;

                return DragDropEffects.None;
            }


            #endregion

            #region Data
            private IExplorerViewModel _evm;
            private ITabbedExplorerViewModel _tevm;

            #endregion

            #region Public Properties

            public bool HasDraggables { get { return true; } }

            public bool IsDraggingOver { get; set; }
            public bool IsDroppable { get { return true; } }

            public string DropTargetLabel
            {
                get
                {
                    return _evm.CurrentDirectory == null ? "" :
                        _evm.CurrentDirectory.EntryModel.Label;
                }
            }


            #endregion















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






        public int GetTabIndex(IExplorerViewModel evm)
        {
            throw new NotImplementedException();
        }

        public void InsertTab(IExplorerViewModel evm, int idx)
        {
            throw new NotImplementedException();
        }
    }
}
