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
            Subdirectories = new BindableCollection<IDirectoryNodeViewModel>(rootModels
                .Select(r => new BreadcrumbItemViewModel(events, this, r, null)));
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

        public override void NotifyOfPropertyChange(string propertyName = "")
        {
            base.NotifyOfPropertyChange(propertyName);
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

        private string _selectedEntryPath;

        #endregion

        #region Public Properties


        public string SelectedEntryPath
        {
            get { return _selectedEntryPath; }
            set { _selectedEntryPath = value; NotifyOfPropertyChange(() => SelectedEntryPath); }
        }

        #endregion


    }
}
