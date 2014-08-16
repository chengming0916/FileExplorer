using Caliburn.Micro;
using FileExplorer.WPF.ViewModels;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public class TabbedExplorerShow : ScriptCommandBase
    {
        /// <summary>
        /// IScriptCommand to run when the Explorer model is created.
        /// </summary>
        public string OnModelCreatedKey { get; set; }

        /// <summary>
        /// IScriptCommand to run when Explorer view is attached to Explorer model. (UI commands)
        /// </summary>
        public string OnViewAttachedKey { get; set; }

        /// <summary>
        /// IScriptCommand to run when the TabbedExplorer model is created.
        /// </summary>
        public string OnTabExplorerCreatedKey { get; set; }

        /// <summary>
        /// IScriptCommand to run when TabbedExplorer view is attached to TabbedExplorer model. (UI commands)
        /// </summary>
        public string OnTabExplorerAttachedKey { get; set; }

        /// <summary>
        /// WindowManager used to show the window, optional, Default={WindowManager}
        /// </summary>
        public string WindowManagerKey { get; set; }

        /// <summary>
        /// Global Event Aggregator, Default={Events}
        /// </summary>
        public string EventAggregatorKey { get; set; }


        /// <summary>
        /// Output the ITabbedExplorerViewModel, Default= {TabExplorer}
        /// </summary>
        public string DestinationKey { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<TabbedExplorerShow>();

        public TabbedExplorerShow()
            : base("TabbedExplorerShow")
        {
            WindowManagerKey = "{WindowManager}";
            EventAggregatorKey = "{Events}";
            OnModelCreatedKey = "{OnModelCreated}";
            OnTabExplorerCreatedKey = "{OnTabExplorerCreated}";
            OnViewAttachedKey = "{OnViewAttached}";
            OnTabExplorerAttachedKey = "{OnTabExplorerAttachedKey}";
            DestinationKey = "{Explorer}";
            ContinueOnCaptureContext = true;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IWindowManager wm = pm.GetValue<IWindowManager>(WindowManagerKey) ?? new WindowManager();
            IEventAggregator events = pm.GetValue<IEventAggregator>(EventAggregatorKey) ?? new EventAggregator();

            IExplorerInitializer initializer = new ScriptCommandInitializer()
            {
                StartupParameters = pm,
                WindowManager = wm,
                Events = events,
                OnModelCreated = ScriptCommands.RunScriptCommand(OnModelCreatedKey),
                OnViewAttached = ScriptCommands.RunScriptCommand(OnViewAttachedKey)
            };

            TabbedExplorerViewModel tevm = new TabbedExplorerViewModel() { Initializer = initializer };
            await tevm.Commands.ExecuteAsync(pm.GetValue<IScriptCommand>(OnTabExplorerCreatedKey));

            logger.Info(String.Format("Showing {0}", tevm));
            wm.ShowWindow(tevm);
            

            return NextCommand;
        }

    }
}
