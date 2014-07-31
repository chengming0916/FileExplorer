using Caliburn.Micro;
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
        public static IScriptCommand ExplorerShow(
            IScriptCommand onModelCreated, IScriptCommand onViewAttached,
            string windowManagerVariable = "{windowManager}", string eventAggregatorVariable = "{Events}", 
            IScriptCommand nextCommand = null
            )
        {
            return new ExplorerShow()
            {
                ModelCreatedCommands = (ScriptCommandBase)onModelCreated, 
                ViewAttachedCommands = (ScriptCommandBase)onViewAttached, 
                WindowManagerKey = windowManagerVariable, 
                EventAggregatorKey = eventAggregatorVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand ExplorerShow(IEntryModel[] rootModels,             
            bool enableDrag, bool enableDrop, bool enableMultiSelect,
            string windowManagerVariable = "{windowManager}", string eventAggregatorVariable = "{Events}", 
            IScriptCommand nextCommand = null)
        {
            return ExplorerShow(
                ScriptCommands.RunCommandsInSequence(
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.RootModels, rootModels.ToArray()),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrag, enableDrag),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrop, enableDrop),
                        UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableMultiSelect, enableMultiSelect)), 
                ScriptCommands.RunCommandsInSequence(
                        ScriptCommands.Assign("{Root}", rootModels.FirstOrDefault(), false),
                        UIScriptCommands.ExplorerGoTo("{Explorer}", "{Root}")), 
                        windowManagerVariable, eventAggregatorVariable, nextCommand);

        }
    }


    public class ExplorerShow : ScriptCommandBase
    {
        /// <summary>
        /// Run when the Explorer model is created.
        /// </summary>
        public ScriptCommandBase ModelCreatedCommands { get; set; }

        /// <summary>
        /// Run when Explorer view is attached to Explorer model. (UI commands)
        /// </summary>
        public ScriptCommandBase ViewAttachedCommands { get; set; }

        ///// <summary>
        ///// Profile to use.
        ///// </summary>
        //public string[] ProfilesKey { get; set; }

        public string WindowManagerKey { get; set; }

        public string EventAggregatorKey { get; set; }

        public string DestinationKey { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ExplorerShow>();

        public ExplorerShow()
            : base("ExplorerShow")
        {
            WindowManagerKey = "{WindowManager}";
            EventAggregatorKey = "{Events}";
            ModelCreatedCommands = ResultCommand.NoError;
            ViewAttachedCommands = ResultCommand.NoError;
            DestinationKey = "{Explorer}";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IWindowManager wm = pm.GetValue<IWindowManager>(WindowManagerKey) ?? new WindowManager();
            IEventAggregator events = pm.GetValue<IEventAggregator>(EventAggregatorKey) ?? new EventAggregator();

            IExplorerInitializer initializer = new ScriptCommandInitializer()
             {
                 StartupParameters = pm,
                 OnModelCreated = new IScriptCommand[] { ModelCreatedCommands },
                 OnViewAttached = new IScriptCommand[] { ViewAttachedCommands }
             };
            
            ExplorerViewModel evm = new ExplorerViewModel(wm, events) { Initializer = initializer };
            logger.Info(String.Format("Showing {0}", evm));
            pm.SetValue(DestinationKey, evm, false);
            wm.ShowWindow(evm);

            return NextCommand;
        }
    }
}
