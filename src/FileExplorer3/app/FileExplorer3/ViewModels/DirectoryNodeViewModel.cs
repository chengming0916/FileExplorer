using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Actions;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{

    public class DirectoryNodeViewModel : PropertyChangedBase, IDirectoryNodeViewModel
    {
        public enum NodeState { IsCreated, IsLoading, IsLoaded, IsError, IsInvalid }

        #region Cosntructor

        public static IDirectoryNodeViewModel DummyNode = new DirectoryNodeViewModel();

        private DirectoryNodeViewModel() //For Dummynode.            
        {

        }

        public DirectoryNodeViewModel(IEventAggregator events, IDirectoryTreeViewModel rootModel, IEntryModel curDirModel,
            IDirectoryNodeViewModel parentModel)
        {

            _events = events;
            _rootModel = rootModel;

            CurrentDirectory = EntryViewModel.FromEntryModel(curDirModel);
            Entries = new SubEntriesHelper<IDirectoryNodeViewModel>(loadEntriesTask);
            Selection = new TreeSelector<IDirectoryNodeViewModel, IEntryModel>(curDirModel, this, 
                parentModel == null ? rootModel.Selection : parentModel.Selection, Entries);
        }



        #endregion

        #region Methods

        async Task<IEnumerable<IDirectoryNodeViewModel>> loadEntriesTask()
        {
            IEntryModel currentDir = Selection.Value;
            var subDir = await currentDir.Profile.ListAsync(currentDir, em => em.IsDirectory);
            return subDir.Select(s => CreateSubmodel(s));
        }

        public IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new DirectoryNodeViewModel(_events, _rootModel, entryModel, this);
        }

    
  

        #endregion

        #region Data

        IEventAggregator _events;
        bool _showCaption = true; bool _isShown = false;
        private IDirectoryTreeViewModel _rootModel;

        #endregion

        #region Public Properties

        public bool ShowCaption { get { return _showCaption; } set { _showCaption = value; NotifyOfPropertyChange(() => ShowCaption); } }
        public IEntryViewModel CurrentDirectory { get; set; }
        public ITreeSelector<IDirectoryNodeViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }


        #endregion









    }
}
