using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IFileListViewModel
    {
        /// <summary>
        /// Return available commands for current filelist.
        /// </summary>
        ICommandsHelper CommandsHelper { get; }
        
        /// <summary>
        /// Load entries and apply filters.
        /// </summary>
        IEntriesProcessor<IEntryViewModel> ProcessedEntries { get; }
                
        IColumnsHelper Columns { get; }


        IListSelector<IEntryViewModel, IEntryModel> Selection { get; }
        
        IEventAggregator Events { get; }

        Task SetCurrentDirectoryAsync(IEntryModel em);

        IEntryModel CurrentDirectory { get; set; }
        IToolbarViewModel Toolbar { get; set; }

        string ViewMode { get; set; }
        int ItemSize { get; set; }

        bool IsCheckBoxVisible { get; set; }

        event EventHandler ViewAttached;
        
    }
}
