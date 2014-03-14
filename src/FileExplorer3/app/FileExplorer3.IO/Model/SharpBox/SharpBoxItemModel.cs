﻿using AppLimit.CloudComputing.SharpBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class SharpBoxItemModel : DiskEntryModelBase
    {
        #region Constructors

        public SharpBoxItemModel(SharpBoxProfile profile, ICloudFileSystemEntry metadata, 
            string parentFullPath = null)
            : base(profile)
        {
            //A list of GetPropertyValue - https://www.dropbox.com/developers/core/docs#metadata-details
            Metadata = metadata;
            base.FullPath = parentFullPath == null ? profile.Alias : profile.Path.Combine(parentFullPath, metadata.Name);
            this.IsDirectory = metadata is ICloudDirectoryEntry;
            this.Label = metadata.Name;
            this._isRenamable = true;
            if (!this.IsDirectory)
            {
                long size = 0;
                if (long.TryParse(metadata.GetPropertyValue("size"), out size))
                    this.Size = size;
            }
   
            this.ImageUrl = metadata.GetPropertyValue("icon");
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public ICloudFileSystemEntry Metadata { get; private set; }
        public string ImageUrl { get; protected set; }

        #endregion
    }
}
