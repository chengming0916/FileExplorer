using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.BaseControls;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public class BreadcrumbViewModel : DirectoryTreeViewModel, IBreadcrumbViewModel
    {
        #region Constructor

        public BreadcrumbViewModel(IExplorerViewModel explorerModel, IEventAggregator events, 
            IEntryModel[] rootModels)
            : base(events, rootModels)
        {
            Label = Subdirectories.First().CurrentDirectory.EntryModel.Profile.RootDisplayName;
            Icon = Subdirectories.First().CurrentDirectory.EntryModel.Profile.GetIconAsync(null, 32).Result; 
        
            _rootViewModel = new BindableCollection<IDirectoryNodeViewModel>(rootModels
                .Select(r => new BreadcrumbItemViewModel(events, this, r, null)));

            //_rootViewModel = new BreadcrumbItemViewModel(events, this, rootModels);

            HierarchyHelper = new BreadcrumbHierarchyHelper(this);
                
                //new EntryViewModelHierarchyHelper(rootModels, m => EntryViewModel.FromEntryModel(m), m => m.IsDirectory);
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        //BreadcrumbItemViewModel _rootViewModel;
        IHierarchyHelper _hierarchyHelper;

        #endregion

        #region Public Properties
        
        public IHierarchyHelper HierarchyHelper { get { return _hierarchyHelper; } set { _hierarchyHelper = value; NotifyOfPropertyChange(() => HierarchyHelper); } }
        //public BreadcrumbItemViewModel RootViewModel { get { return _rootViewModel; } set { _rootViewModel = value; NotifyOfPropertyChange(() => RootViewModel); } }

        public string Label
        {
            get;
            set; 
        }

        public string Value
        {
            get { return ""; }
        }

        public ImageSource Icon
        {
            get;
            set; 
        }


        public override IObservableCollection<IDirectoryNodeViewModel> Subdirectories
        {
            get { return base.Subdirectories; }
        }

        public IObservableCollection<IDirectoryNodeViewModel> SubdirectoriesChecked
        {
            get { return base.Subdirectories; }
        }

        #endregion
    }
}
