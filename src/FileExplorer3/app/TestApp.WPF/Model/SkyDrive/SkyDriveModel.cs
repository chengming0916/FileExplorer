using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class SkyDriveItemModel : EntryModelBase
    {
        #region Constructor

        public SkyDriveItemModel(SkyDriveProfile profile, string path, object data, string parentUniqueId = null)
            : base(profile)
        {
            AccessPath = path;

            dynamic d = data as dynamic;
            UniqueId = d.id;
            this.IsDirectory = d.type == "folder" || d.type == "album";
            this.Type = d.type; //photo, album or folder
            this.Description = d.description;
            this.Label = this.Name = d.name;
            this.FullPath = profile.ModelCache.GetPath(parentUniqueId) + "/" + d.name;
            
            this.ImageUrl = d.picture;
            this.SourceUrl = d.source;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public string AccessPath { get; private set; }
        public string Description { get; private set; }
        public string UniqueId { get; private set; }
        public string Type { get; private set; }
        public string ImageUrl { get; protected set; }
        public string SourceUrl { get; protected set; }

        #endregion
    }

    //public class SkyDriveItemModel : SkyDriveInfoModel
    //{
    //    #region Constructor

    //    public SkyDriveItemModel(SkyDriveProfile profile, string path, object data, string parentUniqueId = null)
    //        : base(profile, path, data, parentUniqueId)
    //    {
    //        dynamic d = data as dynamic;


    //    }

    //    #endregion

    //    #region Methods

    //    #endregion

    //    #region Data

    //    #endregion

    //    #region Public Properties

    //    #endregion

    //}

    //public class SkyDriveDirectoryModel : SkyDriveInfoModel
    //{
    //    #region Constructor

    //    public SkyDriveDirectoryModel(SkyDriveProfile profile, string path, object data, string parentUniqueId = null)
    //        : base(profile, path, data, parentUniqueId)
    //    {
    //        dynamic d = data as dynamic;            

    //    }

    //    #endregion

    //    #region Methods

    //    #endregion

    //    #region Data

    //    #endregion

    //    #region Public Properties

    //    #endregion

    //}
}
