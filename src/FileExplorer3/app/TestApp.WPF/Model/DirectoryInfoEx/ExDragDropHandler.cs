using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Models
{
    public class FileSystemInfoExDragDropHandler : IDragDropHandler
    {
        private IProfile _profile;
        public FileSystemInfoExDragDropHandler(IProfile profile)
        {
            _profile = profile;
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
                        vm = new FileSystemInfoExModel(_profile, FileSystemInfoEx.FromString(fn));
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

        public bool QueryCanDrop(IEntryModel destDir)
        {
            return (destDir as FileSystemInfoExModel).IsDirectory;
        }

        public QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            if ((destDir as FileSystemInfoExModel).IsDirectory)
                return QueryDropResult.CreateNew(DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.Copy);
            else return QueryDropResult.None;
        }


        public DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            MessageBox.Show(
                String.Format("[OnDropCompleted] ({2}) {0} entries to {1}",
                entries.Count(), PathEx.GetFileName(destDir.FullPath), allowedEffects));
            return DragDropEffects.None;
        }
    }
}
