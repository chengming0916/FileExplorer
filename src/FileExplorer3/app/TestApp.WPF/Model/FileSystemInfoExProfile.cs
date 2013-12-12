using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.UserControls.DragDrop;
using FileExplorer.ViewModels.Helpers;
//using FileExplorer.UserControls.DragDrop;
using QuickZip.Converters;
using QuickZip.UserControls.Logic.Tools.IconExtractor;

namespace FileExplorer.Models
{
    public class FileSystemInfoExProfile : IProfile
    {
        #region Constructor

        private class ExHierarchyComparer : IEntryHierarchyComparer
        {
            private bool HasParent(FileSystemInfoEx child, DirectoryInfoEx parent)
            {
                DirectoryInfoEx current = child.Parent;
                while (current != null)
                {
                    if (current.Equals(parent))
                        return true;
                    current = current.Parent;
                }
                return false;
            }

            public HierarchicalResult CompareHierarchyInner(IEntryModel a, IEntryModel b)
            {
                if (a == null || b == null)
                    return HierarchicalResult.Unrelated;
                if (!a.FullPath.Contains("::") && !b.FullPath.Contains("::"))
                    return PathComparer.Default.CompareHierarchy(a, b);                
                FileSystemInfoEx fsia = FileSystemInfoEx.FromString(a.FullPath);
                FileSystemInfoEx fsib = FileSystemInfoEx.FromString(b.FullPath);
                if (a.FullPath == b.FullPath)
                    return HierarchicalResult.Current;

                //if (fsia is DirectoryInfoEx && HasParent(fsib, fsia as DirectoryInfoEx))
                //    return HierarchicalResult.Child;
                //if (fsib is DirectoryInfoEx && HasParent(fsia, fsib as DirectoryInfoEx))
                //    return HierarchicalResult.Parent;

                //if (a.FullPath.EndsWith(":\\") &&
                //    b.IsDirectory && (b as FileSystemInfoExModel).DirectoryType
                //    != DirectoryInfoEx.DirectoryTypeEnum.dtFolder)
                //    return HierarchicalResult.Unrelated;

                //if (b.FullPath.EndsWith(":\\") &&
                //    a.IsDirectory && (a as FileSystemInfoExModel).DirectoryType
                //    != DirectoryInfoEx.DirectoryTypeEnum.dtFolder)
                //    return HierarchicalResult.Unrelated;

                //if (a.FullPath.EndsWith(":\\") &&
                //   b.FullPath.StartsWith("c:\\Temp\\Cofe3", StringComparison.CurrentCultureIgnoreCase))
                //    return HierarchicalResult.Unrelated;

                //if (b.FullPath.EndsWith(":\\") &&
                //    (a as FileSystemInfoExModel).ParentFullPath != null &&
                //    (!(a as FileSystemInfoExModel).ParentFullPath.Equals(b.FullPath)))
                //    return HierarchicalResult.Unrelated;

                if (IOTools.HasParent(fsib, fsia.FullName))
                    return HierarchicalResult.Child;
                if (IOTools.HasParent(fsia, fsib.FullName))
                    return HierarchicalResult.Parent;
                return HierarchicalResult.Unrelated;
            }

            public HierarchicalResult CompareHierarchy(IEntryModel a, IEntryModel b)
            {
                HierarchicalResult retVal = CompareHierarchyInner(a, b);
                //Debug.WriteLine(String.Format("{2} {0},{1}", a.FullPath, b.FullPath, retVal));
                return retVal;
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

        public IDataObject GetDataObject(IEnumerable<IEntryModel> entries)
        {
            //var retVal = new FileDropDataObject(null);
            //retVal.SetFileDropData(entries.Cast<FileSystemInfoExModel>()
            //    .Select(m => new FileDrop(m.FullPath, m.IsDirectory)).ToArray());
            var retVal = new DataObject();
            retVal.SetData(
                DataFormats.FileDrop,
                entries.Cast<FileSystemInfoExModel>()
                .Select(m => m.FullPath).ToArray());
            return retVal;            
        }

        public DragDropEffects QueryDrag(IEnumerable<IEntryModel> entries)
        {
            return DragDropEffects.Copy | DragDropEffects.Move;
        }

        public void OnDragCompleted(IEnumerable<IEntryModel> draggables, IDataObject da, DragDropEffects effect)
        {

        }


        public IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject)
        {
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNameList = dataObject.GetData(DataFormats.FileDrop) as string[];                
                foreach (var fn in fileNameList)
                {
                    FileSystemInfoExModel vm;
                    try
                    {
                        vm = new FileSystemInfoExModel(this, FileSystemInfoEx.FromString(fn));
                    }
                    catch
                    {
                        vm = null;
                    }

                    if (vm != null)
                        yield return vm;
                }
            }
        }


        public QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            return QueryDropResult.CreateNew(DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.Copy);
        }


        public DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            MessageBox.Show(
                String.Format("[OnDropCompleted] ({2}) {0} entries to {1}",
                entries.Count(), PathEx.GetFileName(destDir.FullPath), allowedEffects));
            return DragDropEffects.None;
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
