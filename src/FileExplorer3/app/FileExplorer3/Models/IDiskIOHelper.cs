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

        Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access, CancellationToken ct);
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

        public virtual Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access, CancellationToken ct)
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

    public class DefaultDiskIOHelper : DiskIOHelperBase
    {
        public DefaultDiskIOHelper(IDiskProfile profile)
            : base(profile)
        {

        }

        public override async Task<IEntryModel> RenameAsync(IEntryModel entryModel, string newName, CancellationToken ct)
        {
            string destPath = Path.Combine(Path.GetDirectoryName(entryModel.FullPath), newName);
            if (!entryModel.IsDirectory)
                File.Move(entryModel.FullPath, destPath);
            else if (Directory.Exists(entryModel.FullPath))
                Directory.Move(entryModel.FullPath, destPath);
            return await Profile.ParseAsync(destPath);
        }

        public override async Task DeleteAsync(IEntryModel entryModel, CancellationToken ct)
        {
            if (entryModel.IsDirectory)
                Directory.Delete(entryModel.FullPath, true);
            else File.Delete(entryModel.FullPath);
        }

        public override async Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access, CancellationToken ct)
        {
            switch (access)
            {
                case FileAccess.Read: return File.OpenRead(entryModel.FullPath);
                case FileAccess.Write: return File.OpenWrite(entryModel.FullPath);
                case FileAccess.ReadWrite: return File.Open(entryModel.FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            throw new NotImplementedException();
        }

        public override async Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory, CancellationToken ct)
        {
            if (isDirectory)
                Directory.CreateDirectory(fullPath);
            else
                if (!File.Exists(fullPath))
                    using (File.Create(fullPath))
                    { }
            return await Profile.ParseAsync(fullPath);
        }
    }
}
