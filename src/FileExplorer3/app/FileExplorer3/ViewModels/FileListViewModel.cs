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


namespace FileExplorer.ViewModels
{
#if !WINRT
    [Export(typeof(FileListViewModel))]
#endif
    public class FileListViewModel : PropertyChangedBase, IFileListViewModel,
        IHandle<ViewChangedEvent>, IHandle<DirectoryChangedEvent>, ISupportDragHelper, ISupportDropHelper
    {

        #region FileListDrag/DropHelper
        internal class FileListDropHelper : DropHelper<IEntryModel>
        {
            public FileListDropHelper(FileListViewModel flvm)
                : base(() => flvm.CurrentDirectory.Label,
                (ems, eff) => flvm.CurrentDirectory.Profile.QueryDrop(ems, flvm.CurrentDirectory, eff),
                    da => flvm.CurrentDirectory.Profile.GetEntryModels(da),
                (ems, da, eff) => flvm.CurrentDirectory.Profile.OnDropCompleted(ems, da, flvm.CurrentDirectory, eff), em => EntryViewModel.FromEntryModel(em))
            { }

        }
        private class FileListDragHelper : TreeDragHelper<IEntryModel>
        {
            public FileListDragHelper(FileListViewModel flvm)
                : base(
                () => flvm.Selection.SelectedItems.ToArray(),
                ems => ems.First().Profile.QueryDrag(ems),
                ems => ems.First().Profile.GetDataObject(ems),
                (ems, da, eff) => ems.First().Profile.OnDragCompleted(ems, da, eff)
                , d => (d as IEntryViewModel).EntryModel)
            { }
        }
        #endregion

        #region Cosntructor

        public FileListViewModel(IEventAggregator events)
        {
            Events = events;
            var entryHelper = new EntriesHelper<IEntryViewModel>(loadEntriesTask);
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

            Toolbar = new ToolbarViewModel(events);

            Selection.SelectionChanged += (o, e) =>
            { Events.Publish(new SelectionChangedEvent(this, Selection.SelectedItems)); };

            if (events != null)
                events.Subscribe(this);
            #region Unused
            //var ec = ConventionManager.AddElementConvention<ListView>(
            //   ListView.ItemsSourceProperty, "ItemsSource", "SourceUpdated");
            //ec.ApplyBinding = (vmType, path, property, element, convention) =>
            //{
            //    ConventionManager.SetBinding(vmType, path, property, element,
            //        ec, ItemsControl.ItemsSourceProperty);
            //    return true;
            //};
            #endregion
        }

        #endregion

        #region Methods

        async Task<IEnumerable<IEntryViewModel>> loadEntriesTask()
        {
            if (CurrentDirectory == null)
                return new List<IEntryViewModel>();

            var subEntries = await CurrentDirectory.Profile.ListAsync(CurrentDirectory);
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

            await ProcessedEntries.EntriesHelper.LoadAsync(true);

            Columns.CalculateColumnHeaderCount(from vm in ProcessedEntries.EntriesHelper.AllNonBindable select vm.EntryModel);

            NotifyOfPropertyChange(() => CurrentDirectory);
        }

        public IEnumerable<IResult> ToggleRename()
        {
            yield return new ToggleRename(this);
        }

        #endregion

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


        #endregion

        #region Public Properties

        public IEntriesProcessor<IEntryViewModel> ProcessedEntries { get; private set; }
        public IColumnsHelper Columns { get; private set; }
        public IEventAggregator Events { get; private set; }
        public IListSelector<IEntryViewModel, IEntryModel> Selection { get; private set; }
        public ISupportDrag DragHelper { get; private set; }
        public ISupportDrop DropHelper { get; private set; }

        public IToolbarViewModel Toolbar { get; private set; }

        public IEntryModel CurrentDirectory
        {
            get { return _currentDirVM; }
            set { SetCurrentDirectoryAsync(value); }
        }


        #region ViewMode, ItemSize

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


