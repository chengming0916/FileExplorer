using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileExplorer.Models.Bookmark
{
    public class BookmarkModel : NotifyPropertyChanged, IEntryModel
    {
        #region fields

        public enum BookmarkEntryType {  Root, Directory, Link }

        #endregion

        #region constructors

        public BookmarkModel()
        {

        }

        public BookmarkModel(BookmarkProfile profile, BookmarkEntryType type, string fullPath)
            : this()
        {
            Profile = profile;
            Name = Profile.Path.GetFileName(fullPath);
            this.Type = type;
            FullPath = fullPath;

            if (IsDirectory)
                SubModels = new List<BookmarkModel>();
        }

        #endregion

        #region events


        #endregion

        #region properties

        [XmlIgnore]
        public IProfile Profile
        {
            get;
            private set;
        }

        public BookmarkEntryType Type
        {
            get;
            set;
        }

        [XmlIgnore]
        public bool IsDirectory
        {
            get { return Type != BookmarkEntryType.Link; }            
        }

        public IEntryModel Parent
        {
            get
            {
                string parentPath = Profile.Path.GetDirectoryName(FullPath);
                return AsyncUtils.RunSync(() => Profile.ParseAsync(parentPath));
            }
        }

        [XmlIgnore]
        public string Label
        {
            get { return Name; }
            set { Name = value; }
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string FullPath
        {
            get;
            set;
        }

        public bool IsRenamable
        {
            get;
            set;
        }

        public DateTime CreationTimeUtc
        {
            get;
            set;
        }

        public DateTime LastUpdateTimeUtc
        {
            get;
            set;
        }

        public List<BookmarkModel> SubModels
        {
            get;
            set;
        }

        #endregion

        #region methods

        public bool Equals(IEntryModel other)
        {
            return other is BookmarkModel && (other as BookmarkModel).FullPath.Equals(FullPath);
        }

        public override string ToString()
        {
            return FullPath;
        }

        #endregion

    }
}
