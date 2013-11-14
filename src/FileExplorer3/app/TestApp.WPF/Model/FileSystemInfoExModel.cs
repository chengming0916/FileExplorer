using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class FileSystemInfoExModel : EntryModelBase
    {
        #region Cosntructor

        public FileSystemInfoExModel(IProfile profile, FileSystemInfoEx fsi)
            : base(profile)
        {
            this.Label = fsi.Label;
            this.Attributes = fsi.Attributes;
            this.FullPath = fsi.FullName;
            this.Name = fsi.Name;
            this.IsRenamable = true;
            this.IsDirectory = fsi.IsFolder;
            if (fsi is FileInfoEx)
                Size = (fsi as FileInfoEx).Length;
            string parentPath = PathEx.GetDirectoryName(fsi.FullName);
            this._parentFunc = 
                new Lazy<IEntryModel>(() => {
                    return String.IsNullOrEmpty(parentPath) ? null :
                    new FileSystemInfoExModel(profile,
                        (profile as FileSystemInfoExProfile).createDirectoryInfo(parentPath));
                });
            this.Description = fsi.GetType().ToString();
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public FileAttributes Attributes { get; protected set; }
        public long Size { get; protected set; }       

        #endregion
    }
}
