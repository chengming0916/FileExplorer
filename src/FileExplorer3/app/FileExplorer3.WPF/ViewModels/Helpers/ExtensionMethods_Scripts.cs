using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Script;
using FileExplorer;
using FileExplorer.WPF.ViewModels;

namespace FileExplorer.Script
{
    public static partial class ExtensionMethods
    {
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
