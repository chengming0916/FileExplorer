using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IDirectoryTreeViewModel : ISupportSelectionHelper<IDirectoryNodeViewModel, IEntryModel>
    {        
        ISubEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }
    }

    public interface IDirectoryNodeViewModel : ISupportNodeSelectionHelper<IDirectoryNodeViewModel, IEntryModel>
    {
        bool ShowCaption { get; set; }
        
        ISubEntriesHelper<IDirectoryNodeViewModel> Entries { get; set; }
    }

}
