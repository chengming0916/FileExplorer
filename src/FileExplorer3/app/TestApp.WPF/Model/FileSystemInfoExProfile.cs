using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Defines;
using QuickZip.Converters;
using QuickZip.UserControls.Logic.Tools.IconExtractor;

namespace FileExplorer.Models
{
    public class FileSystemInfoExProfile : IProfile
    {
        #region Constructor

        private class ExHierarchyComparer : IEntryHierarchyComparer
        {
            public HierarchicalResult CompareHierarchy(IEntryModel a, IEntryModel b)
            {
                if (!a.FullPath.Contains("::") && !b.FullPath.Contains("::"))
                    return PathComparer.Default.CompareHierarchy(a, b);
                FileSystemInfoEx fsia = FileSystemInfoEx.FromString(a.FullPath);
                FileSystemInfoEx fsib = FileSystemInfoEx.FromString(b.FullPath);
                if (a.FullPath == b.FullPath)
                    return HierarchicalResult.Current;
                if (IOTools.HasParent(fsib, fsia.FullName))
                    return HierarchicalResult.Child;
                if (IOTools.HasParent(fsia, fsib.FullName))
                    return HierarchicalResult.Parent;
                return HierarchicalResult.Unrelated;
            }
        }

        public FileSystemInfoExProfile()
        {
            HierarchyComparer = new ExHierarchyComparer();
            MetadataProvider = new FileSystemInfoMetadataProvider();
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return obj is FileSystemInfoExProfile;
        }

        public BaseControls.ISuggestSource GetSuggestSource()
        {
            return new ProfileSuggestionSource(this);
        }

        public IComparer<IEntryModel> GetComparer(ColumnInfo column)
        {
            return new ValueComparer<IEntryModel>(p => p.FullPath);
        }

        internal DirectoryInfoEx createDirectoryInfo(string path)
        {
            if (path.EndsWith(":"))
                return new DirectoryInfoEx(path + "\\");
            else return new DirectoryInfoEx(path);
        }

        internal FileInfoEx createFileInfo(string path)
        {
            return new FileInfoEx(path);
        }

        public Task<IEntryModel> ParseAsync(string path)
        {
            IEntryModel retVal = null;
            if (DirectoryEx.Exists(path))
                retVal = new FileSystemInfoExModel(this, createDirectoryInfo(path));
            else
                if (FileEx.Exists(path))
                    retVal = new FileSystemInfoExModel(this, createFileInfo(path));
            return Task.FromResult<IEntryModel>(retVal);
        }

        public Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null)
        {
            if (filter == null)
                filter = (m) => true;
            List<IEntryModel> retVal = new List<IEntryModel>();
            if (entry.IsDirectory)
            {
                DirectoryInfoEx di = createDirectoryInfo(entry.FullPath);
                retVal.AddRange(from fsi in di.GetFileSystemInfos()
                                let m = new FileSystemInfoExModel(this, fsi)
                                where filter(m)
                                select m);
            }
            return Task.FromResult<IEnumerable<IEntryModel>>(retVal);
        }

        public IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            yield return GetDefaultIcon.Instance;
            yield return GetFromSystemImageList.Instance;
            if (!entry.IsDirectory)
                if (entry.FullPath.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    entry.FullPath.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    entry.FullPath.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase)
                    )
                    yield return GetImageFromImageExtractor.Instance;
        }

        public Task<IEnumerable<IEntryModel>> Transfer(TransferMode mode, IEntryModel[] source, IEntryModel dest)
        {
            throw new NotImplementedException();
        }


        public Task<IEnumerable<IEntryModel>> TransferAsync(TransferMode mode, IEntryModel[] source, IEntryModel dest)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Rename(IEntryModel source, string newName)
        {
            throw new NotImplementedException();
        }



        #endregion

        #region Data

        private Bitmap _folderBitmap;
        private IconExtractor _iconExtractor = new ExIconExtractor();

        #endregion

        #region Public Properties

        public string RootDisplayName
        {
            get { return "Root"; }
        }
        public IEntryHierarchyComparer HierarchyComparer { get; private set; }
        public IMetadataProvider MetadataProvider { get; private set; }

        #endregion









        
    }
}
