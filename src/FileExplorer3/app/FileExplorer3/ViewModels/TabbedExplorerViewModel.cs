using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public class TabbedExplorerViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private IExplorerInitializer _initializer;
        #region Constructors

        public TabbedExplorerViewModel(IExplorerInitializer initializer)
        {
            _initializer = initializer;
            OpenTab();
            ////_tabs = new ObservableCollection<ITabItemViewModel>();
        }

        #endregion

        #region Methods

        public void OpenTab(IExplorerViewModel evm = null)
        {
            evm = evm ?? new ExplorerViewModel(_initializer);
            ActivateItem(evm);
        }

        public void CloseTab(IExplorerViewModel evm = null)
        {
            DeactivateItem(evm, true);
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
