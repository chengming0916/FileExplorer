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

namespace FileExplorer.ViewModels
{
    public class BreadcrumbItemViewModel : DirectoryNodeViewModel, IBreadcrumbItemViewModel
    {
        #region Constructor

        public BreadcrumbItemViewModel(IEventAggregator events, IDirectoryTreeViewModel rootModel,
            IEntryModel curDirModel, IDirectoryNodeViewModel parentModel)
            : base(events, rootModel, curDirModel, parentModel)
        {

        }

        #endregion

        #region Methods

        public override IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new BreadcrumbItemViewModel(Events, TreeModel, entryModel, this);
        }


        #endregion

        #region Data

        bool _showCaption = true; bool _isShown = false;

        #endregion

        #region Public Properties

        public bool IsShown
        {
            get { return _isShown; }
            set { _isShown = value; if (value) LoadAsync(false); NotifyOfPropertyChange(() => IsShown); }
        }

        public bool ShowCaption
        {
            get { return _showCaption; }
            set { _showCaption = value; NotifyOfPropertyChange(() => ShowCaption); }
        }

        #endregion
    }
}
