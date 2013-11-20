using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeSelectionHelper : PropertyChangedBase
    {
        #region Constructor

        public TreeSelectionHelper(Func<object, Task<TreeNodeSelectionHelper>> findChildFunc)
        {
            _findChildFunc = findChildFunc;
        }

        #endregion

        #region Methods

        internal async void ReportChildSelected(Stack<TreeNodeSelectionHelper> path)
        {
            if (_selectedValue != null)
                await changeSelectionAsync(_selectedValue, false);
            _selectedValue = path.Last().Value;            
            NotifyOfPropertyChange(() => SelectedValue);
        }

        internal void ReportChildDeselected(Stack<TreeNodeSelectionHelper> path)
        {

        }

        public async Task changeSelectionAsync(object value, bool select)
        {
            var foundPath = await _findChildFunc(value);
            if (foundPath != null)
            {
                var found = await foundPath.LookupAsync(value);
                if (found != null)
                {
                    found.IsSelected = select;
                    return;
                }
            }
        }

        public async Task SelectAsync(object value)
        {
            changeSelectionAsync(value, true);
        }

        #endregion

        #region Data

        object _selectedValue = null;
        object _owner = null;
        Func<object, Task<TreeNodeSelectionHelper>> _findChildFunc;


        #endregion

        #region Public Properties


        public object SelectedValue
        {
            get { return _selectedValue; }
            set { SelectAsync(value); }
        }
        //public IEnumerable<TreeNodeSelectionHelper> RootItems { get; set; }



        #endregion
    }
}
