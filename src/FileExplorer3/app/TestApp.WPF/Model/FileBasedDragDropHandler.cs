using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core;
using Cofe.Core.Script;
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
              AsyncUtils.RunSync(() => Task.Run(async () =>
                  {
                      foreach (var m in _models)
                          if (m.Profile is IDiskProfile)
                          {
                              var mapping = (m.Profile as IDiskProfile).DiskIO.Mapper[m];
                              if (mapping != null && !mapping.IsCached)
                                  await (m.Profile as IDiskProfile).DiskIO.WriteToCacheAsync(m);
                          }
                  }));
        }
    }

    public class FileBasedDragDropHandler : IDragDropHandler
    {
        private static IProfile _fsiProfile = new FileSystemInfoProfile(null); //For loading drag items.
        private IProfile _profile;
        public FileBasedDragDropHandler(IProfile profile)
        {
            _profile = profile;
        }

        public IDataObject GetDataObject(IEnumerable<IEntryModel> entries)
        {
            var retVal = new FileDropDataObject(new HandleFileDropped(entries.ToArray()));
            retVal.SetFileDropData(entries
                .Where(m => m.Profile is IDiskProfile)
                .Select(m => new FileDrop((m.Profile as IDiskProfile).DiskIO.Mapper[m].IOPath, m.IsDirectory))
                .Where (fd => fd.FileSystemPath != null)
                .ToArray());
            
            return retVal;
        }

        public DragDropEffects QueryDrag(IEnumerable<IEntryModel> entries)
        {
            foreach (var e in entries)
                if (e.Profile is IDiskProfile && (e.Profile as IDiskProfile).DiskIO.Mapper[e].IsVirtual)
                    return DragDropEffects.Copy;
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
                    IEntryModel vm = null;
                    try
                    {
                        if (Directory.Exists(fn))
                            vm = new FileSystemInfoModel(_fsiProfile, new DirectoryInfo(fn));
                        else if (File.Exists(fn))
                            vm = new FileSystemInfoModel(_fsiProfile, new FileInfo(fn));                                                    
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
            if (MessageBox.Show(
                String.Format("[OnDropCompleted] ({2}) {0} entries to {1}",
                entries.Count(), fileName, allowedEffects), "FileBasedDragDropHandler.OnDropCompleted", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DragDropEffects effect = allowedEffects.HasFlag(DragDropEffects.Copy) ? DragDropEffects.Copy :
                    allowedEffects.HasFlag(DragDropEffects.Move) ? DragDropEffects.Move : DragDropEffects.None;

                if (effect == DragDropEffects.None)
                    return DragDropEffects.None;

                var sr = new ScriptRunner();
                var scriptToRun = new Queue<IScriptCommand>( 
                    entries.Select(e => new FileTransferScriptCommand(e, destDir, effect == DragDropEffects.Move)));
                sr.Run(scriptToRun, new ParameterDic());
                return effect;                
            };
            return DragDropEffects.None;
        }

      
    }
}
