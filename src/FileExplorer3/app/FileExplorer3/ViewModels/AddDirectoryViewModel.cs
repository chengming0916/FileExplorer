using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Given a number of profiles, allow user to select a directory.
    /// </summary>
    public class AddDirectoryViewModel : Screen
    {

        #region Constructors

        public AddDirectoryViewModel(IWindowManager windowManager, IEventAggregator events, IProfile[] rootProfiles)
        {
            _windowManager = windowManager;
            _events = events;
            _rootProfiles = rootProfiles;
            _selectedPath = null;

        }

        #endregion

        #region Methods

        public void Add()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            SelectedDirectory = null;
            TryClose(false);
        }

        public async Task BrowsePath()
        {
            if (SelectedRootProfile != null)
            {
                IEntryModel rootDirectory = await SelectedRootProfile.ParseAsync("");
                var directoryPicker = new DirectoryPickerViewModel(
                    new ExplorerInitializer(_windowManager, _events, new IEntryModel[] { rootDirectory }));

                if (_windowManager.ShowDialog(directoryPicker).Value)
                    SelectedDirectory = directoryPicker.SelectedDirectory;


            }
        }

        #endregion

        #region Data

        private IProfile[] _rootProfiles;
        private IProfile _selectedRootProfile;
        private IEntryModel _selectedPath;
        private IWindowManager _windowManager;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        public bool CanAdd { get { return _selectedPath != null; } }

        public IProfile[] RootProfiles
        {
            get { return _rootProfiles; }
            set
            {
                _rootProfiles = value;
                NotifyOfPropertyChange(() => RootProfiles);
            }
        }
        public IProfile SelectedRootProfile
        {
            get { return _selectedRootProfile; }
            set
            {
                _selectedRootProfile = value;
                NotifyOfPropertyChange(() => SelectedRootProfile);
            }
        }
        public IEntryModel SelectedDirectory
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value; 
                NotifyOfPropertyChange(() => SelectedDirectory);
                NotifyOfPropertyChange(() => CanAdd);
            }
        }

        #endregion
    }
}
