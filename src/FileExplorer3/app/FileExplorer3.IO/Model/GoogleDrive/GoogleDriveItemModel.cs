using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Utils;
using FileExplorer.Utils;

namespace FileExplorer.Models
{
    public class GoogleDriveItemModel : DiskEntryModelBase
    {
        public static string FolderMimeType = "application/vnd.google-apps.folder";

        #region Constants



        #endregion

        #region Constructor

        public GoogleDriveItemModel(GoogleDriveProfile profile, string alias)
            : base(profile)
        {
            init(profile, alias);
            this.UniqueId = "root";
            this.IsDirectory = true;
        }


        internal void init(GoogleDriveProfile profile, string path)
        {
            FullPath = path;
            this.Label = this.Name = profile.Path.GetFileName(path);
        }


        internal void init(GoogleDriveProfile profile, string path, Google.Apis.Drive.v2.Data.File f)
        {
            init(profile, path);
            UniqueId = f.Id;
            this.IsDirectory = f.MimeType.Equals(FolderMimeType);
            this.Name = f.OriginalFilename ?? f.Title;

            this.Type = profile.MimeTypeManager.GetExportableMimeTypes(f.MimeType).FirstOrDefault();
            if (!this.IsDirectory && String.IsNullOrEmpty(Profile.Path.GetExtension(this.Name)))
            {
                string extension = profile.MimeTypeManager.GetExtension(this.Type);
                if (!String.IsNullOrEmpty(extension))
                    this.Label = this.Name += extension;
            }
            this.Size = f.FileSize.HasValue ? f.FileSize.Value : 0;
            this.IsRenamable = true;

            this.Description = f.Description;
            this.ImageUrl = f.ThumbnailLink;
            this.SourceUrl = f.DownloadUrl;

            if (!this.IsDirectory && System.IO.Path.GetExtension(this.Name) == "" && this.Type != null)
            {
                string extension = ShellUtils.MIMEType2Extension(this.Type);
                if (!String.IsNullOrEmpty(extension))
                {
                    this.Name += extension;
                }
            }

        }

        public GoogleDriveItemModel(GoogleDriveProfile profile, Google.Apis.Drive.v2.Data.File file, string parentFullPath = null)
            : base(profile)
        {
            string name = file.OriginalFilename ?? file.Title;
            string path = parentFullPath == null ? profile.Alias : parentFullPath + "/" + name;
            init(profile, path, file);
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
        public long Size { get; protected set; }

        /// <summary>
        /// Url for downloading a file.
        /// </summary>
        public string SourceUrl { get; protected set; }

        #endregion
    }


}
