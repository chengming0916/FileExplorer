using FileExplorer.IO;
using FileExplorer.Models;
using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IDiskIOHelper
    {
        IDiskPathMapper Mapper { get; }
        IDiskProfile Profile { get; }

        IScriptCommand GetTransferCommand(IEntryModel srcModel, IEntryModel destDirModel, bool removeOriginal);

        Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileExplorer.Defines.FileAccess access, CancellationToken ct);
        Task DeleteAsync(IEntryModel entryModel, CancellationToken ct);
        Task<IEntryModel> RenameAsync(IEntryModel entryModel, string newName, CancellationToken ct);
        Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory, CancellationToken ct);

    }

   

}
