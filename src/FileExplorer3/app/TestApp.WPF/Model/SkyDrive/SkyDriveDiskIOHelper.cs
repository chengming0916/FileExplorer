using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Live;

namespace FileExplorer.Models
{
    public class SkyDriveDiskIOHelper : DiskIOHelperBase
    {


        #region Constructor

        public SkyDriveDiskIOHelper(SkyDriveProfile profile)
            : base(profile)
        {
            _profile = profile;
            this.Mapper = new SkyDriveDiskPathMapper();
        }

        #endregion

        #region Methods

        public override async Task<Stream> OpenStreamAsync(IEntryModel entryModel, FileAccess access)
        {
            switch (access)
            {
                case FileAccess.Read: return await SkyDriveFileStream.OpenReadAsync(entryModel);
                case FileAccess.Write: return SkyDriveFileStream.OpenWrite(entryModel);
                case FileAccess.ReadWrite: return await SkyDriveFileStream.OpenReadWriteAsync(entryModel);
            }
            throw new NotSupportedException();
        }


        public override async Task<IEntryModel> CreateAsync(string fullPath, bool isDirectory)
        {
            if (isDirectory)
            {
                await _profile.checkLoginAsync();
                string parentPath = _profile.Path.GetDirectoryName(fullPath);
                string name = _profile.Path.GetFileName(fullPath);
                SkyDriveItemModel parentDir = await _profile.ParseAsync(parentPath)
                     as SkyDriveItemModel;

                if (parentDir == null)
                    throw new DirectoryNotFoundException(parentPath);

                var folderData = new Dictionary<string, object>();
                folderData.Add("name", name);
                LiveConnectClient liveClient = new LiveConnectClient(_profile.Session);

                LiveOperationResult result = await liveClient.PostAsync(parentDir.UniqueId, folderData);
                return new SkyDriveItemModel(_profile, null, result.Result, parentDir.FullPath);



            }
            else return new SkyDriveItemModel(Profile as SkyDriveProfile, fullPath, false);
        }


        #endregion

        #region Data

        private SkyDriveProfile _profile;

        #endregion

        #region Public Properties

        #endregion
    }
}
