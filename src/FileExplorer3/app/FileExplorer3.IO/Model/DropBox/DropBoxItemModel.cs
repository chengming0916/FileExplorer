using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class DropBoxItemModel : DiskEntryModelBase
    {
        #region Constructors

        public DropBoxItemModel(DropBoxProfile profile, DropNet.Models.MetaData metadata,
            string parentFullPath = null)
            : base(profile)
        {
            //A list of GetPropertyValue - https://www.dropbox.com/developers/core/docs#metadata-details
            Metadata = metadata;
            if (parentFullPath == null)
            {
                base.FullPath = base.Label = base.Name = profile.Alias;
            }
            else
            {
                base.FullPath = profile.Path.Combine(parentFullPath, metadata.Name);
                base.Label = base.Name = metadata.Name;
            }
            this.IsDirectory = metadata.Is_Dir;

            this._isRenamable = true;
            if (!this.IsDirectory)
            {
                long size = 0;
                if (long.TryParse(metadata.Size, out size))
                    this.Size = size;
            }


        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public DropNet.Models.MetaData Metadata { get; private set; }

        #endregion
    }
}
