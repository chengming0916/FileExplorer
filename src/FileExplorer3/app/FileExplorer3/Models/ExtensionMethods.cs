using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public static partial class ExtensionMethods
    {
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
            return new ParsePathCommand(new [] { profile }, path, ifFoundFunc, ifNotFound);
        }

        public static IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(this IProfile[] profiles, IEntryModel entry)
        {
            foreach (var p in profiles)
            {
                var result = p.GetIconExtractSequence(entry);
                if (result != null)
                    return result;
            }
            return new List<IEntryModelIconExtractor>();
        }

        public static void NotifyEntryChanges(this IProfile profile, string fullPath, ChangeType changeType, string orgParseName = null)
        {
            if (changeType == ChangeType.Moved)
                profile.Events.Publish(new EntryChangedEvent(fullPath, orgParseName));
            else profile.Events.Publish(new EntryChangedEvent(fullPath, changeType));
        }

        public static string GetName(this IEntryModel model)
        {
            return model.Profile.Path.GetFileName(model.FullPath);
        }

        public static string Combine(this IEntryModel model, params string[] paths)
        {
            return model.Profile.Path.Combine(model.FullPath, paths);
        }

        public static async Task<Stream> OpenStreamAsync(this IDiskIOHelper ioHelper, string fullPath, FileAccess access)
        {
            IEntryModel entryModel = await ioHelper.Profile.ParseAsync(fullPath);
            if (entryModel == null)
                if (access == FileAccess.Write)
                    entryModel = await ioHelper.CreateAsync(fullPath, false);
                else throw new IOException("File not found.");
            return await ioHelper.OpenStreamAsync(entryModel, access);
        }



        public static async Task<string> WriteToCacheAsync(this IDiskIOHelper ioHelper, IEntryModel entry, bool force = false)
        {
            var mapping = ioHelper.Mapper[entry];

            if (!mapping.IsCached || force)
                using (var srcStream = await ioHelper.OpenStreamAsync(entry.FullPath, System.IO.FileAccess.Read))
                using (var outputStream = System.IO.File.OpenWrite(mapping.IOPath))
                    await StreamUtils.CopyStreamAsync(srcStream, outputStream);

            return mapping.IOPath;
        }

        public static async Task<IEntryModel> GetParentAsync(this IProfile profile, IEntryModel entry)
        {
            var parentFullPath = profile.Path.GetDirectoryName(entry.FullPath);
            if (String.IsNullOrEmpty(parentFullPath))
                return null;
            return await profile.ParseAsync(parentFullPath);
        }

    }
}
