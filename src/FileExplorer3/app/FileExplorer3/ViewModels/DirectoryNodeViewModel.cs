﻿using System;
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
            : base(null, null)
        {

        }

        public DirectoryNodeViewModel(IEventAggregator events, IDirectoryTreeViewModel rootModel, IEntryModel curDirModel, IProfile profile)
            : base(events, profile)
        {
            TreeModel = rootModel;
            _curDirViewModel = EntryViewModel.FromEntryModel(profile, curDirModel);

            this.ColumnList =
                new ColumnInfo[] { 
                    ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", 200) };

            //this.ColumnFilters =
            //    new ColumnFilter[] { 
            //        ColumnFilter.CreateNew("DirectoryOnly", "EntryModel.Label", em => em.IsDirectory) };

            this.SortBy = "EntryModel.Label";
            this.SortDirection = ListSortDirection.Ascending;

            this.Subdirectories.Add(DirectoryNodeViewModel.DummyNode);
        }

     

        #endregion

        #region Methods

        public IEnumerable<IResult> Load(bool force = false)
        {
            if (State != NodeState.IsLoading)
            {
                if (State == NodeState.IsLoaded && !force)
                    yield break;
                yield return new DoSomething((c) => Subdirectories.Clear());
                yield return new DoSomething((c) => { State = NodeState.IsLoading; });
                foreach (var iresult in base.Load(CurrentDirectory.EntryModel, em => em.IsDirectory))
                {
                    if (iresult is AppendEntryList)
                    {
                        yield return new AppendDirectoryTree(this);
                        break;
                    }
                    yield return iresult;
                }
                yield return new DoSomething((c) => { 
                    State = NodeState.IsLoaded; });
            }
            else
            {
                //If is loading... wait till state changed.
                yield return new WaitTilPropertyChanged<NodeState>(this, () => State);

            }
        }        

        public async Task LoadAsync()
        {
            var task = Load().ExecuteAsync();
            await task;
            if (task.Exception != null)
            {
                State = NodeState.IsError;
                Error = task.Exception.Message;
            }
        }

        public override string ToString()
        {
            return "node-" + CurrentDirectory.ToString();
        }

        public IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel)
        {
            return new DirectoryNodeViewModel(Events, TreeModel, entryModel, Profile);
        }
        
        public async Task BroadcastSelectAsync(IEntryModel model, Action<IDirectoryNodeViewModel> action)
        {
            
            switch (Profile.HierarchyComparer.CompareHierarchy(CurrentDirectory.EntryModel, model))
            {
                case HierarchicalResult.Current: 
                    action(this); break;
                case HierarchicalResult.Parent :
                    if (Debugger.IsAttached)
                        Debugger.Break(); 
                    break;
                case HierarchicalResult.Child :
                    ActionExecutionContext context = new ActionExecutionContext();
                    await Load().Append(
                        new DoSomething((c) => { this.IsExpanded = true; }),
                        new FindMatched<IDirectoryNodeViewModel>(model, this.Subdirectories,
                            (nvm, evm) => 
                                { 
                                    var result = 
                                        Profile.HierarchyComparer.CompareHierarchy(nvm.CurrentDirectory.EntryModel, model);
                                    return result == HierarchicalResult.Child || result == HierarchicalResult.Current;
                                })
                        ).ExecuteAsync(context);

                    if (context["MatchedItem"] is IDirectoryNodeViewModel)
                        await (context["MatchedItem"] as IDirectoryNodeViewModel)
                            .BroadcastSelectAsync(model, action);
                    break;
            }
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
            TreeModel.NotifySelected(this);
        }

        protected void OnDeselected()
        {

        }

        #endregion

        #region Data

        NodeState _state = NodeState.IsCreated;
        string _error = null;
        bool _isSelected = false;
        bool _isExpanded = false;
        IEntryViewModel _curDirViewModel;
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

        public IEntryViewModel CurrentDirectory { get { return _curDirViewModel; } }
        public IObservableCollection<IDirectoryNodeViewModel> Subdirectories
        {
            get { return _subdirs; }
        }


        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded); if (IsExpanded) OnExpanded(); else OnCollapsed();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected); if (IsSelected) OnSelected(); else OnDeselected();
            }
        }

        #endregion








        
    }
}
