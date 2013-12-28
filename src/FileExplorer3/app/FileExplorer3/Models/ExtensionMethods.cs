using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
