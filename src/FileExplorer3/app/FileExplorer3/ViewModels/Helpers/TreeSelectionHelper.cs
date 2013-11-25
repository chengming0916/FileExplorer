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
    public class TreeSelectionHelper<VM, T> : NotifyPropertyChanged, ITreeRootSelector<VM, T>
    {
        #region Constructor

        public TreeSelectionHelper(ISubEntriesHelper<VM> entryHelper,
            Func<T, T, HierarchicalResult> compareFunc)
        {
            _entryHelper = entryHelper;
            _compareFunc = compareFunc;
        }

        #endregion

        #region Methods

        public async void ReportChildSelected(Stack<ITreeSelector<VM, T>> path)
        {
            VM _prevSelectedViewModel = _selectedViewModel;
            T _prevSelectedValue = _selectedValue;

            //if (_prevPath != null && !_prevPath.Last().IsSelected)
            //    _prevPath.Last().IsSelected = false;
            _prevPath = path;


            _selectedViewModel = path.Last().ViewModel;
            _selectedValue = path.Last().Value;
            if (_prevSelectedValue != null && !_prevSelectedValue.Equals(path.Last().Value))
            {
                //var found = await LookupAsync(_prevSelectedValue,
                //    RecrusiveBroadcastIfLoaded<VM, T>.Instance, SetNotSelected<VM, T>.WhenCurrent,
                //    SetChildNotSelected<VM, T>.WhenChild);
                (_prevSelectedViewModel as ISupportSelectionHelper<VM, T>).Selection.IsSelected = false;
            }

            //if (_prevPath != null)
            //    foreach (var p in _prevPath)
            //    {
            //        var lookupResult =SelectedValue == null ? null :
            //                    AsyncUtils.RunSync(() => p.LookupAsync(SelectedValue, true));
            //        p.SetSelectedChild(lookupResult == null ? default(T) : lookupResult.Value);

            //    }
            //_prevPath = null;

            //if (_prevPath != null)
            //{
            //        if (!path.Contains(p))
            //        {
            //            p.SetIsSelected(false);
            //            p.SetSelectedChild(default(T));
            //        }
            //}


            NotifyOfPropertyChanged(() => SelectedValue);
            NotifyOfPropertyChanged(() => SelectedViewModel);
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);

            UpdateRootItems(path);
        }

        private void UpdateRootItems(Stack<ITreeSelector<VM, T>> path = null)
        {
            if (_rootItems == null)
                _rootItems = new ObservableCollection<VM>();
            else _rootItems.Clear();
            if (path != null && path.Count() > 0)
            {
                foreach (var p in path.Reverse())
                    if (!(this._entryHelper.AllNonBindable.Contains(p.ViewModel)))
                        _rootItems.Add(p.ViewModel);
                _rootItems.Add(default(VM)); //Separator
            }
            foreach (var e in this._entryHelper.AllNonBindable)
                _rootItems.Add(e);
        }

        public async void ReportChildDeselected(Stack<ITreeSelector<VM, T>> path)
        {

            //Debug.WriteLine(path);
        }

        public async Task<ITreeSelector<VM, T>> LookupAsync(T value, ITreeSelectionLookup<VM, T> lookupProc,
            params ITreeSelectionProcessor<VM, T>[] processors)
        {

            foreach (var current in await _entryHelper.LoadAsync())
            {
                var currentSelectionHelper = (current as ISupportSelectionHelper<VM, T>).Selection;
                var compareResult = _compareFunc(currentSelectionHelper.Value, value);

                if (compareResult == HierarchicalResult.Child || compareResult == HierarchicalResult.Current)
                    if (processors.Process(compareResult, default(VM), current))
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

        public async Task SelectAsync(T value, ITreeSelectionLookup<VM, T> lookupProc,
            params ITreeSelectionProcessor<VM, T>[] processors)
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
        private ISubEntriesHelper<VM> _entryHelper;
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




        public Task<ITreeSelector<VM, T>> LookupAsync(T value, bool nextNode = false)
        {
            throw new NotImplementedException();
        }

        public bool IsSelected
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsChildSelected
        {
            get { throw new NotImplementedException(); }
        }

        public T SelectedChild
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetIsSelected(bool value)
        {
            throw new NotImplementedException();
        }

        public void SetSelectedChild(T value)
        {
            throw new NotImplementedException();
        }

        public VM ViewModel
        {
            get { throw new NotImplementedException(); }
        }

        public T Value
        {
            get { throw new NotImplementedException(); }
        }

        public ITreeSelector<VM, T> ParentSelectionHelper
        {
            get { throw new NotImplementedException(); }
        }
    }
}
