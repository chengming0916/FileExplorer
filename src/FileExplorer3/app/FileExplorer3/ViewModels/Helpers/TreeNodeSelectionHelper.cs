using System;
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


    public class TreeNodeSelectionHelper<VM, T> : INotifyPropertyChanged
    {
        #region Constructor

        public TreeNodeSelectionHelper(T currentValue, TreeSelectionHelper<VM, T> rootSelectionHelper,
            TreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            TreeEntryHelper<VM> entryHelper)
        {
            _rootSelectionHelper = rootSelectionHelper;
            _parentSelectionHelper = parentSelectionHelper;
            _entryHelper = entryHelper;
            _currentValue = currentValue;
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
        protected void ReportChildSelected(Stack<TreeNodeSelectionHelper<VM, T>> path)
        {
            if (path.Count() > 0)
            {
                //var lookupResult = AsyncUtils.RunSync(() => this.LookupAsync(path.Last, true));
                _selectedValue = path.Peek().Value;
                NotifyPropertyChanged("IsChildSelected");
                NotifyPropertyChanged("SelectedChild");
            }

            path.Push(this);
            if (_parentSelectionHelper != null)
                _parentSelectionHelper.ReportChildSelected(path);
            else _rootSelectionHelper.ReportChildSelected(path);
        }

        protected void ReportChildDeselected(Stack<TreeNodeSelectionHelper<VM, T>> path)
        {
            if (_entryHelper.IsLoaded)
            {
                var lookupResult = AsyncUtils.RunSync(() => this.LookupAsync(_rootSelectionHelper.SelectedValue, true));
                _selectedValue = lookupResult == null ? default(T) : lookupResult.Value;
                NotifyPropertyChanged("IsChildSelected");
                NotifyPropertyChanged("SelectedChild");
            }
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
        public async Task<TreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false)
        {
            foreach (var current in await _entryHelper.LoadAsync())
            {
                var currentSelectionHelper = _rootSelectionHelper.GetSelectionHelperFunc(current);
                switch (_rootSelectionHelper.CompareFunc(currentSelectionHelper.Value, value))
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


        public void OnSelected(bool selected)
        {
            if (selected)
                ReportChildSelected(new Stack<TreeNodeSelectionHelper<VM, T>>());
            else ReportChildDeselected(new Stack<TreeNodeSelectionHelper<VM, T>>());
        }



        public void OnChildSelected(T newValue)
        {
            if (_selectedValue == null || !_selectedValue.Equals(newValue))
            {
                if (_selectedValue != null) //Deselect previous selection of that combobox.
                {
                    var originalSelected = LookupAsync(_selectedValue, true).Result;
                    if (originalSelected != null)
                    {
                        originalSelected.OnSelected(false);
                        //originalSelected._isSelected = false;
                        //NotifyPropertyChanged("IsSelected");
                        originalSelected.OnChildSelected(default(T));
                    }
                }

                _selectedValue = newValue;
                NotifyPropertyChanged("SelectedChild");
                NotifyPropertyChanged("IsChildSelected");

                if (newValue != null)
                {
                    var found = LookupAsync(newValue, true).Result;
                    if (found == null)
                        Debug.WriteLine(String.Format("findChildFunc failed when looking for {0}", newValue));
                    else found.IsSelected = true;
                }
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Data

        T _currentValue = default(T);
        bool _isSelected = false;
        T _selectedValue = default(T);

        private TreeNodeSelectionHelper<VM, T> _parentSelectionHelper;
        private TreeSelectionHelper<VM, T> _rootSelectionHelper;
        private TreeEntryHelper<VM> _entryHelper;

        #endregion

        #region Public Properties
        public event PropertyChangedEventHandler PropertyChanged;

        public T Value { get { return _currentValue; } }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value || value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                    OnSelected(value);

                    //If current item is selected, Clear it's ChildSelection Item.
                    //if (value && _rootSelectionHelper.CompareFunc(Value, _rootSelectionHelper.SelectedValue) 
                    //    == HierarchicalResult.Unrelated)
                     //   OnChildSelected(default(T));

                    //if (!value && _rootSelectionHelper.CompareFunc(Value, _rootSelectionHelper.SelectedValue) 
                    //    == HierarchicalResult.Child)
                    //    OnChildSelected(default(T));
                }
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
                _isSelected = false;
                NotifyPropertyChanged("IsSelected");
                OnChildSelected(value);

            }
        }



        #endregion


    }
}
