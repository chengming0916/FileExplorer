using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Utils;

namespace FileExplorer.Models
{
    public class SkyDriveItemModel : EntryModelBase
    {
        #region Constructor

        private SkyDriveItemModel(SkyDriveProfile profile)
            : base(profile)
        {            
            
        }


        internal void init(SkyDriveProfile profile, string path)
        {            
            FullPath = path;
            this.Label = this.Name = profile.Path.GetFileName(path);            
            //this._parentFunc = new Lazy<IEntryModel>(() =>
            //    AsyncUtils.RunSync(() => profile.ParseAsync(PathFE.GetDirectoryNameR(path))));
        }

        internal void init(SkyDriveProfile profile, string path, dynamic d)
        {            
            init(profile, path);
            UniqueId = d.id;
            this.IsDirectory = d.type == "folder" || d.type == "album";

            this.Type = d.type; //photo, album or folder
            this.Description = d.description;
            this.ImageUrl = d.picture;
            this.SourceUrl = d.source;
        }
        /// <summary>
        /// Generate a temporary model for uploading.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        public SkyDriveItemModel(SkyDriveProfile profile, string path, bool isDirectory)
            : this(profile)
        {
            //AccessPath = profile.GetAccessPath(path);
            init(profile, path);
            this.IsDirectory = isDirectory;
            //SourceUrl = "Unknown";
        }

        public SkyDriveItemModel(SkyDriveProfile profile, string accessPath, object data, string parentFullPath = null)
            : this(profile)
        {
            //AccessPath = accessPath;
            //Debug.WriteLine(AccessPath);
            dynamic d = data as dynamic;
            string path = parentFullPath == null ? profile.Alias :  parentFullPath  + "/" + d.name;
            init(profile, path, d);
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties
        
        //public string AccessPath { get; private set; }
        public string Description { get; private set; }
        public string UniqueId { get; private set; }
        public string Type { get; private set; }
        public string ImageUrl { get; protected set; }

        /// <summary>
        /// Url for downloading a file.
        /// </summary>
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
