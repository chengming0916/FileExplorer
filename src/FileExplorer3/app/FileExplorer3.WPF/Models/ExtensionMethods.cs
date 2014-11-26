﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using FileExplorer.Defines;
using FileExplorer.WPF.Defines;
using Caliburn.Micro;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileExplorer.Models;
using FileExplorer.WPF.Utils;
using System.Windows;

namespace FileExplorer.WPF.Models
{
    public static partial class ExtensionMethods
    {
        //public static IShellDragDropHandler DragDrop(this IProfile profile)
        //{
        //    return (profile is IWPFProfile) ? (profile as IWPFProfile).DragDrop : null;
        //}

        //public static IEventAggregator Events(this IProfile profile)
        //{
        //    return profile.Events;
        //}


        public static async Task<IEntryModel> ParseAsync(this IProfile[] profiles, string path)
        {
            foreach (var p in profiles)
            {
                var result = await p.ParseAsync(path);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static IScriptCommand Parse(this IProfile[] profiles, string path,
            Func<IEntryModel, IScriptCommand> ifFoundFunc, IScriptCommand ifNotFound)
        {
            return new ParsePathCommand(profiles, path, ifFoundFunc, ifNotFound);
        }


        public static IScriptCommand Parse(this IProfile profile, string path,
            Func<IEntryModel, IScriptCommand> ifFoundFunc, IScriptCommand ifNotFound)
        {
            return new ParsePathCommand(new[] { profile }, path, ifFoundFunc, ifNotFound);
        }

        public static IEnumerable<IModelIconExtractor<IEntryModel>> GetIconExtractSequence(this IProfile[] profiles, IEntryModel entry)
        {
            foreach (var p in profiles)
            {
                var result = p.GetIconExtractSequence(entry);
                if (result != null)
                    return result;
            }
            return new List<IModelIconExtractor<IEntryModel>>();
        }

        public static void NotifyEntryChanges(this IProfile profile, object sender, string fullPath, ChangeType changeType, string orgParseName = null)
        {
            if (profile == null || profile.Events == null)
                return;

            if (changeType == ChangeType.Moved)
                profile.Events.PublishOnUIThread(new EntryChangedEvent(fullPath, orgParseName));
            else profile.Events.PublishOnUIThread(new EntryChangedEvent(changeType, fullPath));
        }


        public static string GetName(this IEntryModel model)
        {
            return model.Profile.Path.GetFileName(model.FullPath);
        }

        public static string GetExtension(this IEntryModel model)
        {
            return model.Profile.Path.GetExtension(model.FullPath);
        }


        public static string Combine(this IEntryModel model, params string[] paths)
        {
            return model.Profile.Path.Combine(model.FullPath, paths);
        }

        public static async Task<Stream> OpenStreamAsync(this IDiskIOHelper ioHelper, string fullPath,
            FileExplorer.Defines.FileAccess access, CancellationToken ct)
        {
            IEntryModel entryModel = await ioHelper.Profile.ParseAsync(fullPath);
            ct.ThrowIfCancellationRequested();
            if (entryModel == null)
                if (access == FileExplorer.Defines.FileAccess.Write)
                    entryModel = await ioHelper.CreateAsync(fullPath, false, ct);
                else throw new IOException("File not found.");
            ct.ThrowIfCancellationRequested();
            return await ioHelper.OpenStreamAsync(entryModel, access, ct);
        }



        public static async Task<string> WriteToCacheAsync(this IDiskIOHelper ioHelper, IEntryModel entry, CancellationToken ct, bool force = false)
        {
            var mapping = ioHelper.Mapper[entry];

            if (!mapping.IsCached || force)
            {
                if (entry.IsDirectory)
                {
                    System.IO.Directory.CreateDirectory(mapping.IOPath);
                    var listing = await entry.Profile.ListAsync(entry, ct).ConfigureAwait(false);
                    foreach (var subEntry in listing)
                        await WriteToCacheAsync(ioHelper, subEntry, ct, force).ConfigureAwait(false);
                }
                else
                {
                    using (var srcStream = await ioHelper.OpenStreamAsync(entry.FullPath,
                        FileExplorer.Defines.FileAccess.Read, ct))
                    using (var outputStream = System.IO.File.OpenWrite(mapping.IOPath))
                        await StreamUtils.CopyStreamAsync(srcStream, outputStream).ConfigureAwait(false);
                }
            }

            return mapping.IOPath;
        }

        public static async Task DeleteAsync(this IDiskIOHelper ioHelper, IEntryModel[] entryModels, CancellationToken ct)
        {
            foreach (var em in entryModels)
                await ioHelper.DeleteAsync(em, ct);
        }

        public static async Task<IEntryModel> GetParentAsync(this IProfile profile, IEntryModel entry)
        {
            var parentFullPath = profile.Path.GetDirectoryName(entry.FullPath);
            if (String.IsNullOrEmpty(parentFullPath))
                return null;
            return await profile.ParseAsync(parentFullPath);
        }

        public static IProfile[] GetProfiles(this IEntryModel[] entryModels)
        {
            return entryModels.Select(em => em.Profile).Distinct().ToArray();
        }

        public static async Task<ImageSource> GetIconForModelAsync(this IModelIconExtractor<IEntryModel> extractor,
            IEntryModel model, CancellationToken ct)
        {
            byte[] bytes = await extractor.GetIconBytesForModelAsync(model, ct);
            ct.ThrowIfCancellationRequested();
            return bytes == null ?
                new BitmapImage() :
                FileExplorer.WPF.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bytes);
        }


        public static Task<IDataObject> GetDataObject(this IDragDropHandler dragDropHandler, IEnumerable<IEntryModel> entries)
        {
            if (dragDropHandler is IShellDragDropHandler)
                return (dragDropHandler as IShellDragDropHandler).GetDataObject(entries);
            else return Task.FromResult<IDataObject>(null);
        }
        public static IEnumerable<IEntryModel> GetEntryModels(this IDragDropHandler dragDropHandler, IDataObject dataObject)
        {
            if (dragDropHandler is IShellDragDropHandler)
                return (dragDropHandler as IShellDragDropHandler).GetEntryModels(dataObject);
            else return new List<IEntryModel>();
        }

        public static void OnDragCompleted(this IDragDropHandler dragDropHandler,
            IEnumerable<IEntryModel> entries, IDataObject da, DragDropEffectsEx effect)
        {
            if (dragDropHandler is IShellDragDropHandler)
                (dragDropHandler as IShellDragDropHandler).OnDragCompleted(entries, da, effect);
            else dragDropHandler.OnDragCompleted(entries, effect);
        }

        public static QueryDropEffects QueryDrop(this IDragDropHandler dragDropHandler,
            IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            if (dragDropHandler is IShellDragDropHandler)
                return (dragDropHandler as IShellDragDropHandler).QueryDrop(entries, da, dest, allowedEffects);
            else return dragDropHandler.QueryDrop(entries, dest, allowedEffects);
        }
        public static DragDropEffectsEx OnDropCompleted(this IDragDropHandler dragDropHandler,
            IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            if (dragDropHandler is IShellDragDropHandler)
                return (dragDropHandler as IShellDragDropHandler).OnDropCompleted(entries, da, dest, allowedEffects);
            else return dragDropHandler.OnDropCompleted(entries, dest, allowedEffects);
        }

        //        public interface IModelIconExtractor<IEntryModel>
        //{
        //    Task<byte[]> GetIconForModelAsync(IEntryModel model, CancellationToken ct);
        //}
    }
}
