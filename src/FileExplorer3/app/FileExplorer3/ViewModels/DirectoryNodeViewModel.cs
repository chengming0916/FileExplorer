using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Actions;
using FileExplorer.ViewModels.Helpers;
using System.Windows;

namespace FileExplorer.ViewModels
{

    public class DirectoryNodeViewModel : EntryViewModel, IDirectoryNodeViewModel, ISupportDropHelper
    {
        public enum NodeState { IsCreated, IsLoading, IsLoaded, IsError, IsInvalid }

        #region Cosntructor

        #region DirectoryNodeDropHelper
        internal class DirectoryNodeDropHelper : DropHelper<IEntryModel>
        {
            private static IEnumerable<IEntryModel> dataObjectFunc(IDataObject da,
                ITreeSelector<IDirectoryNodeViewModel, IEntryModel> selection)
            {
                var profiles = selection.RootSelector.EntryHelper.All.Select(rvm => rvm.EntryModel.Profile);
                foreach (var p in profiles)
                {
                    var retVal = p.GetEntryModels(da);
                    if (retVal != null)
                        return retVal;
                }
                return null;
            }

          

            public DirectoryNodeDropHelper(IEntryModel curDir, IEntriesHelper<IDirectoryNodeViewModel> entries,
                ITreeSelector<IDirectoryNodeViewModel, IEntryModel> selection)
                : base(
                () => curDir.Label,
                (ems, eff) => curDir.Profile.QueryDrop(ems, curDir, eff),                
                da => dataObjectFunc(da, selection),                
                (ems, da, eff) => curDir.Profile.OnDropCompleted(ems, da, curDir, eff), em => EntryViewModel.FromEntryModel(em))
            { }
        }
        #endregion

        public static IDirectoryNodeViewModel DummyNode = new DirectoryNodeViewModel();

        /// <summary>
        /// For dummy node.
        /// </summary>
        private DirectoryNodeViewModel() 
            : base()
        {

        }

        /// <summary>
        /// For displaying contents only (e.g. DragAdorner).
        /// </summary>
        /// <param name="curDirModel"></param>
        public DirectoryNodeViewModel(IEntryModel curDirModel) 
            : base(curDirModel)
        {
            
        }

        public DirectoryNodeViewModel(IEventAggregator events, IDirectoryTreeViewModel rootModel, IEntryModel curDirModel,
            IDirectoryNodeViewModel parentModel)
            : base(curDirModel)
        {

            _events = events;
            _rootModel = rootModel;
            
            Entries = new EntriesHelper<IDirectoryNodeViewModel>(loadEntriesTask);
            Selection = new TreeSelector<IDirectoryNodeViewModel, IEntryModel>(curDirModel, this, 
                parentModel == null ? rootModel.Selection : parentModel.Selection, Entries);
            DropHelper = new DirectoryNodeDropHelper(curDirModel, Entries, Selection);
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
        public ITreeSelector<IDirectoryNodeViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }
        public ISupportDrop DropHelper { get; set; }


        #endregion










     
    }
}
