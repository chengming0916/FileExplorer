using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ISupportNodeSelectionHelper<VM, T> : ISupportSubEntriesHelper<VM>       
    {
        ITreeNodeSelectionHelper<VM, T> Selection { get; set; }
    }

    /// <summary>
    /// Implemented in tree node view model, to provide selection support.
    /// </summary>
    /// <typeparam name="VM">ViewModel.</typeparam>
    /// <typeparam name="T">Value</typeparam>
    public interface ITreeSelectionHelper<VM, T> : ITreeNodeSelectionHelper<VM,T>
    {
       

        /// <summary>
        /// Raised when a node is selected, use SelectedValue/ViewModel to return the selected item.
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Selected node.
        /// </summary>
        VM SelectedViewModel { get;  }

        /// <summary>
        /// Value of SelectedViewModel.
        /// </summary>
        T SelectedValue { get; set; }

        /// <summary>
        /// Compare Hierarchy of two value.
        /// </summary>
        Func<T, T, HierarchicalResult> CompareFunc { get; }

        ObservableCollection<VM> OverflowedAndRootItems { get; set; }


    }
}
