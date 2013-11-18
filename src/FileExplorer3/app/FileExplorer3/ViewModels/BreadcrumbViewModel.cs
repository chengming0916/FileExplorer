using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public class BreadcrumbViewModel : DirectoryTreeViewModel, IBreadcrumbViewModel
    {
        #region Constructor

        public BreadcrumbViewModel(IExplorerViewModel explorerModel, IEventAggregator events,
            IEntryModel[] rootModels)
            : base(events, rootModels)
        {
            //Label = Subdirectories.First().CurrentDirectory.EntryModel.Profile.RootDisplayName;
            //Icon = Subdirectories.First().CurrentDirectory.EntryModel.Profile.GetIconAsync(null, 32).Result;

            //_events = events;
            //rootModels =
            //    rootModels.Union(
            //        rootModels.SelectMany(r => r.Profile.ListAsync(r, m => m.IsDirectory).Result))
            //.ToArray();
            Subdirectories = new BindableCollection<IDirectoryNodeViewModel>(rootModels
                .Select(r => new BreadcrumbItemViewModel(events, this, r, null)));


            //_rootViewModel = new BreadcrumbItemViewModel(events, this, rootModels);

            //HierarchyHelper = new BreadcrumbHierarchyHelper(this);

            //new EntryViewModelHierarchyHelper(rootModels, m => EntryViewModel.FromEntryModel(m), m => m.IsDirectory);
        }

        #endregion

        #region Methods

        protected override IDirectoryNodeBroadcastHandler[] getBroadcastHandlers(IEntryModel model)
        {
            return new IDirectoryNodeBroadcastHandler[] {
                BroadcastSubEntry.All(model, (nvm,hr) => hr == HierarchicalResult.Child),
                         UpdateIsSelected.Instance
            };
        }

        

        public override void NotifySelectionChanged(IEnumerable<IDirectoryNodeViewModel> path, bool selected)
        {
            base.NotifySelectionChanged(path, selected);
            if (selected)
            {
                if (path.Count() > 0)
                    (path.First() as IBreadcrumbItemViewModel).ShowCaption = (path.Count() <= 1);
            }
        }


        public override async Task SelectAsync(IEntryModel model)
        {
            await base.SelectAsync(model);
        }

        #endregion

        #region Data

        //BreadcrumbBase _bcrumb = null;
        //BreadcrumbItemViewModel _rootViewModel;
        //IHierarchyHelper _hierarchyHelper;
        //object _selectedValue;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        //public IHierarchyHelper HierarchyHelper { get { return _hierarchyHelper; } set { _hierarchyHelper = value; NotifyOfPropertyChange(() => HierarchyHelper); } }
        //public BreadcrumbItemViewModel RootViewModel { get { return _rootViewModel; } set { _rootViewModel = value; NotifyOfPropertyChange(() => RootViewModel); } }


        //public override IObservableCollection<IDirectoryNodeViewModel> Subdirectories
        //{
        //    get { return base.Subdirectories; }
        //}

        #endregion


    }
}
