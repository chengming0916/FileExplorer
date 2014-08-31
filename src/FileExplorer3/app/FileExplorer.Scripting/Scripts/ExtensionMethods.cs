using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class ExtensionMethods 
    {
        public static IProgress<TransferProgress> GetProgress(this IParameterDic pd)
        {
            return (pd.ContainsKey("Progress") ? pd["Progress"] as IProgress<TransferProgress> : null) ??
                NullTransferProgress.Instance;
        }

        public static void SetProgress(this IParameterDic pd, IProgress<TransferProgress> progress)
        {
            if (pd.ContainsKey("Progress"))
                pd["Progress"] = progress;
            else pd.Add("Progress", progress);
        }
    }
}
