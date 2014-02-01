using System;
using System.Collections.Generic;
#if WINRT
using Windows.UI.Xaml.Controls;
#else
using System.ComponentModel.Composition;
using System.Windows.Controls;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using FileExplorer.ViewModels.Actions;
using FileExplorer.Utils;
using System.Collections.ObjectModel;
using FileExplorer.ViewModels.Helpers;
using Cinch;
using System.Windows.Input;
using FileExplorer.Defines;
using System.Threading;


namespace FileExplorer.ViewModels
{
#if !WINRT
    [Export(typeof(FileListViewModel))]
#endif
    public class FileListViewModel : ViewAware, IFileListViewModel, 
        IHandle<ViewChangedEvent>, IHandle<DirectoryChangedEvent>, ISupportDragHelper, ISupportDropHelper
    {

        #region FileListDrag/DropHelper
        internal class FileListDropHelper : DropHelper<IEntryModel>
        {
            public FileListDropHelper(FileListViewModel flvm)
                : base(() => flvm.CurrentDirectory.Label,
                (ems, eff) => flvm.CurrentDirectory.Profile.DragDrop.QueryDrop(ems, flvm.CurrentDirectory, eff),
                    da => flvm.CurrentDirectory.Profile.DragDrop.GetEntryModels(da),
                (ems, da, eff) => flvm.CurrentDirectory.Profile.DragDrop.OnDropCompleted(ems, da, flvm.CurrentDirectory, eff), em => EntryViewModel.FromEntryModel(em))
            { }

        }
        private class FileListDragHelper : TreeDragHelper<IEntryModel>
        {
            public FileListDragHelper(FileListViewModel flvm)
                : base(
                () => flvm.Selection.SelectedItems.ToArray(),
                ems => ems.First().Profile.DragDrop.QueryDrag(ems),
                ems => ems.First().Profile.DragDrop.GetDataObject(ems),
                (ems, da, eff) => ems.First().Profile.DragDrop.OnDragCompleted(ems, da, eff)
                , d => (d as IEntryViewModel).EntryModel)
            { }
        }
        #endregion

        #region Cosntructor

        public FileListViewModel(IEventAggregator events)
        {
            Events = events;
            var entryHelper = new EntriesHelper<IEntryViewModel>(loadEntriesTask) { ClearBeforeLoad = false };
            ProcessedEntries = new EntriesProcessor<IEntryViewModel>(entryHelper);
            Columns = new ColumnsHelper(ProcessedEntries,
                (col, direction) =>
                    new EntryViewModelComparer(
                        col.Comparer != null ? col.Comparer : CurrentDirectory.Profile.GetComparer(col),
                        direction)
            );
            Selection = new ListSelector<IEntryViewModel, IEntryModel>(entryHelper);
            DropHelper = new FileListDropHelper(this);
            DragHelper = new FileListDragHelper(this);

            Selection.SelectionChanged += (o, e) =>
            { Events.Publish(new SelectionChangedEvent(this, Selection.SelectedItems)); };

            if (events != null)
                events.Subscribe(this);


            Commands = new FileListScriptCommandManager(this, events, Selection);        
        }

        #endregion

        #region Methods

        async Task<IEnumerable<IEntryViewModel>> loadEntriesTask(bool refresh)
        {
            if (CurrentDirectory == null)
                return new List<IEntryViewModel>();

            var subEntries = await CurrentDirectory.Profile.ListAsync(CurrentDirectory, CancellationToken.None, null, refresh);
            return subEntries.Select(s => CreateSubmodel(s));
        }

        public IEntryViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new FileListItemViewModel(entryModel, Selection);
        }


        #region Actions

        public async Task SetCurrentDirectoryAsync(IEntryModel em)
        {
            _currentDirVM = em;

            ProcessedEntries.EntriesHelper.IsLoaded = false;
            await ProcessedEntries.EntriesHelper.LoadAsync(false);

            Columns.CalculateColumnHeaderCount(from vm in ProcessedEntries.EntriesHelper.AllNonBindable select vm.EntryModel);

            NotifyOfPropertyChange(() => CurrentDirectory);
        }

        //public IEnumerable<IResult> ToggleRename()
        //{
        //    yield return new ToggleRename(this);
        //}


        #endregion        

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
              var uiEle = view as System.Windows.UIElement;
            Commands.RegisterCommand(uiEle, ScriptBindingScope.Local);            
        }

        public void SignalChangeDirectory(IEntryModel newDirectory)
        {
            Events.Publish(new DirectoryChangedEvent(this,
                     newDirectory, CurrentDirectory));
        }

        private void setProfiles(IProfile[] profiles)
        {
            Commands.ToolbarCommands.RootProfiles = profiles;
        }

        public void Handle(ViewChangedEvent message)
        {
            if (!(message.Sender.Equals(this)))
                this.ViewMode = message.ViewMode;
        }

        public void Handle(DirectoryChangedEvent message)
        {
            if (message.NewModel != null)
                CurrentDirectory = message.NewModel;
        }


        #endregion

        #region Data

        private IEntryModel _currentDirVM = null;
        private int _itemSize = 60;
        private string _viewMode = "Icon";
        private IToolbarViewModel _toolbar = null;
        private bool _isCheckboxVisible = false, _isContextMenuVisible = false;
        private bool _enableDrag = true, _enableDrop = true, _enableMultiSelect = true;

        #endregion

        #region Public Properties
        public IProfile[] Profiles { set { setProfiles(value); } }

        public bool EnableDrag { get { return _enableDrag; } set { _enableDrag = value; NotifyOfPropertyChange(() => EnableDrag); } }
        public bool EnableDrop { get { return _enableDrop; } set { _enableDrop = value; NotifyOfPropertyChange(() => EnableDrop); } }
        public bool EnableMultiSelect { get { return _enableMultiSelect; } set { _enableMultiSelect = value; NotifyOfPropertyChange(() => EnableMultiSelect); } }

        public IScriptCommandManager Commands { get; private set; }        
        public IEntriesProcessor<IEntryViewModel> ProcessedEntries { get; private set; }
        public IColumnsHelper Columns { get; private set; }
        public IEventAggregator Events { get; private set; }
        public IListSelector<IEntryViewModel, IEntryModel> Selection { get; private set; }
        public ISupportDrag DragHelper { get; private set; }
        public ISupportDrop DropHelper { get; private set; }

        public IToolbarViewModel Toolbar { get { return _toolbar; } set { _toolbar = value; NotifyOfPropertyChange(() => Toolbar); } }

        public IEntryModel CurrentDirectory
        {
            get { return _currentDirVM; }
            set { SetCurrentDirectoryAsync(value); }
        }


        #region ViewMode, ItemSize

        public bool IsCheckBoxVisible
        {
            get { return _isCheckboxVisible; }
            set { _isCheckboxVisible = value; NotifyOfPropertyChange(() => IsCheckBoxVisible); }
        }

        public bool IsContextMenuVisible
        {
            get { return _isContextMenuVisible; }
            set { _isContextMenuVisible = value; NotifyOfPropertyChange(() => IsContextMenuVisible); }
        }

        public string ViewMode
        {
            get { return _viewMode; }
            set
            {
                _viewMode = value;
                NotifyOfPropertyChange(() => ViewMode);
            }
        }

        public int ItemSize
        {
            get { return _itemSize; }
            set
            {
                _itemSize = value;
                NotifyOfPropertyChange(() => ItemSize);
            }
        }



        #endregion


        #endregion


    }
}


