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
    public interface ITreeNodeSelectionHelper<VM, T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Used by a tree node to report to it's parent it's selected.
        /// </summary>
        /// <param name="path"></param>
        void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path);

        /// <summary>
        /// Used by a tree node to report to it's parent it's deselected.
        /// </summary>
        /// <param name="path"></param>
        void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pathAction">Run when lookup along the path (e.g. when HierarchicalResult = Child or Current)</param>
        /// <param name="nextNodeOnly"></param>
        /// <returns></returns>
        Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false);

        /// <summary>
        /// Whether current view model is selected.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Whether a child of current view model is selected.
        /// </summary>
        bool IsChildSelected { get; }

        /// <summary>
        /// The selected child of current view model.
        /// </summary>
        T SelectedChild { get; set; }

        /// <summary>
        /// The owner view model of this selection helper.
        /// </summary>
        VM ViewModel { get; }

        /// <summary>
        /// The represented value of this selection helper.
        /// </summary>
        T Value { get; }

    }
}
