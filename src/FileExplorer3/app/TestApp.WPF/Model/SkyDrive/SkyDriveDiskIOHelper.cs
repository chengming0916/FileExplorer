using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class SkyDriveDiskIOHelper : DiskIOHelperBase
    {        

        #region Constructor

        public SkyDriveDiskIOHelper(SkyDriveProfile profile)
            : base(profile)
        {            
            this.DiskPath = new SkyDriveDiskPathMapper();
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
            return new SkyDriveItemModel(Profile as SkyDriveProfile, fullPath, false);
        }


        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }
}
