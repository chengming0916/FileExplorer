using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class BreadcrumbItemViewModel : PropertyChangedBase, IBreadcrumbItemViewModel
    {
        #region Constructor

        public BreadcrumbItemViewModel(IEventAggregator events, IBreadcrumbViewModel rootModel,
            IEntryModel curDirModel, IBreadcrumbItemViewModel parentModel)
        {
            _events = events;
            _rootModel = rootModel;

            CurrentDirectory = EntryViewModel.FromEntryModel(curDirModel);
            Entries = new EntriesHelper<IBreadcrumbItemViewModel>(loadEntriesTask);
            Selection = new TreeSelector<IBreadcrumbItemViewModel, IEntryModel>(curDirModel, this, 
                parentModel == null ? rootModel.Selection : parentModel.Selection, Entries);
        }

        #endregion

        #region Methods

        async Task<IEnumerable<IBreadcrumbItemViewModel>> loadEntriesTask()
        {
            IEntryModel currentDir = Selection.Value;
            var subDir = await currentDir.Profile.ListAsync(currentDir, em => em.IsDirectory);
            return subDir.Select(s => CreateSubmodel(s));
        }

        public IBreadcrumbItemViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new BreadcrumbItemViewModel(_events, _rootModel, entryModel, this);
        }


        #endregion

        #region Data

        IEventAggregator _events;
        bool _showCaption = true; bool _isShown = false;
        private IBreadcrumbViewModel _rootModel;
        

        #endregion

        #region Public Properties
        
        public IEntryViewModel CurrentDirectory { get; set; }
        public ITreeSelector<IBreadcrumbItemViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IBreadcrumbItemViewModel> Entries { get; set; }

        public bool IsShown
        {
            get { return _isShown; }
            set { _isShown = value; if (value) Entries.LoadAsync(false); NotifyOfPropertyChange(() => IsShown); }
        }

        public bool ShowCaption
        {
            get { return _showCaption; }
            set { _showCaption = value; NotifyOfPropertyChange(() => ShowCaption); }
        }

        #endregion
    }
}
