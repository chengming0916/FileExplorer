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
    public class BreadcrumbItemViewModel : DirectoryNodeViewModel
    {
        #region Constructor

        public BreadcrumbItemViewModel(IEventAggregator events, IDirectoryTreeViewModel rootModel,
            IEntryModel curDirModel, IDirectoryNodeViewModel parentModel)
            : base(events, rootModel, curDirModel, parentModel)
        {
        
        }

        #endregion

        #region Methods

       
      
        #endregion

        #region Data



        #endregion

        #region Public Properties

    

        #endregion
    }
}
