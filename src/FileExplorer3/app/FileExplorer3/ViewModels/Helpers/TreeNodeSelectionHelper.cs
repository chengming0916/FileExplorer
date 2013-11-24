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


    public class TreeNodeSelectionHelper<VM, T> : NotifyPropertyChanged, ITreeNodeSelectionHelper<VM, T>
    {
        #region Constructor

        protected TreeNodeSelectionHelper(ISubEntriesHelper<VM> entryHelper, Func<T, T, HierarchicalResult> compareFunc)
        {
            _entryHelper = entryHelper;
            _compareFunc = compareFunc;
        }

        public TreeNodeSelectionHelper(T currentValue, VM currentViewModel,
            ITreeNodeSelectionHelper<VM, T> rootSelectionHelper,
            ITreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            ISubEntriesHelper<VM> entryHelper)
        {
            _rootSelectionHelper = rootSelectionHelper;
            _parentSelectionHelper = parentSelectionHelper;
            _entryHelper = entryHelper;
            _currentValue = currentValue;
            _currentViewModel = currentViewModel;
            _compareFunc = parentSelectionHelper.CompareFunc;
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
        public virtual void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path)
        {
            //if (path.Count() > 0)
            //{
            //    //var lookupResult = AsyncUtils.RunSync(() => this.LookupAsync(path.Last, true));
            //    _selectedValue = path.Peek().Value;
            //    NotifyOfPropertyChanged(() => IsChildSelected);
            //    NotifyOfPropertyChanged(() => SelectedChild);
            //}

            path.Push(this);
            if (_parentSelectionHelper != null)
                _parentSelectionHelper.ReportChildSelected(path);
            else throw new Exception();
            //else _rootSelectionHelper.ReportChildSelected(path);
        }

        public virtual void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path)
        {
            if (_entryHelper.IsLoaded)
            {
                var lookupResult =
                    _rootSelectionHelper.SelectedValue == null ? null :
                    AsyncUtils.RunSync(() => this.LookupAsync(_rootSelectionHelper.SelectedValue,
                        new SearchNextUsingReverseLookup<VM, T>(_rootSelectionHelper.SelectedViewModel)));
                SetSelectedChild(lookupResult == null ? default(T) : lookupResult.Value);
                NotifyOfPropertyChanged(() => IsChildSelected);
                NotifyOfPropertyChanged(() => SelectedChild);
            }
            path.Push(this);
            if (_parentSelectionHelper != null)
                _parentSelectionHelper.ReportChildDeselected(path);
            else throw new Exception();
            //else _rootSelectionHelper.ReportChildDeselected(path);
        }

        /// <summary>
        /// Tunnel down to select the specified item.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentAction"></param>
        /// <returns></returns>
        public async Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value,
            ITreeSelectionLookup<VM, T> lookupProc,
            params ITreeSelectionProcessor<VM, T>[] processors)
        {
            return await lookupProc.Lookup(value, this, _compareFunc, processors);
        }

        public async Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value,
            bool nextNodeOnly)
        {
            return await LookupAsync(value, SearchNextLevelOnly<VM, T>.Instance);
        }

        public void SetIsSelected(bool value)
        {
            _isSelected = value;
            NotifyOfPropertyChanged(() => IsSelected);
        }

        public void OnSelected(bool selected)
        {
            if (selected)
                ReportChildSelected(new Stack<ITreeNodeSelectionHelper<VM, T>>());
            else ReportChildDeselected(new Stack<ITreeNodeSelectionHelper<VM, T>>());
        }

        public void SetSelectedChild(T newValue)
        {
            Debug.WriteLine(String.Format("SetSelectedChild of {0} to {1}", this.Value, newValue));

            if (newValue == null && this._entryHelper.IsLoaded && _selectedValue != null)
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
                        _prevSelected.SetIsSelected(true);
                }
            }
        }



        #endregion

        #region Data

        T _currentValue = default(T);
        bool _isSelected = false;
        T _selectedValue = default(T);
        ITreeNodeSelectionHelper<VM, T> _rootSelectionHelper = null;
        ITreeNodeSelectionHelper<VM, T> _prevSelected = null;

        private ITreeNodeSelectionHelper<VM, T> _parentSelectionHelper;
        //private ITreeSelectionHelper<VM, T> _rootSelectionHelper;
        private ISubEntriesHelper<VM> _entryHelper;

        #endregion

        #region Public Properties
        private VM _currentViewModel;
        private Func<T, T, HierarchicalResult> _compareFunc;

        public T Value { get { return _currentValue; } }
        public VM ViewModel { get { return _currentViewModel; } }

        public ISubEntriesHelper<VM> Entries { get { return _entryHelper; } }

        public ITreeNodeSelectionHelper<VM, T> ParentSelectionHelper { get { return _parentSelectionHelper; } }

        public Func<T, T, HierarchicalResult> CompareFunc { get { return _compareFunc; } }

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
