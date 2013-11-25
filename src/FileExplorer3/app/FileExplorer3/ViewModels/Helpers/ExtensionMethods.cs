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
    }
}
