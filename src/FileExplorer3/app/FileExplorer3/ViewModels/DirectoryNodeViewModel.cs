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

namespace FileExplorer.ViewModels
{

    public class DirectoryNodeViewModel : FileListViewModel, IDirectoryNodeViewModel
    {
        public enum NodeState { IsCreated, IsLoading, IsLoaded, IsError, IsInvalid }

        #region Cosntructor

        public static IDirectoryNodeViewModel DummyNode = new DirectoryNodeViewModel();

        private DirectoryNodeViewModel() //For Dummynode.
            : base(null, false)
        {

        }

        public DirectoryNodeViewModel(IEventAggregator events, IDirectoryTreeViewModel rootModel, IEntryModel curDirModel,
            IDirectoryNodeViewModel parentNode)
            : base(events, false)
        {
            TreeModel = rootModel;
            ParentNode = parentNode;
            CurrentDirectory = EntryViewModel.FromEntryModel(curDirModel);

            this.ColumnList =
                new ColumnInfo[] { 
                    ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", null, 200) };

            //this.ColumnFilters =
            //    new ColumnFilter[] { 
            //        ColumnFilter.CreateNew("DirectoryOnly", "EntryModel.Label", em => em.IsDirectory) };

            this.SortBy = "EntryModel.Label";
            this.SortDirection = ListSortDirection.Ascending;

            this.Subdirectories.Add(DirectoryNodeViewModel.DummyNode);
        }



        #endregion

        #region Methods

        #region Utils



        #endregion

        public async Task LoadAsync(bool force = false)
        {
            if (State != NodeState.IsLoading)
            {
                if (State == NodeState.IsLoaded && !force)
                    return;
                Debug.WriteLine(CurrentDirectory.EntryModel.FullPath);
                try
                {
                    State = NodeState.IsLoading;
                    var entryModels = await base.LoadAsync(CurrentDirectory.EntryModel, em => em.IsDirectory);
                    replaceEntryList(_subdirs, entryModels, evm => CreateSubmodel(evm));
                    State = NodeState.IsLoaded;
                }
                catch (Exception ex)
                {
                    State = NodeState.IsError;
                }
            }
            else
            {
                while (State == NodeState.IsLoading)
                    await Task.Delay(1000);
            }
        }

        public override string ToString()
        {
            return "node-" + CurrentDirectory == null ? "??" : CurrentDirectory.ToString();
        }

        public virtual IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new DirectoryNodeViewModel(Events, TreeModel, entryModel, this);
        }

        protected async Task BroadcastChildSelectedAsync(IEntryModel model, Action<IDirectoryNodeViewModel> action,
            bool load = false, Func<IDirectoryNodeViewModel, IEntryModel, bool> matchFilter = null)
        {
            if (load)
                await LoadAsync();
            if (matchFilter == null)
                foreach (var subdir in Subdirectories)
                    await subdir.BroadcastSelectAsync(model, action);
            else
            {
                var matched = findMatched(model, this.Subdirectories, matchFilter);
                if (matched != null)
                    await matched.BroadcastSelectAsync(model, action);
            }
        }


        public virtual async Task BroadcastSelectAsync(IDirectoryTreeViewModel sender, IEntryModel model, IDirectoryNodeBroadcastHandler[] allHandlers)
        {
            var result = model == null || CurrentDirectory == null ? HierarchicalResult.Unrelated :
                model.Profile.HierarchyComparer.CompareHierarchy(CurrentDirectory.EntryModel, model);
            foreach (var handler in allHandlers)
                if (handler.AppliedResult.HasFlag(result))
                    await handler.HandleBroadcastAsync(sender, this, result, allHandlers);
        }

