using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public interface IExplorerViewModel : IExportCommandBindings
    {
        IEntryModel[] RootModels { get; set; }

        IDirectoryTreeViewModel DirectoryTree { get; }
        IFileListViewModel FileList { get; }
        IStatusbarViewModel Statusbar { get; }

        void Go(string gotoPath);
    }
}
