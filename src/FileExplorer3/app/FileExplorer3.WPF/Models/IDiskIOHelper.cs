using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Models
{
    public interface IDiskIOHelper
    {
        IDiskPathMapper Mapper { get; }
        IDiskProfile Profile { get; }

        Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileExplorer.Defines.FileAccess access, CancellationToken ct);
        Task DeleteAsync(IEntryModel entryModel, CancellationToken ct);
        Task<IEntryModel> RenameAsync(IEntryModel entryModel, string newName, CancellationToken ct);
        Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory, CancellationToken ct);

    }

    public class DiskIOHelperBase : IDiskIOHelper
    {

        protected DiskIOHelperBase(IDiskProfile profile)
        {
            Mapper = new IODiskPatheMapper();
            Profile = profile;
        }

        public IDiskProfile Profile { get; protected set; }
        public IDiskPathMapper Mapper { get; protected set; }

        public virtual Task<Stream> OpenStreamAsync(IEntryModel entryModel, 
            FileExplorer.Defines.FileAccess access, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public virtual Task DeleteAsync(IEntryModel entryModel, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEntryModel> RenameAsync(IEntryModel entryModel, string newName, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

    }

    
}
