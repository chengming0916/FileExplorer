﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Utils;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{


    public class TreeSelector<VM, T> : NotifyPropertyChanged, ITreeSelector<VM, T>
    {
        #region Constructor

        protected TreeSelector(IEntriesHelper<VM> entryHelper)
        {
            EntryHelper = entryHelper;
            RootSelector = this as ITreeRootSelector<VM, T>;
        }

        public TreeSelector(T currentValue, VM currentViewModel, 
            ITreeSelector<VM, T> parentSelector,
            IEntriesHelper<VM> entryHelper)
        {
            RootSelector = parentSelector.RootSelector;
            ParentSelector = parentSelector;
            EntryHelper = entryHelper;
            _currentValue = currentValue;
            _currentViewModel = currentViewModel;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return _currentValue.ToString();
        }

        /// <summary>
        /// Bubble up to TreeSelectionHelper for selection.
        /// </summary>
        /// <param name="path"></param>
        public virtual void ReportChildSelected(Stack<ITreeSelector<VM, T>> path)
        {
            if (path.Count() > 0)
            {
                _selectedValue = path.Peek().Value;
                NotifyOfPropertyChanged(() => IsChildSelected);
                NotifyOfPropertyChanged(() => SelectedChild);
            }

            path.Push(this);
            if (ParentSelector != null)
                ParentSelector.ReportChildSelected(path);
        }

        public virtual void ReportChildDeselected(Stack<ITreeSelector<VM, T>> path)
        {
            if (EntryHelper.IsLoaded)
            {
                //Clear child node selection.
                SetSelectedChild(default(T));
                //And just in case if the new selected value is child of this node.
                if (RootSelector.SelectedValue != null) 
                    this.LookupAsync(RootSelector.SelectedValue,
                        new SearchNextUsingReverseLookup<VM, T>(RootSelector.SelectedViewModel),
                        new TreeSelectionProcessor<VM, T>(HierarchicalResult.All, (hr, p, c) =>
                            {
                                SetSelectedChild(c == null ? default(T) : (c as ISupportTreeSelector<VM, T>).Selection.Value);
                                return true;
                            })
                        );
                //SetSelectedChild(lookupResult == null ? default(T) : lookupResult.Value);
                NotifyOfPropertyChanged(() => IsChildSelected);
                NotifyOfPropertyChanged(() => SelectedChild);
            }
            path.Push(this);
            if (ParentSelector != null)
                ParentSelector.ReportChildDeselected(path);
        }

        /// <summary>
        /// Tunnel down to select the specified item.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentAction"></param>
        /// <returns></returns>
        public async Task<ITreeSelector<VM, T>> LookupAsync(T value,
            ITreeSelectionLookup<VM, T> lookupProc,
            params ITreeSelectionProcessor<VM, T>[] processors)
        {
            return await lookupProc.Lookup(value, this.ViewModel, RootSelector.CompareFunc, processors);
        }

        public async Task<ITreeSelector<VM, T>> LookupAsync(T value,
            bool nextNodeOnly)
        {
            return await LookupAsync(value, SearchNextLevelOnly<VM, T>.Instance);
        }

        public void SetIsSelected(bool value)
        {
            _isSelected = value;
            NotifyOfPropertyChanged(() => IsSelected);
            SetSelectedChild(default(T));
        }

        public void OnSelected(bool selected)
        {
            if (selected)
                ReportChildSelected(new Stack<ITreeSelector<VM, T>>());
            else ReportChildDeselected(new Stack<ITreeSelector<VM, T>>());
        }

        public void SetSelectedChild(T newValue)
        {
            Debug.WriteLine(String.Format("SetSelectedChild of {0} to {1}", this.Value, newValue));
            
            if (newValue == null && this.EntryHelper.IsLoaded && _selectedValue != null)
            {
                //foreach (var node in _entryHelper.AllNonBindable)
                //    if ((node as ISupportNodeSelectionHelper<VM, T>).Selection.IsChildSelected)
                //        (node as ISupportNodeSelectionHelper<VM, T>).Selection.SelectedChild = default(T);
                //var selectedNode = AsyncUtils.RunSync(() => this.LookupAsync(_selectedValue, true));
                //if (selectedNode != null && selectedNode.IsChildSelected)
                //    selectedNode.SetSelectedChild(default(T));
            }

            _selectedValue = newValue;
            
            NotifyOfPropertyChanged(() => SelectedChild);
            NotifyOfPropertyChanged(() => IsChildSelected);
        }

        public void OnChildSelected(T newValue)
        {
            if (_selectedValue == null || !_selectedValue.Equals(newValue))
            {
                if (_prevSelected != null)
                {
                    _prevSelected.SetIsSelected(false);                    
                }

                SetSelectedChild(newValue);

                if (newValue != null)
                {
                    _prevSelected = LookupAsync(newValue, true).Result;
                    if (_prevSelected == null)
                        Debug.WriteLine(String.Format("findChildFunc failed when looking for {0}", newValue));
                    else
                        _prevSelected.IsSelected = true;
                }
            }
        }



        #endregion

        #region Data

        T _currentValue = default(T);
        bool _isSelected = false;
        T _selectedValue = default(T);
        ITreeSelector<VM, T> _prevSelected = null;
        
        

        #endregion

        #region Public Properties
        private VM _currentViewModel;

        public T Value { get { return _currentValue; } }
        public VM ViewModel { get { return _currentViewModel; } }

        public ITreeSelector<VM, T> ParentSelector { get; private set; }
        public ITreeRootSelector<VM, T> RootSelector { get; private set; }
        public IEntriesHelper<VM> EntryHelper { get; private set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                //if (_isSelected != value || value)
                //{
                    SetIsSelected(value);
                    OnSelected(value);

                //}
            }
        }

        public virtual bool IsChildSelected
        {
            get { return _selectedValue != null; }
        }

        public T SelectedChild
        {
            get { return _selectedValue; }
            set
            {
                SetIsSelected(false);
                NotifyOfPropertyChanged(() => IsSelected);
                OnChildSelected(value);
            }
        }



        #endregion


    }
}