        public virtual async Task BroadcastSelectAsync(IEntryModel model,
            Action<IDirectoryNodeViewModel> currentAction)
        {
            switch (model.Profile.HierarchyComparer.CompareHierarchy(CurrentDirectory.EntryModel, model))
            {
                case HierarchicalResult.Current:
                    currentAction(this); 
                    break;
                case HierarchicalResult.Parent:
                    break;
                case HierarchicalResult.Child:
                    await BroadcastChildSelectedAsync(model, currentAction, true,
                        (nvm, evm) =>
                        {
                            var result =
                                model.Profile.HierarchyComparer.CompareHierarchy(nvm.CurrentDirectory.EntryModel, model);
                            return result == HierarchicalResult.Child || result == HierarchicalResult.Current;
                        });
                    break;
                default:
                    break;
            }
        }

        public async Task NotifyChildSelectionChangedAsync(IDirectoryNodeViewModel node, bool selected, Stack<IDirectoryNodeViewModel> path)
        {
            var model = node.CurrentDirectory.EntryModel;
            
            path.Push(this);
            if (ParentNode != null)
                await ParentNode.NotifyChildSelectionChangedAsync(node, selected, path);
            else //Root directory 
                TreeModel.NotifySelectionChanged(path, selected);
        }

        public override bool Equals(object obj)
        {
            return obj is DirectoryNodeViewModel && this.CurrentDirectory != null &&
                this.CurrentDirectory.Equals((obj as IDirectoryNodeViewModel).CurrentDirectory);
        }

        protected void OnExpanded()
        {
            LoadAsync(); //Expand asynchronously.
        }

        protected void OnCollapsed()
        {

        }

        protected void OnSelected()
        {
            NotifyChildSelectionChangedAsync(this, true, 
                    new Stack<IDirectoryNodeViewModel>());
        }

        protected void OnDeselected()
        {
            NotifyChildSelectionChangedAsync(this, false, 
                    new Stack<IDirectoryNodeViewModel>());
        }

        #endregion

        #region Data

        NodeState _state = NodeState.IsCreated;
        string _error = null;
        bool _isSelected = false;
        bool _isExpanded = false, _isExpanded2 = false;
        bool _isChildSelected = false;
        object _selectedChild = null;
        protected bool _expandWhenBroadcastSelect = true;

        IDirectoryNodeViewModel _parentNode;
        IObservableCollection<IDirectoryNodeViewModel> _subdirs = new BindableCollection<IDirectoryNodeViewModel>();

        #endregion

        #region Public Properties

        public IDirectoryTreeViewModel TreeModel { get; private set; }

        public NodeState State
        {
            get { return _state; }
            private set { _state = value; NotifyOfPropertyChange(() => State); }
        }

        public string Error
        {
            get { return _error; }
            private set { _error = value; NotifyOfPropertyChange(() => Error); }
        }

        public IDirectoryNodeViewModel ParentNode
        {
            get { return _parentNode; }
            set { _parentNode = value; }
        }



        public virtual IObservableCollection<IDirectoryNodeViewModel> Subdirectories
        {
            get { return _subdirs; }
            set { _subdirs = value; NotifyOfPropertyChange(() => Subdirectories); }
        }


        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded);
                if (IsExpanded) OnExpanded(); else OnCollapsed();
            }
        }

        public bool IsExpanded2
        {
            get { return _isExpanded2; }
            set
            {
                _isExpanded2 = value;
                NotifyOfPropertyChange(() => IsExpanded2);
                if (IsExpanded2) OnExpanded(); else OnCollapsed();
            }
        }

        public bool IsDummyNode { get { return CurrentDirectory == null; } }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected); if (IsSelected) OnSelected(); else OnDeselected();
                }
            }
        }

        public bool IsChildSelected
        {
            get { return _isChildSelected; }
            set
            {
                _isChildSelected = value;
                NotifyOfPropertyChange(() => IsChildSelected);
            }
        }

        public object SelectedChild
        {
            get { return _selectedChild; }
            set
            {
                _selectedChild = value;
                NotifyOfPropertyChange(() => SelectedChild);
                if (value is DirectoryNodeViewModel)
                    (value as DirectoryNodeViewModel).IsSelected = true;
            }
        }

        #endregion









    }
}
