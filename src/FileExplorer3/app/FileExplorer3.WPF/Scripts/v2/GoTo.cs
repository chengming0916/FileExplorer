using FileExplorer.Models;
using FileExplorer.WPF.ViewModels;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class UIScriptCommands
    {
        /// <summary>
        /// Serializable, Go to directory specified in directoryVariable.
        /// </summary>
        /// <param name="explorerVariable"></param>
        /// <param name="directoryVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand GoTo(string explorerVariable = "{Explorer}", string directoryVariable = "{Directory}", 
            IScriptCommand nextCommand = null)
        {
            return new GoTo()
            {
                NextCommand = (ScriptCommandBase)nextCommand,
                ExplorerKey = explorerVariable, 
                DirectoryEntryKey = directoryVariable
            };
        }

        /// <summary>
        /// Not serializable, goto the specified directory.
        /// </summary>
        /// <param name="explorerVariable"></param>
        /// <param name="directory"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand GoTo(string explorerVariable = "{Explorer}", IEntryModel directory = null, 
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign("{Goto-Directory}", directory, false,
                GoTo(explorerVariable, "{Goto-Directory}", nextCommand));
        }

        public static IScriptCommand GoTo(IEntryModel directory = null,
           IScriptCommand nextCommand = null)
        {
            return GoTo("{Explorer}", directory, nextCommand);
        }
    }

    class GoTo : ScriptCommandBase
    {
        /// <summary>
        /// Point to Explorer (IExplorerViewModel) to be used.  Default = "{Explorer}".
        /// </summary>
        public string ExplorerKey { get; set; }

        /// <summary>
        /// Point to Directory (IEntryModel) to be opened.  Default = "{Directory}", 
        /// if not specified root directory will be opened.
        /// </summary>
        public string DirectoryEntryKey { get; set; }

 
        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<GoTo>();

        public GoTo()
            : base("GoToDirectory")
        {
            ExplorerKey = "{Explorer}";            
            DirectoryEntryKey = "{Directory}";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {

            var evm = pm.GetValue<IExplorerViewModel>(ExplorerKey);
            if (evm == null)
                return ResultCommand.Error(new ArgumentNullException(ExplorerKey));

            var dm = DirectoryEntryKey == null ? null :
                (await pm.GetValueAsEntryModelArrayAsync(DirectoryEntryKey)).FirstOrDefault();

            await evm.GoAsync(dm);
            return ResultCommand.NoError;
        }        
    }
}
