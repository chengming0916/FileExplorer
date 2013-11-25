using System;
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
            VM _prevSelectedViewModel = _selectedViewModel;
            T _prevSelectedValue = _selectedValue;
            _prevPath = path;

            _selectedViewModel = path.Last().ViewModel;
            _selectedValue = path.Last().Value;
            if (_prevSelectedValue != null && !_prevSelectedValue.Equals(path.Last().Value))
            {            
                (_prevSelectedViewModel as ISupportTreeSelector<VM, T>).Selection.IsSelected = false;
            }
            NotifyOfPropertyChanged(() => SelectedValue);
            NotifyOfPropertyChanged(() => SelectedViewModel);
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);

            UpdateRootItems(path);
        }

        public override void ReportChildDeselected(Stack<ITreeSelector<VM, T>> path)
        {
        }

        private void UpdateRootItems(Stack<ITreeSelector<VM, T>> path = null)
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

     

        public async Task<ITreeSelector<VM, T>> LookupAsync(T value, ITreeLookup<VM, T> lookupProc,
            params ITreeProcessor<VM, T>[] processors)
        {

            foreach (var current in await EntryHelper.LoadAsync())
            {
                var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                var compareResult = _compareFunc(currentSelectionHelper.Value, value);

                if (compareResult == HierarchicalResult.Child || compareResult == HierarchicalResult.Current)
                    if (processors.Process(compareResult, this, currentSelectionHelper))
                        switch (compareResult)
                        {
                            case HierarchicalResult.Child:
                                if (lookupProc is SearchNextLevelOnly<VM, T>)
                                    return currentSelectionHelper;
                                return await currentSelectionHelper.LookupAsync(value, lookupProc, processors);
                            case HierarchicalResult.Current:
                                return currentSelectionHelper;
                        }
            }
            return null;
        }



        public async Task SelectAsync(T value)
        {
            if (_selectedValue == null || _compareFunc(_selectedValue, value) != HierarchicalResult.Current)
            {
                await LookupAsync(value, RecrusiveSearchUntilFound<VM, T>.Instance,
                    SetSelected<VM, T>.Instance, SetChildSelected<VM, T>.Instance);
            }
        }

        public async Task SelectAsync(T value, ITreeLookup<VM, T> lookupProc,
            params ITreeProcessor<VM, T>[] processors)
        {
            if (_selectedValue == null || _compareFunc(_selectedValue, value) != HierarchicalResult.Current)
            {
                await LookupAsync(value, lookupProc, processors);
            }
        }

        #endregion

        #region Data

        T _selectedValue = default(T);
        VM _selectedViewModel = default(VM);
        Stack<ITreeSelector<VM, T>> _prevPath = null;
        private Func<T, T, HierarchicalResult> _compareFunc;        
        private ObservableCollection<VM> _rootItems = null;

        #endregion

        #region Public Properties

        public event EventHandler SelectionChanged;

        public ObservableCollection<VM> OverflowedAndRootItems
        {
            get { if (_rootItems == null) UpdateRootItems(); return _rootItems; }
            set { _rootItems = value; NotifyOfPropertyChanged(() => OverflowedAndRootItems); }
        }

        public VM SelectedViewModel
        {
            get { return _selectedViewModel; }
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
