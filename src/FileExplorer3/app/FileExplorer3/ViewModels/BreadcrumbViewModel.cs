using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.BaseControls;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public class BreadcrumbViewModel : Screen, IBreadcrumbViewModel
    {
        #region Constructor

        public BreadcrumbViewModel(IExplorerViewModel explorerModel, IEventAggregator events, IEntryModel[] rootModels)
        {
            _rootViewModel = new BindableCollection<IEntryViewModel>(rootModels
                .Select(m => EntryViewModel.FromEntryModel(m)));
            
            HierarchyHelper = new EntryViewModelHierarchyHelper(rootModels, m => EntryViewModel.FromEntryModel(m), m => m.IsDirectory);
        }
        
        #endregion

        #region Methods
        
        #endregion

        #region Data

        BindableCollection<IEntryViewModel> _rootViewModel;
        IHierarchyHelper _hierarchyHelper;
        
        #endregion

        #region Public Properties

        public IHierarchyHelper HierarchyHelper { get { return _hierarchyHelper; } set { _hierarchyHelper = value; NotifyOfPropertyChange(() => HierarchyHelper); } }
        public BindableCollection<IEntryViewModel> RootViewModels { get { return _rootViewModel; } set { _rootViewModel = value; NotifyOfPropertyChange(() => RootViewModels); } }
        
        #endregion
    }
}
