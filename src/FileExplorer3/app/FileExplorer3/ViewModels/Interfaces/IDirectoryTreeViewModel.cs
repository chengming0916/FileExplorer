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
    public interface IDirectoryTreeViewModel : ISupportTreeSelector<IDirectoryNodeViewModel, IEntryModel>, IExportCommandBindings
    {
        IEntryModel[] RootModels { set; }
        IProfile[] Profiles { set; }

        bool EnableDrag { get; set; }
        bool EnableDrop { get; set; }

        Task SelectAsync(IEntryModel value);
        void ExpandRootEntryModels();

        /// <summary>
        /// Return available commands for current filelist, for toolbar and context menu.
        /// </summary>
        IToolbarCommandsHelper ToolbarCommands { get; }

        /// <summary>
        /// All changable script commands for the current file list, allow customize what to execute when certain action.
        /// </summary>
        IDirectoryTreeScriptCommandContainer ScriptCommands { get; }
    }

    public interface IDirectoryNodeViewModel : IEntryViewModel, ISupportTreeSelector<IDirectoryNodeViewModel, IEntryModel>, IDraggable
    {
        bool ShowCaption { get; set; }        
    }

}
