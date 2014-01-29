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
        IDiskPathMapper Mapper { get; }
        IDiskProfile Profile { get; }

        Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access);
        Task DeleteAsync(params IEntryModel[] entryModels);
        Task<IEntryModel> RenameAsync(string fullPath, string newName);
        Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory);

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

        public virtual Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public virtual Task DeleteAsync(IEntryModel[] entryModels)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEntryModel> RenameAsync(string fullPath, string newName)
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

        public override async Task<IEntryModel> RenameAsync(string fullPath, string newName)
        {
            string destPath = Path.Combine( Path.GetDirectoryName(fullPath), newName);
            if (File.Exists(fullPath))
                File.Move(fullPath, destPath);
            else if (Directory.Exists(fullPath))
                Directory.Move(fullPath, destPath);
            return await Profile.ParseAsync(destPath);
        }

        public override async Task DeleteAsync(IEntryModel[] entryModels)
        {
            foreach (var entryModel in entryModels)
            {
                if (entryModel.IsDirectory)
                    Directory.Delete(entryModel.FullPath, true);
                else File.Delete(entryModel.FullPath);
            }
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
