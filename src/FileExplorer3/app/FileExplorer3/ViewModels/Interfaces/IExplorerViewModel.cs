using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public interface IExplorerViewModel
    {
        IDirectoryTreeViewModel DirectoryTreeModel { get; }
        IFileListViewModel FileListModel { get; }
        IStatusbarViewModel StatusbarModel { get; }
    }
}
