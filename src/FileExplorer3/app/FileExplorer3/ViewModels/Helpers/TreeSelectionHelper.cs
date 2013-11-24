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
    public class TreeSelectionHelper<VM, T> : TreeNodeSelectionHelper<VM, T>, ITreeSelectionHelper<VM, T>
    {
        #region Constructor

        public TreeSelectionHelper(ISubEntriesHelper<VM> entryHelper,
            Func<T, T, HierarchicalResult> compareFunc)
            : base(entryHelper, compareFunc)
        {
            _entryHelper = entryHelper;
            _compareFunc = compareFunc;
            OverflowedAndRootItems = new ObservableCollection<VM>();
        }

        #endregion

        #region Methods

        public override void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path)
        {
            VM _prevSelectedViewModel = _selectedViewModel;
            T _prevSelectedValue = _selectedValue;

            _selectedViewModel = path.Last().ViewModel;
            _selectedValue = path.Last().Value;



            //if (_prevPath != null)
            //    foreach (var p in _prevPath)
            //    {
            //        if (!(path.Contains(p)))
            //        {
            //            p.SetSelectedChild(default(T));
            //        }
            //        p.SetIsSelected(p.Value.Equals(_selectedValue));                    
            //    }
            //_prevPath = path;
            //path.Last().SetSelectedChild(default(T));

            if (_prevSelectedValue != null && !_prevSelectedValue.Equals(path.Last().Value))
            {
              

                //AsyncUtils.RunSync(() => LookupAsync(_prevSelectedValue,
                //    new ReceusiveSearchUsingReverseLookup<VM, T>(_prevPath),
                //    SetChildNotSelected<VM, T>.WhenChild));
                //AsyncUtils.RunSync(() => LookupAsync(_selectedValue,
                //    new ReceusiveSearchUsingReverseLookup<VM, T>(path),
                //    SetChildSelected<VM, T>.Instance));
                //var found = await LookupAsync(_prevSelectedValue,
                //    RecrusiveBroadcastIfLoaded<VM, T>.Instance, SetNotSelected<VM, T>.WhenCurrent,
                //    SetChildNotSelected<VM, T>.WhenChild);
                (_prevSelectedViewModel as ISupportNodeSelectionHelper<VM, T>).Selection.IsSelected = false;
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

            OverflowedAndRootItems.Clear();
            foreach (var p in path.Reverse())
                OverflowedAndRootItems.Add(p.ViewModel);
        }

        public override void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path)
        {
           

            //Debug.WriteLine(path);
        }

        //public async Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, ITreeSelectionLookup<VM, T> lookupProc,
        //    params ITreeSelectionProcessor<VM, T>[] processors)
        //{

        //    foreach (var current in await _entryHelper.LoadAsync())
        //    {
        //        var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;
        //        var compareResult = _compareFunc(currentSelectionHelper.Value, value);

        //        if (compareResult == HierarchicalResult.Child || compareResult == HierarchicalResult.Current)
        //            if (processors.Process(compareResult, default(VM), current))
        //                switch (compareResult)
        //                {
        //                    case HierarchicalResult.Child:
        //                        if (lookupProc is SearchNextLevelOnly<VM, T>)
        //                            return currentSelectionHelper;
        //                        return await currentSelectionHelper.LookupAsync(value, lookupProc, processors);
        //                    case HierarchicalResult.Current:
        //                        return currentSelectionHelper;
        //                }
        //    }
        //    return null;
        //}



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
        Stack<ITreeNodeSelectionHelper<VM, T>> _prevPath = null;
        private Func<T, T, HierarchicalResult> _compareFunc;
        private ISubEntriesHelper<VM> _entryHelper;
        private ObservableCollection<VM> _rootItems;

        #endregion

        #region Public Properties

        public event EventHandler SelectionChanged;

        public ObservableCollection<VM> OverflowedAndRootItems
        {
            get { return _rootItems; }
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
