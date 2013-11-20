using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeNodeSelectionHelper : PropertyChangedBase
    {
        #region Constructor     
   
        public TreeNodeSelectionHelper(object currentValue, TreeSelectionHelper rootSelectionHelper, 
            TreeNodeSelectionHelper parentSelectionHelper, 
            Func<object, Task<TreeNodeSelectionHelper>> findChildFunc, Func<object, object, HierarchicalResult> compareFunc)
        {
            _rootSelectionHelper = rootSelectionHelper;
            _parentSelectionHelper = parentSelectionHelper;
            _currentValue = currentValue;
            _findChildFunc = findChildFunc;
            _compareFunc = compareFunc;
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
        protected void ReportChildSelected(Stack<TreeNodeSelectionHelper> path)
        {
            if (path.Count() > 0)
            {
                _selectedChild = path.Peek()._currentValue;
                NotifyOfPropertyChange(() => IsChildSelected);
                NotifyOfPropertyChange(() => SelectedChild);
            }

            path.Push(this);
            if (_parentSelectionHelper != null)
                _parentSelectionHelper.ReportChildSelected(path);
            else _rootSelectionHelper.ReportChildSelected(path);
        }

        protected void ReportChildDeselected(Stack<TreeNodeSelectionHelper> path)
        {
            path.Push(this);
            if (_parentSelectionHelper != null)
                _parentSelectionHelper.ReportChildDeselected(path);
            else _rootSelectionHelper.ReportChildDeselected(path);
        }

        /// <summary>
        /// Tunnel down to select the specified item.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentAction"></param>
        /// <returns></returns>
        public virtual async Task<TreeNodeSelectionHelper> LookupAsync(object value)
        {
            switch (_compareFunc(_currentValue, value))
            {
                case HierarchicalResult.Current:
                    return this;
                case HierarchicalResult.Parent:
                    break;
                case HierarchicalResult.Child:
                    var childHelper = await _findChildFunc(value); 
                    if (childHelper != null)
                        return await childHelper.LookupAsync(value);
                    break;
                default:
                    break;
            }
            return null;
        }

        public void OnSelected(bool selected)
        {
            
            if (selected)
                ReportChildSelected(new Stack<TreeNodeSelectionHelper>());
            else ReportChildDeselected(new Stack<TreeNodeSelectionHelper>());
        }

        public void OnChildSelected(object newValue)
        {
            if (_selectedChild == null || !_selectedChild.Equals(newValue))
            {
                if (_selectedChild != null) //Deselect previous selection of that combobox.
                {
                    var originalSelected = _findChildFunc(_selectedChild).Result;
                    if (originalSelected != null)
                    {
                        //originalSelected.OnSelected(false);
                        originalSelected.OnChildSelected(null);
                    }
                }

                _selectedChild = newValue;                
                NotifyOfPropertyChange(() => IsSelected);
                NotifyOfPropertyChange(() => SelectedChild);
                NotifyOfPropertyChange(() => IsChildSelected);

                if (newValue != null)
                {
                    var found = _findChildFunc(newValue).Result;
                    if (found == null)
                        Debug.WriteLine(String.Format("findChildFunc failed when looking for {0}", newValue));
                    else found.IsSelected = true;
                }
            }
        }

        #endregion

        #region Data

        object _currentValue = null;
        bool _isSelected = false;
        object _selectedChild = null;
        
        private TreeNodeSelectionHelper _parentSelectionHelper;
        private TreeSelectionHelper _rootSelectionHelper;
        private Func<object, Task<TreeNodeSelectionHelper>> _findChildFunc; //find direct child or path to child.
        private Func<object, object, HierarchicalResult> _compareFunc;

        #endregion

        #region Public Properties

        public object Value { get { return _currentValue; } }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnChildSelected(null);
                    OnSelected(value);
                }
            }
        }

        public virtual bool IsChildSelected
        {
            get { return _selectedChild != null; }
        }

        public object SelectedChild
        {
            get { return _selectedChild; }
            set
            {
                _isSelected = false;
                OnChildSelected(value);
                
            }
        }



        #endregion
    }
}
