using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public interface IDirectoryTreeViewModel
    {
        void NotifySelectionChanged(IEnumerable<IDirectoryNodeViewModel> path, bool selected);
        Task SelectAsync(IEntryModel model);
        IEntryModel SelectedEntry { get; set; }
        IEntryModel SelectingEntry { get; }

        IObservableCollection<IDirectoryNodeViewModel> Subdirectories { get; }
    }

    public interface IDirectoryNodeViewModel
    {
        IDirectoryTreeViewModel TreeModel { get; }

        IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel);

        Task LoadAsync(bool force = false);

        Task BroadcastSelectAsync(IEntryModel model, Action<IDirectoryNodeViewModel> action);
        Task BroadcastSelectAsync(IDirectoryTreeViewModel sender, IEntryModel model, params IDirectoryNodeBroadcastHandler[] allHandlers);
        
        Task NotifyChildSelectionChangedAsync(IDirectoryNodeViewModel node, bool selected, Stack<IDirectoryNodeViewModel> path);

        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }

        bool IsChildSelected { get; set; }

        bool IsDummyNode { get; }
        IEntryViewModel CurrentDirectory { get; }

        IDirectoryNodeViewModel ParentNode { get; }

        IObservableCollection<IDirectoryNodeViewModel> Subdirectories { get; }
    }

}
