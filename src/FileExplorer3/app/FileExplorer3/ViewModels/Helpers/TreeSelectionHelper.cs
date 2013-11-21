using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeSelectionHelper<VM, T> : NotifyPropertyChanged, ITreeSelectionHelper<VM, T>
    {
        #region Constructor

        public TreeSelectionHelper(ISubEntriesHelper<VM> entryHelper,
            Func<T, T, HierarchicalResult> compareFunc, Func<VM, ITreeNodeSelectionHelper<VM,T>> getSelectionHelperFunc)
        {
            _entryHelper = entryHelper;
            _compareFunc = compareFunc;
            _getSelectionHelperFunc = getSelectionHelperFunc;
        }

        #endregion

        #region Methods

        public async void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path)
        {
            T _prevSelectedValue = _selectedValue;

            _selectedViewModel = path.Last().ViewModel;
            _selectedValue = path.Last().Value;
            if (_prevSelectedValue != null && !_prevSelectedValue.Equals(path.Last().Value))
            {
                var found = await LookupAsync(_prevSelectedValue);
                if (found != null)
                    found.IsSelected = false;
            }
            NotifyOfPropertyChanged(() => SelectedValue);
            NotifyOfPropertyChanged(() => SelectedViewModel);
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        public async void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path)
        {

        }

        public async Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false)
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
        VM _selectedViewModel = default(VM);
        //object _owner = null;
        //Func<object, Task<TreeNodeSelectionHelper<VM, T>>> _findChildFunc;
        private Func<T, T, HierarchicalResult> _compareFunc;
        private ISubEntriesHelper<VM> _entryHelper;
        private Func<VM, ITreeNodeSelectionHelper<VM, T>> _getSelectionHelperFunc;
        
        #endregion

        #region Public Properties

        public event EventHandler SelectionChanged;

        public VM SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { SelectAsync(_getSelectionHelperFunc(_selectedViewModel).Value); }
        }

        public T SelectedValue
        {
            get { return _selectedValue; }
            set { SelectAsync(value); }
        }        

        public Func<T, T, HierarchicalResult> CompareFunc { get { return _compareFunc; } }

        public Func<VM, ITreeNodeSelectionHelper<VM, T>> GetSelectionHelperFunc { get { return _getSelectionHelperFunc; } }


        #endregion

        
    }
}
