using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
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
            base.Subdirectories.Clear();
        }

        #endregion

        #region Methods

        public override IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new BreadcrumbItemViewModel(Events, TreeModel, entryModel, this);
        }

        public void OnIsTopLevelChanged(bool value)
        {
            if (value)
                LoadAsync(false);
        }

        #endregion

        #region Data

        bool _isTopLevel = false;

        #endregion

        #region Public Properties

        public bool IsTopLevel
        {
            get { return _isTopLevel; }
            set
            {
                //Debug.WriteLine("{0} - {1}", this.ToString(), value);
                _isTopLevel = value;
                //NotifyOfPropertyChange(() => IsTopLevel);
                OnIsTopLevelChanged(value);
            }
        }
        public string Label
        {
            get { return CurrentDirectory.EntryModel.Label; }
        }

        public string Value
        {
            get { return CurrentDirectory.EntryModel.FullPath.TrimEnd('\\'); }
        }

        public override IObservableCollection<IDirectoryNodeViewModel> Subdirectories
        {
            get { /* if (IsTopLevel) LoadAsync(false).Wait();*/ return base.Subdirectories; }
        }

        public ImageSource Icon
        {
            get { return CurrentDirectory.Icon.Value; }
        }

        //public IObservableCollection<IDirectoryNodeViewModel> SubdirectoriesChecked
        //{
        //    get { LoadAsync(false).Wait(); return base.Subdirectories; }
        //}

        #endregion
    }
}
