using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core.Script;

namespace FileExplorer.Models
{
    /// <summary>
    /// Implemented by IProfile for document how to handle when certain action is taken place
    /// (e.g. double click on an item in file list)
    /// Because it's run by IExplorerViewModel, certain parameter is always available:
    /// - Profile:IProfile
    /// - FileList:IFileListViewModel
    /// </summary>
    public interface IProfileCommands
    {
        /// <summary>
        /// Rename an entry (SourceEntry:IEntryModel) to a new file name (DestName:string)
        /// </summary>
        IScriptCommand Rename { get; }
        
        /// <summary>
        /// Transfer(Mode:string) a group of entries (SourceEntries:IEntryModel[]) to destination (DestDirectory:IEntryModel)
        /// </summary>
        IScriptCommand Transfer { get; }

        /// <summary>
        /// How to react when double click an item (SelectedItem:IEntryModel) on file list (FileList:IFileListViewModel).
        /// </summary>
        IScriptCommand DoubleClickFileList { get; }
        
        IScriptCommand OpenItem { get; }
    }
}
