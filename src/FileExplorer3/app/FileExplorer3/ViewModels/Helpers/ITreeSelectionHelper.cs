using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ITreeSelectionHelper<VM,T> : INotifyPropertyChanged
    {
        void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path);
        void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path);
        Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false);
        Task SelectAsync(T value);

        event EventHandler SelectionChanged;
        VM SelectedViewModel { get; set; }
        T SelectedValue { get; set; }

        Func<T, T, HierarchicalResult> CompareFunc { get; }

        Func<VM, ITreeNodeSelectionHelper<VM, T>> GetSelectionHelperFunc { get; }

    }
}
