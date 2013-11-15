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

            _events = events;
            rootModels =
                rootModels.Union(
                    rootModels.SelectMany(r => r.Profile.ListAsync(r, m => m.IsDirectory).Result))
                .ToArray();
            Subdirectories = new BindableCollection<IDirectoryNodeViewModel>(rootModels
                .Select(r => new BreadcrumbItemViewModel(events, this, r, null)));


            //_rootViewModel = new BreadcrumbItemViewModel(events, this, rootModels);

            //HierarchyHelper = new BreadcrumbHierarchyHelper(this);

            //new EntryViewModelHierarchyHelper(rootModels, m => EntryViewModel.FromEntryModel(m), m => m.IsDirectory);
        }

        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            _bcrumb = (view as FileExplorer.Views.BreadcrumbView).bcrumb;
            _bcrumb.AddValueChanged(BreadcrumbBase.SelectedValueProperty, (o, e) =>
                {
                    Debug.WriteLine(_bcrumb.SelectedValue);
                    if (SelectedViewModel == null || !SelectedViewModel.Equals(_bcrumb.SelectedValue))
                    {
                        SelectedViewModel = _bcrumb.SelectedValue as IDirectoryNodeViewModel;
                        updateBreadcrumb(SelectedViewModel);
                        updateExternal(SelectedViewModel);
                    }
                });
            updateBreadcrumb(base.Subdirectories.FirstOrDefault());
        }

        /// <summary>
        /// Update Breadcrumb for SelectedValue or Hierarchy.
        /// </summary>
        /// <param name="selectedViewModel"></param>
        protected void updateBreadcrumb(IDirectoryNodeViewModel selectedViewModel)
        {
            _bcrumb.SetValue(BreadcrumbBase.SelectedValueProperty, selectedViewModel);
            if (selectedViewModel == null)
                _bcrumb.SetValue(BreadcrumbBase.ItemsSourceProperty, null);
            else _bcrumb.SetValue(BreadcrumbBase.ItemsSourceProperty,
                 selectedViewModel.GetHierarchy(true).Reverse());

            _bcrumb.SetValue(BreadcrumbBase.TextProperty,
                selectedViewModel == null ? "" : selectedViewModel.CurrentDirectory.EntryModel.FullPath);
        }

        /// <summary>
        /// Breadcrumb selected, so update external (by raising event)
        /// </summary>
        /// <param name="selectedViewModel"></param>
        protected void updateExternal(IDirectoryNodeViewModel selectedViewModel)
        {
            if (selectedViewModel == null) //Root   
                _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { }));
            else _events.Publish(new SelectionChangedEvent(this,
               new IEntryViewModel[] { selectedViewModel.CurrentDirectory }));
        }

        public override async Task SelectAsync(IEntryModel model)
        {
            if (_bcrumb.RootItemsSource != null)
                foreach (var rootNodes in _bcrumb.RootItemsSource.Cast<IDirectoryNodeViewModel>())
                {
                    await rootNodes.BroadcastSelectAsync(model, (foundModel) =>
                        {
                            SelectedViewModel = foundModel;
                            updateBreadcrumb(SelectedViewModel);
                        });
                }

            //SelectedViewModel = BreadcrumbItemViewModel.FromEntryModel(_events, this, model);
            //updateBreadcrumb(SelectedViewModel);
        }

        //public void NotifySelected(DirectoryNodeViewModel node)
        //{
        //    _events.Publish(new SelectionChangedEvent(this,
        //        new IEntryViewModel[] { node.CurrentDirectory }));

        //    _selectedViewModel = node;
        //    NotifyOfPropertyChange(() => SelectedEntry);
        //    NotifyOfPropertyChange(() => SelectedViewModel);
        //}

        #endregion

        #region Data

        BreadcrumbBase _bcrumb = null;
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
