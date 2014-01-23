using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IDiskIOHelper
    {
        IDiskPathMapper DiskPath { get; }
        IDiskProfile Profile { get; }

        Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access);
        Task DeleteAsync(string fullPath);
        Task RenameAsync(string fullPath, string newName);
        Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory);

    }

    public class DiskIOHelperBase : IDiskIOHelper
    {

        protected DiskIOHelperBase(IDiskProfile profile)
        {
            DiskPath = new IODiskPatheMapper();
            Profile = profile;
        }

        public IDiskProfile Profile { get; protected set; }
        public IDiskPathMapper DiskPath { get; protected set; }

        public virtual Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public virtual Task DeleteAsync(string fullPath)
        {
            throw new NotImplementedException();
        }

        public virtual Task RenameAsync(string fullPath, string newName)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory)
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

        public override async Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access)
        {
            switch (access)
            {
                case FileAccess.Read: return File.OpenRead(entryModel.FullPath);
                case FileAccess.Write: return File.OpenWrite(entryModel.FullPath);
                case FileAccess.ReadWrite: return File.Open(entryModel.FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            throw new NotImplementedException();
        }

        public override async Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory)
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
