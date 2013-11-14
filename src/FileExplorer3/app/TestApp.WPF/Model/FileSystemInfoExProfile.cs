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

        public FileSystemInfoExProfile()
        {
            HierarchyComparer = PathComparer.Default;
            MetadataProvider = new FileSystemInfoMetadataProvider();
        }

        #endregion

        #region Methods

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
            List<IEntryModel> retVal = new List<IEntryModel>();
            if (entry.IsDirectory)
            {
                DirectoryInfoEx di = createDirectoryInfo(entry.FullPath);
                retVal.AddRange(from fsi in di.GetFileSystemInfos()
                                select new FileSystemInfoExModel(this, fsi));
            }
            return Task.FromResult<IEnumerable<IEntryModel>>(retVal);
        }

        private Icon getFolderIcon()
        {
            return new Icon(System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TestApp.WPF.Model.folder.ico"));
        }

        public Task<ImageSource> GetIconAsync(IEntryModel entry, int size)
        {
            if (entry != null)
            {
                using (FileSystemInfoEx fsi = FileSystemInfoEx.FromString(entry.FullPath))
                {
                    Bitmap bitmap = null;
                    if (fsi != null && fsi.PIDL != null)
                    {
                        bitmap = _iconExtractor.GetBitmap(QuickZip.Converters.IconSize.extraLarge,
                            fsi.PIDL.Ptr, entry.IsDirectory, false);
                        return Task.FromResult<ImageSource>(
                            Cofe.Core.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bitmap));
                    }
                }
            }
            
            return Task.FromResult<ImageSource>(null);
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
