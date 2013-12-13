using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class FileListItemViewModel : EntryViewModel, IFileListItemViewModel
    {
        #region Constructor

        public FileListItemViewModel(IEntryModel model, IReportSelected<IEntryViewModel> reportSelected)
            : base(model)
        {
            _reportSelected = reportSelected;
            
        }

        #endregion

        #region Methods

        public override void NotifyOfPropertyChange(string propertyName = "")
        {
            base.NotifyOfPropertyChange(propertyName);

            switch (propertyName)
            {
                case "IsSelected" :
                    if (this.IsSelected)
                        _reportSelected.ReportChildSelected(this);
                    else _reportSelected.ReportChildUnSelected(this);
                    break;
            }
        }

        #endregion

        #region Data

        IReportSelected<IEntryViewModel> _reportSelected;

        #endregion

        #region Public Properties

        #endregion
    }
}
