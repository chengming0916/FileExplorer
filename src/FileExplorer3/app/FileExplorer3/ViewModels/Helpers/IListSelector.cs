using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.ViewModels.Helpers
{
    public interface IReportSelected<VM>
    {
        void ReportChildSelected(VM viewModel);
        void ReportChildUnSelected(VM viewModel);
    }

    public interface IListSelector<VM, T> : IReportSelected<VM>, INotifyPropertyChanged
    {
        #region Constructor

        #endregion

        #region Methods
        
        void SelectAll();
        void UnselectAll();

        ///// <summary>
        ///// Called by FileListViewModel to notify selectedItems is changed.
        ///// </summary>
        ///// <param name="selectedItems"></param>
        //void OnSelectionChanged(IList selectedItems);

        #endregion

        #region Data

        #endregion

        #region Public Properties
        event EventHandler SelectionChanged;

        IList<VM> SelectedItems { get;  }

        ICommand UnselectAllCommand { get; }        
        ICommand SelectAllCommand { get; }

        #endregion
    }
}
