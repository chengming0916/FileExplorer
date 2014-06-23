using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Return the first directory found from the input. 
        /// (E.g. ExtractFirstDir("\temp\1\2",0) will return "Temp")
        /// </summary>
        public static string ExtractFirstDir(this IPathHelper pathHelper, string input, Int32 startIndex)
        {
            if (input.Length < startIndex)
                return "";

            Int32 idx = input.IndexOf(pathHelper.Separator, startIndex);
            if (idx == -1)
                return input.Substring(startIndex);
            return input.Substring(startIndex, idx - startIndex);
        }

         /// <summary>
        /// Add a slash "\" to end of input if not exists.
        /// </summary>
        public static string AppendSlash(this IPathHelper pathHelper, string input)
        {
            if (input.EndsWith(pathHelper.Separator + "")) { return input; }
            else
            { return input + pathHelper.Separator; }
        }

        /// <summary>
        /// Add a slash "\" to front of input if not exists.
        /// </summary>
        public static string AppendFrontSlash(this IPathHelper pathHelper, string input)
        {
            if (input.StartsWith(pathHelper.Separator + "")) { return input; }
            else
            { return pathHelper.Separator + input; }
        }

        public static async Task<IEntryModel> LookupAsync(this IProfile profile, IEntryModel startEntry, string[] paths, CancellationToken ct, int idx = 0)
        {
            if (idx >= paths.Length)
                return startEntry;
            else
            {
                IEntryModel lookup = (await profile.ListAsync(startEntry, ct, em => 
                    em.Name.Equals(paths[idx], StringComparison.CurrentCultureIgnoreCase), true)).FirstOrDefault();
                ct.ThrowIfCancellationRequested();
                if (lookup != null)
                    return await LookupAsync(profile, lookup, paths, ct, idx+1);
                else return null;
            }
        }        

        public static IEntryModel Convert(this IConverterProfile[] converterProfiles, IEntryModel entryModel)
        {
            IEntryModel retVal = entryModel;
            foreach (var p in converterProfiles)
                retVal = p.Convert(retVal);
            return retVal;
        }

        public static async Task<IEntryModel> LookupAsync(this IProfile profile, string path, CancellationToken ct)
        {
            string curPath = path;
            IEntryModel retVal = await profile.ParseAsync(path);
            while (retVal == null && curPath != null)
            {
                curPath = profile.Path.GetDirectoryName(curPath);
                retVal = await profile.ParseAsync(curPath);
            }

            if (retVal != null && curPath != path && path.StartsWith(curPath, StringComparison.CurrentCultureIgnoreCase))
            {
                string[] trailingPaths = path.Substring(curPath.Length).Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                return await LookupAsync(retVal.Profile, retVal, trailingPaths, CancellationToken.None, 0);
            }

            return retVal;
        }

         public static async Task<IEntryModel> ParseThenLookupAsync(this IProfile profile, string path, CancellationToken ct)
        {
            IEntryModel retVal = await profile.ParseAsync(path);
            if (retVal == null)
                retVal = await profile.LookupAsync(path, ct);
            return retVal;
        }

    }
}
