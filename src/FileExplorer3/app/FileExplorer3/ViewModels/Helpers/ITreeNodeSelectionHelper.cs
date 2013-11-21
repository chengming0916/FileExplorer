using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ITreeNodeSelectionHelper<VM,T> : INotifyPropertyChanged
    {
        void ReportChildSelected(Stack<ITreeNodeSelectionHelper<VM, T>> path);
        void ReportChildDeselected(Stack<ITreeNodeSelectionHelper<VM, T>> path);
        Task<ITreeNodeSelectionHelper<VM, T>> LookupAsync(T value, bool nextNodeOnly = false);

        void OnSelected(bool selected);
        void OnChildSelected(T newValue);

        bool IsSelected { get; set; }
        bool IsChildSelected { get; }
        T SelectedChild { get; set; }

        VM ViewModel { get; }
        T Value { get; }

    }
}
