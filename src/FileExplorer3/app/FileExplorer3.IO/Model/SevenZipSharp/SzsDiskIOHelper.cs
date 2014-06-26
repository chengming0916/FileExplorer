using FileExplorer.IO;
using FileExplorer.IO.Compress;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models.SevenZipSharp
{
    public class SzsDiskIOHelper : DiskIOHelperBase
    {
        public SzsDiskIOHelper(SzsProfile profile)
            : base(profile)
        {
            this.Mapper = new FileBasedDiskPathMapper(m =>
                {
                    ISzsItemModel model = m as ISzsItemModel;
                    return model.Profile.Path.Combine(model.Root.FullPath, model.RelativePath);
                });
        }

        public override async Task<IEntryModel> RenameAsync(IEntryModel entryModel, string newName, CancellationToken ct)
        {
            throw new NotImplementedException();
            //SevenZipWrapper wrapper = (Profile as SzsProfile).Wrapper;
            //string destPath =  Profile.Path.Combine(Profile.Path.GetDirectoryName(entryModel.FullPath), newName);

            //return await Profile.ParseAsync(destPath);

        }

        public override async Task DeleteAsync(IEntryModel entryModel, CancellationToken ct)
        {
            SzsProfile profile = Profile as SzsProfile;
            ISzsItemModel szsEntryModel = entryModel as ISzsItemModel;

            //Bug: overwrite stream, so trail contents remains.
            using (var stream = await profile.DiskIO.OpenStreamAsync(szsEntryModel.Root, Defines.FileAccess.ReadWrite, ct))
            {
                string type = profile.Path.GetExtension(szsEntryModel.Root.Name);

                await profile.Wrapper.DeleteAsync(type, stream, szsEntryModel.RelativePath + (szsEntryModel.IsDirectory ? "\\*" : ""));
                
                lock(profile.VirtualModels)
                    if (profile.VirtualModels.Contains(szsEntryModel))
                        profile.VirtualModels.Remove(szsEntryModel);
            }            
        }

        public override async Task<Stream> OpenStreamAsync(IEntryModel entryModel,
            FileExplorer.Defines.FileAccess access, CancellationToken ct)
        {
            if (entryModel is SzsRootModel)
            {
                SzsRootModel rootModel = entryModel as SzsRootModel;
                return await (rootModel.ReferencedFile.Profile as IDiskProfile).DiskIO
                    .OpenStreamAsync(rootModel.ReferencedFile, access, ct);
            }

            switch (access)
            {
                case FileExplorer.Defines.FileAccess.Read: return await SzsFileStream.OpenReadAsync(entryModel, ct);
                case FileExplorer.Defines.FileAccess.Write: return SzsFileStream.OpenWrite(entryModel);
                case FileExplorer.Defines.FileAccess.ReadWrite: return await SzsFileStream.OpenReadWriteAsync(entryModel, ct);
            }
            throw new NotSupportedException();
        }

        public override async Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory, CancellationToken ct)
        {
            SzsProfile profile = Profile as SzsProfile;
            string parentPath = profile.Path.GetDirectoryName(fullPath);
            string name = profile.Path.GetFileName(fullPath);
            ISzsItemModel parentDir = await profile.ParseAsync(parentPath) as ISzsItemModel;
            if (parentDir == null)
                throw new Exception(String.Format("Parent dir {0} not exists.", parentPath));
            string relativePath = profile.Path.Combine(parentDir.RelativePath, name);
            ISzsItemModel retEntryModel = new SzsChildModel(parentDir.Root, relativePath, isDirectory);

            profile.VirtualModels.Add(retEntryModel);
            return retEntryModel;
        }

    }
}
