using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.Models.Bookmark;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    public class AddBookmarksViewModel : Screen
    {        
        #region fields

        private IEntryModel _selectedPath;

        #endregion

        #region constructors

        public AddBookmarksViewModel(BookmarkProfile bProfile, IWindowManager windowManager, IEventAggregator events)
        {
            Profile = bProfile;            
        }

        #endregion

        #region events

        #endregion

        #region properties

        public BookmarkProfile Profile { get; set; }

        public ICommandManager Commands { get; private set; }        

        public IEntryModel SelectedDirectory
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                NotifyOfPropertyChange(() => SelectedDirectory);                
            }
        }


        #endregion

        #region methods

        #endregion
    }
}
