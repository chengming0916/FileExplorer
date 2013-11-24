using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IBreadcrumbItemViewModel : ISupportNodeSelectionHelper<IBreadcrumbItemViewModel, IEntryModel>
    {
        #region Constructor
        
        #endregion

        #region Methods
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        bool ShowCaption { get; set; }        
        ISubEntriesHelper<IBreadcrumbItemViewModel> Entries { get; set; }
        
        #endregion
    }
}
