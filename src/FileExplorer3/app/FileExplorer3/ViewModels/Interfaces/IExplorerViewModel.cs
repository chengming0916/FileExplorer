using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IExplorerViewModel : ISupportCommandManager, 
        IScreen, IDraggable
    {
        IEntryModel[] RootModels { get; set; }
       
        IDirectoryTreeViewModel DirectoryTree { get; }
        IFileListViewModel FileList { get; }
        IStatusbarViewModel Statusbar { get; }
        ISidebarViewModel Sidebar { get; }

        IExplorerParameters Parameters { get; set; }
        IConfiguration Configuration { get; set; }

        IEntryViewModel CurrentDirectory { get; }
        Task GoAsync(string gotoPath);
        Task GoAsync(IEntryModel entryModel);
    }
}
