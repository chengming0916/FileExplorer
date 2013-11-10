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
        void NotifySelected(DirectoryNodeViewModel node);
        void Select(IEntryModel model);
    }

    public interface IDirectoryNodeViewModel
    {
        IDirectoryTreeViewModel TreeModel { get; }

        IDirectoryNodeViewModel CreateSubmodel(IEntryModel entryModel);
        
        Task BroadcastSelectAsync(IEntryModel model, Action<IDirectoryNodeViewModel> action);

        bool IsSelected { get; set; }

        IEntryViewModel CurrentDirectory { get; }

        IObservableCollection<IDirectoryNodeViewModel> Subdirectories { get; }
    }

}
