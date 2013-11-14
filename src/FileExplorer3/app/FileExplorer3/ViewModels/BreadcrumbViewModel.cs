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
            Label = Subdirectories.First().CurrentDirectory.EntryModel.Profile.RootDisplayName;
            Icon = Subdirectories.First().CurrentDirectory.EntryModel.Profile.GetIconAsync(null, 32).Result;

            _events = events;
            _rootViewModel = new BindableCollection<IDirectoryNodeViewModel>(rootModels
                .Select(r => new BreadcrumbItemViewModel(events, this, r, null)));

            //_rootViewModel = new BreadcrumbItemViewModel(events, this, rootModels);

            HierarchyHelper = new BreadcrumbHierarchyHelper(this);
                
                //new EntryViewModelHierarchyHelper(rootModels, m => EntryViewModel.FromEntryModel(m), m => m.IsDirectory);
        }

        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            _bcrumb = (view as FileExplorer.Views.BreadcrumbView).bcrumb;
            _bcrumb.AddValueChanged(Breadcrumb.SelectedValueProperty, (o, e) =>
                {
                    Debug.WriteLine((o as Breadcrumb).SelectedValue);
                    SelectedViewModel = (o as Breadcrumb).SelectedValue as IDirectoryNodeViewModel;    
                    if (SelectedViewModel == null) //Root                    
                        _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] {}));
                    else _events.Publish(new SelectionChangedEvent(this, 
                        new IEntryViewModel[] { SelectedViewModel.CurrentDirectory }));
                });
        }


        public override void Select(IEntryModel model)
        {
            _bcrumb.SelectedPathValue = model.FullPath;
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

        Breadcrumb _bcrumb = null;
        //BreadcrumbItemViewModel _rootViewModel;
        IHierarchyHelper _hierarchyHelper;
        object _selectedValue;
        private IEventAggregator _events;

        #endregion

        #region Public Properties
        
        public IHierarchyHelper HierarchyHelper { get { return _hierarchyHelper; } set { _hierarchyHelper = value; NotifyOfPropertyChange(() => HierarchyHelper); } }
        //public BreadcrumbItemViewModel RootViewModel { get { return _rootViewModel; } set { _rootViewModel = value; NotifyOfPropertyChange(() => RootViewModel); } }


        //public IEntryModel SelectedEntry
        //{
        //    get { return _selectedViewModel == null ? null : _selectedViewModel.CurrentDirectory.EntryModel; }
        //    set { Select(_selectedEntry); }
        //}

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

        //public IObservableCollection<IDirectoryNodeViewModel> SubdirectoriesChecked
        //{
        //    get { return base.Subdirectories; }
        //}

        #endregion
    }
}
