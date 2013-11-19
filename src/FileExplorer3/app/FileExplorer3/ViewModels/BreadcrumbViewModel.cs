using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            _profiles = rootModels.Select(rm => rm.Profile).Distinct();
            SuggestSources = _profiles.Select(p => p.GetSuggestSource());
            if (Subdirectories.Count > 0)
                Subdirectories[0].IsSelected = true;
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

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            //_sbox = (view as UserControl).FindName("sbox") as SuggestBoxBase;
            //_sbox.TextChanged += (o, e) =>
            //    {
            //        _sbox.Suggestions
            //    };
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
            _suggestedPath = model.FullPath;
            NotifyOfPropertyChange(() => SuggestedPath);
        }

        void OnSuggestPathChanged()
        {
            if (!ShowBreadcrumb)
            {
                

                foreach (var p in _profiles)
                {
                    var found = p.ParseAsync(SuggestedPath).Result;
                    if (found != null)
                    {
                        ShowBreadcrumb = true;
                        base.SelectAsync(found);
                        BroadcastDirectoryChanged(EntryViewModel.FromEntryModel(found));
                    }
                    //else not found
                }
            }
        }

        #endregion

        #region Data

        private IEnumerable<IProfile> _profiles;
        private string _suggestedPath;
        private bool _showBreadcrumb = true;
        private IEnumerable<ISuggestSource> _suggestSources;

        #endregion

        #region Public Properties
        public bool ShowBreadcrumb
        {
            get { return _showBreadcrumb; }
            set { _showBreadcrumb = value; NotifyOfPropertyChange(() => ShowBreadcrumb); }
        }

        public IEnumerable<ISuggestSource> SuggestSources
        {
            get { return _suggestSources; }
            set { _suggestSources = value; NotifyOfPropertyChange(() => SuggestSources); }
        }

        public string SuggestedPath
        {
            get { return _suggestedPath; }
            set
            {
                _suggestedPath = value;
                NotifyOfPropertyChange(() => SuggestedPath);
                OnSuggestPathChanged();
            }
        }

        #endregion


    }
}
