using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core.Utils;
using FileExplorer.Models;
using FileExplorer.UserControls.DragDrop;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Models
{
    /// <summary>
    /// Create virtual items in preset path if not created already.
    /// </summary>
    public class HandleFileDropped : INotifyDropped
    {
        IEntryModel[] _models;
        public HandleFileDropped(IEntryModel[] models)
        {
            _models = models;
        }

        public void NotifyPrepareDrop(VirtualDataObject sender, string format)
        {
              FileDropDataObject dataObject = sender as FileDropDataObject;
              Task.Run(async () =>
                  {
                      foreach (var m in _models)
                          if (!m.Profile.PathMapper[m].IsCached)
                          {
                              await m.Profile.PathMapper.UpdateCacheAsync(m);
                          }
                  }).Wait();
        }
    }

    public class FileBasedDragDropHandler : IDragDropHandler
    {
        private IProfile _profile;
        public FileBasedDragDropHandler(IProfile profile)
        {
            _profile = profile;
        }

        public IDataObject GetDataObject(IEnumerable<IEntryModel> entries)
        {
            var retVal = new FileDropDataObject(new HandleFileDropped(entries.ToArray()));
            retVal.SetFileDropData(entries
                .Select(m => new FileDrop(m.Profile.PathMapper[m].IOPath, m.IsDirectory))
                .Where (fd => fd.FileSystemPath != null)
                .ToArray());
            
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
                    IEntryModel vm;
                    try
                    {
                        vm = AsyncUtils.RunSync(() => _profile.ParseAsync(fn));                        
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
            return destDir.IsDirectory;
        }

        public QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            if (destDir.IsDirectory)
                return QueryDropResult.CreateNew(DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.Copy);
            else return QueryDropResult.None;
        }


        public DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel destDir, DragDropEffects allowedEffects)
        {
            string fileName = PathEx.GetFileName(destDir.FullPath);
            if (fileName == null) fileName = destDir.FullPath;
            MessageBox.Show(
                String.Format("[OnDropCompleted] ({2}) {0} entries to {1}",
                entries.Count(), fileName, allowedEffects));
            return DragDropEffects.None;
        }

      
    }
}
