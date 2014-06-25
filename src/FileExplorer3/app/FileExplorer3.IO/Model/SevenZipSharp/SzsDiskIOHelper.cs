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
            throw new NotImplementedException();
            //if (entryModel.IsDirectory)
            //    Directory.Delete(entryModel.FullPath, true);
            //else File.Delete(entryModel.FullPath);
        }

        public override async Task<Stream> OpenStreamAsync(IEntryModel entryModel,
            FileExplorer.Defines.FileAccess access, CancellationToken ct)
        {
            //SevenZipWrapper wrapper = (Profile as SzsProfile).Wrapper;
            //ISzsItemModel itemModel = entryModel as ISzsItemModel;
            //IEntryModel rootReferenceModel = itemModel.Root.ReferencedFile;
            //return new CompressMemoryStream(wrapper, rootReferenceModel, itemModel.RelativePath, access, ct);
            //To-DO: save to Profile.DiskIO.Mapper[itemModel].IOPath

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
            string parentPath = Profile.Path.GetDirectoryName(fullPath);
            string name = Profile.Path.GetFileName(fullPath);
            ISzsItemModel parentDir = await Profile.ParseAsync(parentPath) as ISzsItemModel;
            if (parentDir == null)
                throw new Exception(String.Format("Parent dir {0} not exists.", parentPath));
            string relativePath = Profile.Path.Combine(parentDir.RelativePath, name);
            ISzsItemModel retEntryModel = new SzsChildModel(parentDir.Root, relativePath, isDirectory);

            (Profile as SzsProfile).VirtualModels.Add(retEntryModel);
            return retEntryModel;
            //throw new NotImplementedException();
            //if (isDirectory)
            //    Directory.CreateDirectory(fullPath);
            //else
            //    if (!File.Exists(fullPath))
            //        using (File.Create(fullPath))
            //        { }
            //return await Profile.ParseAsync(fullPath);
        }

    }
}
