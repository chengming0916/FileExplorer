using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;

namespace FileExplorer
{
    public static partial class ExtensionMethods
    {
        public static void Run(this IScriptRunner scriptRunner, ParameterDic initialParameters, params IScriptCommand[] cmds)
        {
            scriptRunner.Run(new Queue<IScriptCommand>(cmds), initialParameters);
        }
        public static async Task RunAsync(this IScriptRunner scriptRunner, ParameterDic initialParameters, params IScriptCommand[] cmds)
        {
            await scriptRunner.RunAsync(new Queue<IScriptCommand>(cmds), initialParameters);
        }

        public static ParameterDic ConvertAndMerge(this IParameterDicConverter converter, ParameterDic pd, object parameter = null, params object[] additionalParameters)
        {
            var convertedPd = converter.Convert(parameter, additionalParameters);
            if (pd != null)
                foreach (var k in pd.Keys)
                    convertedPd.AddOrUpdate(k, pd[k]);
            return convertedPd;
        }

    }
}
