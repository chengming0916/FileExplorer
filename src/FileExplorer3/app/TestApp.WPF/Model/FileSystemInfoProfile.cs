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

namespace FileExplorer.Models
{
    public class FileSystemInfoProfile : IProfile
    {
        #region Cosntructor

        public FileSystemInfoProfile()
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

        private DirectoryInfo createDirectoryInfo(string path)
        {
            if (path.EndsWith(":"))
                return new DirectoryInfo(path + "\\");
            else return new DirectoryInfo(path);
        }

        private FileInfo createFileInfo(string path)
        {
            return new FileInfo(path);
        }

        public Task<IEntryModel> ParseAsync(string path)
        {
            IEntryModel retVal = null;
            if (Directory.Exists(path))
                retVal = new FileSystemInfoModel(this, createDirectoryInfo(path));
            else
                if (File.Exists(path))
                    retVal = new FileSystemInfoModel(this, createFileInfo(path));
            return Task.FromResult<IEntryModel>(retVal);
        }

        public Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null)
        {
            List<IEntryModel> retVal = new List<IEntryModel>();
            if (entry.IsDirectory)
            {
                DirectoryInfo di = createDirectoryInfo(entry.FullPath);
                retVal.AddRange(from fsi in di.GetFileSystemInfos() select new FileSystemInfoModel(this, fsi));
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
            using (var icon = entry == null || entry.IsDirectory ?
                getFolderIcon() :
                System.Drawing.Icon.ExtractAssociatedIcon(entry.FullPath))
            using (var bitmap = icon.ToBitmap())
                return Task.FromResult<ImageSource>(
                    Cofe.Core.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bitmap));
        }

        public IResult<IEnumerable<IEntryModel>> Transfer(TransferMode mode, IEntryModel[] source, IEntryModel dest)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> Rename(IEntryModel source, string newName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        private Bitmap _folderBitmap;

        #endregion

        #region Public Properties

        public string RootDisplayName
        {
            get { return "Root"; }
        }
        public IEntryHierarchyComparer HierarchyComparer { get; private set; }
        public IMetadataProvider MetadataProvider { get; private set; }

        #endregion





        public Task<IEnumerable<IEntryModel>> TransferAsync(TransferMode mode, IEntryModel[] source, IEntryModel dest)
        {
            throw new NotImplementedException();
        }

        Task<bool> IProfile.Rename(IEntryModel source, string newName)
        {
            throw new NotImplementedException();
        }










      
    }
}
