﻿using System;
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
    public interface IFileListViewModel : IExportCommandBindings
    {
        /// <summary>
        /// Return available commands for current filelist, for toolbar and context menu.
        /// </summary>
        IToolbarCommandsHelper ToolbarCommands { get; }

        /// <summary>
        /// All changable script commands for the current file list, allow customize what to execute when certain action.
        /// </summary>
        IFileListScriptCommandContainer ScriptCommands { get; }
        
        /// <summary>
        /// Load entries and apply filters.
        /// </summary>
        IEntriesProcessor<IEntryViewModel> ProcessedEntries { get; }
                
        /// <summary>
        /// Allow customize columns and filters.
        /// </summary>
        IColumnsHelper Columns { get; }

        /// <summary>
        /// Responsible for item selection of the file list.
        /// </summary>
        IListSelector<IEntryViewModel, IEntryModel> Selection { get; }

        /// <summary>
        /// Setting the current directory will start the load of entries.
        /// </summary>
        IEntryModel CurrentDirectory { get; set; }

        string ViewMode { get; set; }
        int ItemSize { get; set; }

        bool IsCheckBoxVisible { get; set; }

        //bool IsContextMenuVisible { get; set; }

        void SignalChangeDirectory(IEntryModel newDirectory);
        
        IProfile[] Profiles { set; }        
    }
}
