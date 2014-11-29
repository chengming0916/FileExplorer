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

        [XmlIgnore]
        public string Name
        {
            get { return Profile.Path.GetFileName(FullPath); }
            set { FullPath = Profile.Path.Combine(Profile.Path.GetDirectoryName(FullPath), value); }
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

        public BookmarkModel AddLink(string label, string linkPath)
        {
            var nameGenerator = FileNameGenerator.Rename(label);
            while (SubModels.Any(bm => bm.Label == label))
                label = nameGenerator.Generate();

            var retVal = new BookmarkModel(Profile as BookmarkProfile, BookmarkEntryType.Link,
                FullPath + "/" + label) { LinkPath = linkPath, Label = label };
            SubModels.Add(retVal);
            return retVal;
        }

        public BookmarkModel AddFolder(string label)
        {
            var nameGenerator = FileNameGenerator.Rename(label);
            while (SubModels.Any(bm => bm.Label == label))
                label = nameGenerator.Generate();

            var retVal = new BookmarkModel(Profile as BookmarkProfile, BookmarkEntryType.Directory,
                FullPath + "/" + label) { Label = label };
            SubModels.Add(retVal);
            return retVal;
        }

        public void Remove(string label)
        {
            var link2Remove = SubModels.FirstOrDefault(bm => bm.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
            if (link2Remove != null)
                SubModels.Remove(link2Remove);
        }

        #endregion


        public bool IsDragging { get; set; }       
        public string DisplayName { get { return this.Label; } }
        public string LinkPath { get; set; }
    }
}
