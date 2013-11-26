﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core.Utils;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeRootSelector<VM, T> : TreeSelector<VM,T>, ITreeRootSelector<VM, T>
    {
        #region Constructor

        public TreeRootSelector(IEntriesHelper<VM> entryHelper,
            Func<T, T, HierarchicalResult> compareFunc)
            : base(entryHelper)
        {
            
            _compareFunc = compareFunc;
        }

        #endregion

        #region Methods

        public override void ReportChildSelected(Stack<ITreeSelector<VM, T>> path)
        {
            ITreeSelector<VM, T> _prevSelector = _selectedSelector;
            T _prevSelectedValue = _selectedValue;
            _prevPath = path;

            _selectedSelector = path.Last();
            _selectedValue = path.Last().Value;
            if (_prevSelectedValue != null && !_prevSelectedValue.Equals(path.Last().Value))
            {
                _prevSelector.IsSelected = false;
            }
            NotifyOfPropertyChanged(() => SelectedValue);
            NotifyOfPropertyChanged(() => SelectedViewModel);
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);

            updateRootItems(path);
        }

        public override void ReportChildDeselected(Stack<ITreeSelector<VM, T>> path)
        {
        }

        private void updateRootItems(Stack<ITreeSelector<VM, T>> path = null)
        {
            if (_rootItems == null)
                _rootItems = new ObservableCollection<VM>();
            else _rootItems.Clear();
            if (path != null && path.Count() > 0)
            {
                foreach (var p in path.Reverse())
                    if (!(this.EntryHelper.AllNonBindable.Contains(p.ViewModel)))
                        _rootItems.Add(p.ViewModel);
                _rootItems.Add(default(VM)); //Separator
            }
            foreach (var e in this.EntryHelper.AllNonBindable)
                _rootItems.Add(e);
        }
     
        public async Task SelectAsync(T value)
        {
            if (_selectedValue == null || _compareFunc(_selectedValue, value) != HierarchicalResult.Current)
            {
                await LookupAsync(value, RecrusiveSearch<VM, T>.LoadSubentriesIfNotLoaded,
                    SetSelected<VM, T>.WhenSelected, SetChildSelected<VM, T>.ToSelectedChild);
            }
        }      

        #endregion

        #region Data

        T _selectedValue = default(T);
        ITreeSelector<VM, T> _selectedSelector;
        Stack<ITreeSelector<VM, T>> _prevPath = null;
        private Func<T, T, HierarchicalResult> _compareFunc;        
        private ObservableCollection<VM> _rootItems = null;

        #endregion

        #region Public Properties

        public event EventHandler SelectionChanged;

        public ObservableCollection<VM> OverflowedAndRootItems
        {
            get { if (_rootItems == null) updateRootItems(); return _rootItems; }
            set { _rootItems = value; NotifyOfPropertyChanged(() => OverflowedAndRootItems); }
        }

        public ITreeSelector<VM, T> SelectedSelector
        {
            get { return _selectedSelector; }
        }

        public VM SelectedViewModel
        {
            get { return (SelectedSelector == null ? default(VM) : SelectedSelector.ViewModel); }
        }

        public T SelectedValue
        {
            get { return _selectedValue; }
            set { SelectAsync(value); }
        }

        public Func<T, T, HierarchicalResult> CompareFunc { get { return _compareFunc; } }

        #endregion




    }
}
