using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeSelectionHelper<VM,T> : PropertyChangedBase
    {
        #region Constructor

        public TreeSelectionHelper(TreeEntryHelper<VM> entryHelper,
            Func<T, T, HierarchicalResult> compareFunc, Func<VM, TreeNodeSelectionHelper<VM,T>> getSelectionHelperFunc)
        {
            _entryHelper = entryHelper;
            _compareFunc = compareFunc;
            _getSelectionHelperFunc = getSelectionHelperFunc;
        }

        #endregion

        #region Methods

        internal async void ReportChildSelected(Stack<TreeNodeSelectionHelper<VM, T>> path)
        {
            T _prevSelectedValue = _selectedValue;           
            _selectedValue = path.Last().Value;
            if (_prevSelectedValue != null && !_prevSelectedValue.Equals(path.Last().Value))
            {
                var found = await LookupAsync(_prevSelectedValue);
                if (found != null)
                    found.IsSelected = false;
            }
            NotifyOfPropertyChange(() => SelectedValue);
        }

        internal void ReportChildDeselected(Stack<TreeNodeSelectionHelper<VM, T>> path)
        {

        }

        public async Task<TreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false)
        {
            foreach (var current in await _entryHelper.LoadAsync())
            {
                var currentSelectionHelper = _getSelectionHelperFunc(current);
                switch (_compareFunc(currentSelectionHelper.Value, value))
                {
                    case HierarchicalResult.Child:
                        if (nextNodeOnly)
                            return currentSelectionHelper;
                        else return await currentSelectionHelper.LookupAsync(value, nextNodeOnly);                        
                    case HierarchicalResult.Current:
                        return currentSelectionHelper;
                }
            }
            return null;
        }

        public async Task SelectAsync(T value)
        {
            var found = await LookupAsync(value);
            if (found != null)
                found.IsSelected = true;
        }

        #endregion

        #region Data

        T _selectedValue = default(T);
        object _owner = null;
        Func<object, Task<TreeNodeSelectionHelper<VM, T>>> _findChildFunc;
        private Func<T, T, HierarchicalResult> _compareFunc;
        private TreeEntryHelper<VM> _entryHelper;
        private Func<VM, TreeNodeSelectionHelper<VM, T>> _getSelectionHelperFunc;


        #endregion

        #region Public Properties


        public T SelectedValue
        {
            get { return _selectedValue; }
            set { SelectAsync(value); }
        }
        //public IEnumerable<TreeNodeSelectionHelper> RootItems { get; set; }

        public Func<T, T, HierarchicalResult> CompareFunc { get { return _compareFunc; } }

        public Func<VM, TreeNodeSelectionHelper<VM, T>> GetSelectionHelperFunc { get { return _getSelectionHelperFunc; } }


        #endregion
    }
}
