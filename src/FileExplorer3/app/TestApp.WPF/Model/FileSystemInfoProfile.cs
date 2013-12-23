using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;
//using FileExplorer.UserControls.DragDrop;

namespace FileExplorer.Models
{
    public class FileSystemInfoProfile : IProfile
    {
        #region Cosntructor

        public FileSystemInfoProfile()
        {
            HierarchyComparer = PathComparer.Default;
            MetadataProvider = new FileSystemInfoExMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return obj is FileSystemInfoProfile;
        }

        public BaseControls.ISuggestSource GetSuggestSource()
        {
            return new ProfileSuggestionSource(this);
        }

        public IComparer<IEntryModel> GetComparer(ColumnInfo column)
        {
            return new ValueComparer<IEntryModel>(p => p.FullPath);
        }

        internal DirectoryInfo createDirectoryInfo(string path)
        {
            if (path.EndsWith(":"))
                return new DirectoryInfo(path + "\\");
            else return new DirectoryInfo(path);
        }

        internal FileInfo createFileInfo(string path)
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

        public IDataObject GetDataObject(IEnumerable<IEntryModel> entries)
        {            
            var retVal = new DataObject();
            retVal.SetData(
               DataFormats.FileDrop,
               entries.Cast<FileSystemInfoModel>()
               .Select(m => m.FullPath).ToArray());
            return retVal;            
        }

        public DragDropEffects QueryDrag(IEnumerable<IEntryModel> entries)
        {
            return DragDropEffects.Copy;
        }

        public void OnDragCompleted(IEnumerable<IEntryModel> draggables, IDataObject da, DragDropEffects effect)
        {

        }


        public IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject)
        {
            yield break; ;
        }

        public bool QueryCanDrop(IEntryModel destDir)
        {
            return (destDir as FileSystemInfoModel).IsDirectory;
        }


        public QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            return QueryDropResult.None;
        }

        public DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            throw new NotImplementedException();
        }


        private Icon getFolderIcon()
        {
            return new Icon(System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TestApp.WPF.Model.folder.ico"));
        }


        public IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            yield return GetDefaultIcon.Instance;
            if (entry.IsDirectory)
                yield return GetFromIconExtractIcon.Instance;
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
        public IEnumerable<ICommandProvider> CommandProviders { get; private set; }

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
