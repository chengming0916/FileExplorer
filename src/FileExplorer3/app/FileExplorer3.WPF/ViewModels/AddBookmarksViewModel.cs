using FileExplorer.Models;
using FileExplorer.Models.Bookmark;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer3.WPF.ViewModels
{
    public class AddBookmarksViewModel : NotifyPropertyChanged
    {        
        #region fields

        private IEntryModel _selectedPath;

        #endregion

        #region constructors

        #endregion

        #region events

        #endregion

        #region properties

        public BookmarkProfile Profile { get; set; }

        public IEntryModel SelectedDirectory
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                NotifyOfPropertyChanged(() => SelectedDirectory);                
            }
        }


        #endregion

        #region methods

        #endregion
    }
}
