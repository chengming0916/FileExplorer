using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer
{
    public static partial class ExtensionMethods
    {
        public static ITreeRootSelector<VM, T> AsRoot<VM, T>(this ITreeSelector<VM, T> selector)
        {
            return selector as ITreeRootSelector<VM, T>;
        }

        /// <summary>
        /// Broadcast changes, so the tree can refresh changed items.
        /// </summary>
        public static async Task BroascastAsync<VM, T>(this ITreeRootSelector<VM, T> rootSelector, T changedItem)
        {
            await rootSelector.LookupAsync(changedItem,
                    RecrusiveSearch<VM, T>.SkipIfNotLoaded,
                    RefreshDirectory<VM, T>.WhenFound);
        }
    }
}
