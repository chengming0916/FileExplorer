using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.WPF.ViewModels.Helpers;
using Cofe.Core.Script;
using Cofe.Core;
using FileExplorer.WPF.ViewModels;

namespace FileExplorer.WPF
{
    public static partial class ExtensionMethods
    {
        public static ITreeRootSelector<VM, T> AsRoot<VM, T>(this ITreeSelector<VM, T> selector)
        {
            return selector as ITreeRootSelector<VM, T>;
        }

        /// <summary>
        /// Whether current directory is root directory
        /// </summary>
        /// <typeparam name="VM"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static bool IsFirstLevelSelector<VM, T>(this ITreeSelector<VM, T> selector)
        {
            return selector.ParentSelector.Equals(selector.RootSelector);
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


        /// <summary>
        /// Execute an Explorer command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterDic">Will be merged with parameters from ParameterDicConverter</param>
        /// <param name="scriptRunner"></param>
        public static void Execute(this ICommandManager commandManager, IScriptCommand[] commands, ParameterDic parameterDic = null, IScriptRunner scriptRunner = null)
        {
            scriptRunner = scriptRunner ?? new ScriptRunner();
            scriptRunner.Run(commandManager.ParameterDicConverter.ConvertAndMerge(parameterDic), commands);
        }
        /// <summary>
        /// Execute an Explorer command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterDic"></param>
        /// <param name="scriptRunner"></param>
        /// <returns></returns>
        public static async Task ExecuteAsync(this ICommandManager commandManager, IScriptCommand[] commands, ParameterDic parameterDic = null, IScriptRunner scriptRunner = null)
        {
            scriptRunner = scriptRunner ?? new ScriptRunner();
            await scriptRunner.RunAsync(commandManager.ParameterDicConverter.ConvertAndMerge(parameterDic), commands);
        }
    }
}
