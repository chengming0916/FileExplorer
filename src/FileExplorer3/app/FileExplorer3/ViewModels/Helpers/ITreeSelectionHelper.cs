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
    public interface ITreeSelectionHelper<VM, T> : INotifyPropertyChanged
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pathAction">Run when lookup along the path (e.g. when HierarchicalResult = Child or Current)</param>
        /// <param name="nextNodeOnly"></param>
        /// <returns></returns>
        Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, ITreeSelectionLookup<VM, T> lookupProc,
            params ITreeSelectionProcessor<VM, T>[] processors);

        
    }


    public interface ISupportSelectionHelper<VM, T> : ISupportSubEntriesHelper<VM>       
    {
        ITreeNodeSelectionHelper<VM, T> Selection { get; set; }
    }

    public interface ISupportSelectionHelper<VM, T> : ISupportSubEntriesHelper<VM>       
    {
        ITreeRootSelectionHelper<VM, T> Selection { get; set; }
    }

    /// <summary>
    /// Implemented in tree node view model, to provide selection support.
    /// </summary>
    /// <typeparam name="VM">ViewModel.</typeparam>
    /// <typeparam name="T">Value</typeparam>
    public interface ITreeRootSelectionHelper<VM, T> : ITreeSelectionHelper<VM,T>
    {

        
        //Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false);

        /// <summary>
        /// Select a tree node by value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SelectAsync(T value);

        /// <summary>
        /// Select a tree node by value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lookupProc"></param>
        /// <param name="processors"></param>
        /// <returns></returns>
        Task SelectAsync(T value, ITreeSelectionLookup<VM, T> lookupProc,
            params ITreeSelectionProcessor<VM, T>[] processors);

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
