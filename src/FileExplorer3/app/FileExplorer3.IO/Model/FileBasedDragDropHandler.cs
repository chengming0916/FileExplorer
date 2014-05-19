using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Defines;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.UserControls.DragDrop;
using FileExplorer.WPF.ViewModels.Helpers;
using System.Windows;

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

            foreach (var m in _models)
                if (m.Profile is IDiskProfile)
                {
                    var mapping = (m.Profile as IDiskProfile).DiskIO.Mapper[m];
                    if (mapping != null && !mapping.IsCached)
                        AsyncUtils.RunSync(() => (m.Profile as IDiskProfile).DiskIO
                            .WriteToCacheAsync(m, CancellationToken.None)
                            );
                }

            //AsyncUtils.RunSync(() => Task.Run(async () =>
            //    {
            //        foreach (var m in _models)
            //            if (m.Profile is IDiskProfile)
            //            {
            //                var mapping = (m.Profile as IDiskProfile).DiskIO.Mapper[m];                              
            //                if (mapping != null && !mapping.IsCached)
            //                    await (m.Profile as IDiskProfile).DiskIO.WriteToCacheAsync(m, CancellationToken.None);
            //            }
            //    }));
        }
    }

    public class FileBasedDragDropHandler : IDragDropHandler
    {
        private IProfile _fsiProfile; //For loading drag items.        
        public IScriptCommand TransferCommand { get; set; }

        private IProfile _profile;
        public FileBasedDragDropHandler(IProfile profile, IWindowManager windowManager)
        {
            _profile = profile;
            _fsiProfile = profile is FileSystemInfoProfile ? (FileSystemInfoProfile)profile :
                new FileSystemInfoProfile(profile.Events(), windowManager);

            TransferCommand =
               new FileExplorer.WPF.ViewModels.TransferCommand((effect, source, destDir) =>
                   source.Profile is IDiskProfile ?
                       (IScriptCommand)new FileTransferScriptCommand(source, destDir, effect == DragDropResult.Move)
                       : ResultCommand.Error(new NotSupportedException())
                   , windowManager);
        }

        public async Task<IDataObject> GetDataObject(IEnumerable<IEntryModel> entries)
        {
            var retVal = new FileDropDataObject(new HandleFileDropped(entries.ToArray()));
            retVal.SetFileDropData(entries
                .Where(m => m.Profile is IDiskProfile)
                .Select(m => new FileDrop((m.Profile as IDiskProfile).DiskIO.Mapper[m].IOPath, m.IsDirectory))
                .Where(fd => fd.FileSystemPath != null)
                .ToArray());

            return retVal;
        }

        public DragDropResult QueryDrag(IEnumerable<IEntryModel> entries)
        {
            foreach (var e in entries)
                if (e.Profile is IDiskProfile && (e.Profile as IDiskProfile).DiskIO.Mapper[e].IsVirtual)
                    return DragDropResult.Copy;
            return DragDropResult.Copy | DragDropResult.Move;
        }

        public void OnDragCompleted(IEnumerable<IEntryModel> draggables, DragDropResult effect)
        {
            //if (effect == DragDropResult.Move)
            //    draggables.First().Profile.
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

        public QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel destDir, DragDropResult allowedEffects)
        {
            if (destDir.IsDirectory)
                return QueryDropResult.CreateNew(DragDropResult.Copy | DragDropResult.Move, DragDropResult.Copy);
            else return QueryDropResult.None;
        }


        public DragDropResult OnDropCompleted(IEnumerable<IEntryModel> entries, IEntryModel destDir, DragDropResult allowedEffects)
        {
            string fileName = PathEx.GetFileName(destDir.FullPath);
            if (fileName == null) fileName = destDir.FullPath;

            if (entries.Count() == 0)
                return DragDropResult.None;
            if (entries.Any(e => e.Equals(destDir)))
                return DragDropResult.None;

            if (MessageBox.Show(
                String.Format("[OnDropCompleted] ({2}) {0} entries to {1}",
                entries.Count(), fileName, allowedEffects), "FileBasedDragDropHandler.OnDropCompleted", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DragDropResult effect = allowedEffects.HasFlag(DragDropResult.Copy) ? DragDropResult.Copy :
                    allowedEffects.HasFlag(DragDropResult.Move) ? DragDropResult.Move : DragDropResult.None;

                if (effect == DragDropResult.None)
                    return DragDropResult.None;

                var sr = new ScriptRunner();
                sr.RunAsync(
                    new ParameterDic()
                    {
                        { "Source" , entries.ToArray() },
                        { "Dest" , destDir },
                        {"DragDropResult", effect }
                    }, TransferCommand);
                return effect;
            };
            return DragDropResult.None;
        }


    }
}
