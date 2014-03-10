using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v2.Data;

namespace FileExplorer.Models
{
    public class GoogleDriveDiskIOHelper : DiskIOHelperBase
    {
        private GoogleDriveProfile _profile;
        #region Constructor

        public GoogleDriveDiskIOHelper(GoogleDriveProfile profile)
            : base(profile)
        {
            _profile = profile;
            this.Mapper = new FileBasedDiskPathMapper(m => (m as GoogleDriveItemModel).SourceUrl);
        }


        #endregion

        #region Methods

        public override async Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access, CancellationToken ct)
        {
            switch (access)
            {
                case FileAccess.Read: return await GoogleDriveFileStream.OpenReadAsync(entryModel,
                    () => (entryModel.Profile as GoogleDriveProfile).HttpClientFunc(), ct);
                case FileAccess.Write: return GoogleDriveFileStream.OpenWrite(entryModel, () => (entryModel.Profile as GoogleDriveProfile).HttpClientFunc());
                case FileAccess.ReadWrite: return await GoogleDriveFileStream.OpenReadWriteAsync(entryModel,
                    () => (entryModel.Profile as GoogleDriveProfile).HttpClientFunc(), ct);
            }
            throw new NotSupportedException();
        }

        public override async Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory, CancellationToken ct)
        {
            if (isDirectory)
            {
                 string parentPath = _profile.Path.GetDirectoryName(fullPath);
                string name = _profile.Path.GetFileName(fullPath);
                GoogleDriveItemModel parentDir = await _profile.ParseAsync(parentPath)
                     as GoogleDriveItemModel;

                if (parentDir == null)
                    throw new DirectoryNotFoundException(parentPath);

                var newFolder = new Google.Apis.Drive.v2.Data.File()
                {
                     Title = name,                     
                     MimeType = GoogleMimeTypeManager.FolderMimeType
                };
                if (parentDir.UniqueId != "root")
                {
                    if (newFolder.Parents == null)
                        newFolder.Parents = new List<ParentReference>();
                    newFolder.Parents.Add(new ParentReference() { Id = parentDir.UniqueId });
                }
                var insertedFolder = (await _profile.DriveService.Files.Insert(newFolder).ExecuteAsync());
                return new GoogleDriveItemModel(_profile, insertedFolder, parentDir.FullPath);
            }
            else return new GoogleDriveItemModel(_profile, fullPath, false);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

    }
}
