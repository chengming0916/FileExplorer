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
    public interface ITreeNodeSelectionHelper<VM, T> : ITreeSelectionHelper<VM,T>
    {


        Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNode = false);

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

        void SetIsSelected(bool value);
        void SetSelectedChild(T value);

        /// <summary>
        /// The owner view model of this selection helper.
        /// </summary>
        VM ViewModel { get; }

        /// <summary>
        /// The represented value of this selection helper.
        /// </summary>
        T Value { get; }

        ITreeNodeSelectionHelper<VM, T> ParentSelectionHelper { get; }

    }
}
