using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.WPF.Defines;
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
        public static IScriptCommand ExplorerGoTo(string explorerVariable = "{Explorer}", string directoryVariable = "{Directory}",
            IScriptCommand nextCommand = null)
        {
            return new ExplorerGoTo()
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
        public static IScriptCommand ExplorerGoTo(string explorerVariable = "{Explorer}", IEntryModel directory = null,
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign("{Goto-Directory}", directory, false,
                ExplorerGoTo(explorerVariable, "{Goto-Directory}", nextCommand));
        }

        public static IScriptCommand ExplorerGoTo(IEntryModel directory = null,
           IScriptCommand nextCommand = null)
        {
            return ExplorerGoTo("{Explorer}", directory, nextCommand);
        }
    }

    class ExplorerGoTo : ScriptCommandBase
    {
        /// <summary>
        /// Point to Events (IEventAggregator), this is used if Explorer is not found. Default = "{Events}".
        /// </summary>
        public string EventsKey { get; set; }

        /// <summary>
        /// Point to Explorer (IExplorerViewModel) to be used.  Default = "{Explorer}".        
        /// </summary>
        public string ExplorerKey { get; set; }

        /// <summary>
        /// Point to Directory (IEntryModel) to be opened.  Default = "{Directory}", 
        /// if not specified root directory will be opened.
        /// </summary>
        public string DirectoryEntryKey { get; set; }



        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ExplorerGoTo>();

        public ExplorerGoTo()
            : base("GoToDirectory")
        {
            EventsKey = "{Events}";
            ExplorerKey = "{Explorer}";
            DirectoryEntryKey = "{Directory}";
            ContinueOnCaptureContext = true;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {

            var evm = pm.GetValue<IExplorerViewModel>(ExplorerKey);
            var dm = DirectoryEntryKey == null ? null :
               (await pm.GetValueAsEntryModelArrayAsync(DirectoryEntryKey)).FirstOrDefault();

            if (evm == null)
            {
                var events = pm.GetValue<IEventAggregator>(EventsKey);
                if (events != null)
                    events.PublishOnUIThread(new DirectoryChangedEvent(this, dm, null));
                else return ResultCommand.Error(new ArgumentNullException(ExplorerKey));
            }
            else
            {
                await evm.GoAsync(dm);
            }

            logger.Info("Path = " + dm.FullPath);
            return NextCommand;
        }
    }
}
