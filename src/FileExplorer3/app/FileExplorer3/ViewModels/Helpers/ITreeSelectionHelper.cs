using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    /// <summary>
    /// Implemented in tree node view model, to provide selection support.
    /// </summary>
    /// <typeparam name="VM"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface ITreeSelectionHelper<VM,T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Used by a tree node to report to it's root it's selected.
        /// </summary>
        /// <param name="path"></param>
        void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path);

        /// <summary>
        /// Used by a tree node to report to it's parent it's deselected.
        /// </summary>
        /// <param name="path"></param>
        void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path);

        //Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false);

        /// <summary>
        /// Select a tree node by value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SelectAsync(T value);

        /// <summary>
        /// Raised when a node is selected, use SelectedValue/ViewModel to return the selected item.
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Selected node.
        /// </summary>
        VM SelectedViewModel { get; set; }

        /// <summary>
        /// Value of SelectedViewModel.
        /// </summary>
        T SelectedValue { get; set; }

        /// <summary>
        /// Compare Hierarchy of two value.
        /// </summary>
        Func<T, T, HierarchicalResult> CompareFunc { get; }

        /// <summary>
        /// Given a ViewModel, return it's selection helper.
        /// </summary>
        Func<VM, ITreeNodeSelectionHelper<VM, T>> GetSelectionHelperFunc { get; }

    }
}
