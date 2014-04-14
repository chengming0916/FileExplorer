using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Models;
using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FileExplorer.ViewModels
{

    public class TabbedExplorerViewModel : Conductor<IScreen>.Collection.OneActive, ITabbedExplorerViewModel
    {
        private IExplorerInitializer _initializer;
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
            IExplorerViewModel evm = new ExplorerViewModel(initializer);
            ActivateItem(evm);
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

        #endregion

        #region Public Properties

        //public ObservableCollection<ITabItemViewModel> Tabs { get { return _tabs; } }
        public ICommandManager Commands { get; private set; }
        //public ITabItemViewModel SelectedTab { get { return _selectedTab; } 
        //    set { _selectedTab = value; NotifyOfPropertyChange(() => SelectedTab); } }

        #endregion




    }
}
