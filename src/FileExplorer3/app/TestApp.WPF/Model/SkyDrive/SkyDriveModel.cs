using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class SkyDriveInfoModel : EntryModelBase
    {
        #region Constructor

        public SkyDriveInfoModel(SkyDriveProfile profile, object data, string parentUniqueId = null)
            : base(profile)
        {
            dynamic d = data as dynamic;
            UniqueId = d.id;
            this.Type = d.type; //photo, album or folder
            this.Description = d.description;
            this.FullPath = profile.ModelCache.GetPath(parentUniqueId) + "/" + d.name;
            this.ImageUrl = null;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public string Description { get; private set; }
        public string UniqueId { get; private set; }
        public string Type { get; private set; }
        public string ImageUrl { get; protected set; }

        #endregion
    }

    public class SkyDriveItemModel : SkyDriveInfoModel
    {
        #region Constructor

        public SkyDriveItemModel(SkyDriveProfile profile, object data, string parentUniqueId = null)
            : base(profile, data, parentUniqueId)
        {
            dynamic d = data as dynamic;


        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

    }

    public class SkyDriveDirectoryModel : SkyDriveInfoModel
    {
        #region Constructor

        public SkyDriveDirectoryModel(SkyDriveProfile profile, object data, string parentUniqueId = null)
            : base(profile, data, parentUniqueId)
        {
            dynamic d = data as dynamic;            

        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

    }
}
