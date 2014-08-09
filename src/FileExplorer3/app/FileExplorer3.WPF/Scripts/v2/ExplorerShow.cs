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
        private static IScriptCommand explorerShow(ExplorerMode explorerMode,
            string onModelCreatedVariable = "{OnModelCreated}", string onViewAttachedVariable = "{OnViewAttached}",
            string windowManagerVariable = "{WindowManager}", string eventAggregatorVariable = "{Events}",
            string destinationVariable = "{Explorer}", string dialogResultVariable = "{DialogResult}",
            string selectionEntriesVariable = "{Selection}", string selectionPathsVariable = "{SelectionPaths}",
            IScriptCommand nextCommand = null
            )
        {
            return new ExplorerShow()
            {
                ExplorerMode = explorerMode,
                OnModelCreatedKey = onModelCreatedVariable,
                OnViewAttachedKey = onViewAttachedVariable,
                WindowManagerKey = windowManagerVariable,
                EventAggregatorKey = eventAggregatorVariable,
                DestinationKey = destinationVariable,
                DialogResultKey = dialogResultVariable,
                SelectionEntriesKey = selectionEntriesVariable,
                SelectionPathsKey = selectionPathsVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
        
         public static IScriptCommand ExplorerShow(
            string onModelCreatedVariable = "{OnModelCreated}", string onViewAttachedVariable = "{OnViewAttached}",
            string windowManagerVariable = "{WindowManager}", string eventAggregatorVariable = "{Events}",
            string destinationVariable = "{Explorer}", 
            IScriptCommand nextCommand = null)
        {
            return explorerShow(ExplorerMode.Normal, onModelCreatedVariable, onViewAttachedVariable, windowManagerVariable, 
                eventAggregatorVariable, destinationVariable, null, null, null, nextCommand);
        }

        public static IScriptCommand FileSave(string onModelCreatedVariable = "{OnModelCreated}", string onViewAttachedVariable = "{OnViewAttached}",
            string windowManagerVariable = "{WindowManager}", string eventAggregatorVariable = "{Events}",
            string dialogResultVariable = "{DialogResult}", string selectionPathsVariable = "{SelectionPaths}",
            IScriptCommand nextCommand = null)
        {
            return explorerShow(ExplorerMode.FileSave, onModelCreatedVariable, onViewAttachedVariable,
                windowManagerVariable, eventAggregatorVariable, null, dialogResultVariable, null,
                selectionPathsVariable, nextCommand);
        }

        public static IScriptCommand DirectoryPick(string onModelCreatedVariable = "{OnModelCreated}", string onViewAttachedVariable = "{OnViewAttached}",
            string windowManagerVariable = "{WindowManager}", string eventAggregatorVariable = "{Events}",
            string selectedEntryVariable = "{Selection}", string selectedPathVariable = "{SelectionPath}", 
            IScriptCommand nextCommand = null, IScriptCommand cancelCommand = null)
        {
            string dialogResultVariable = "{DirectoryPick-DialogResult}";

            IScriptCommand onDirectoryPickCreated = 
                ScriptCommands.RunCommandsInSequence(
                    ScriptCommands.RunScriptCommand(onModelCreatedVariable),         
                    UIScriptCommands.ExplorerSetParameter("{Explorer}",
                            ExplorerParameterType.RootModels, "{RootDirectory}"));
            IScriptCommand onDirectoryPickAttached = 
                ScriptCommands.RunCommandsInSequence(
                    ScriptCommands.RunScriptCommand(onViewAttachedVariable),  
                UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot());


            return 
                ScriptCommands.Assign("{OnDirectoryPickCreated}", onDirectoryPickCreated, false, 
                ScriptCommands.Assign("{OnDirectoryPickAttached}", onDirectoryPickAttached, false,                 
                    explorerShow(ExplorerMode.DirectoryOpen, "{OnDirectoryPickCreated}", "{OnDirectoryPickAttached}",
              windowManagerVariable, eventAggregatorVariable, null, dialogResultVariable, selectedEntryVariable,
              selectedPathVariable, 
              ScriptCommands.IfTrue(dialogResultVariable,  nextCommand, cancelCommand))));
        }

        //public static IScriptCommand ExplorerShow(IEntryModel[] rootModels,
        //    bool enableDrag, bool enableDrop, bool enableMultiSelect,
        //    string windowManagerVariable = "{windowManager}", string eventAggregatorVariable = "{Events}",
        //    IScriptCommand nextCommand = null)
        //{
        //    return explorerShow(ExplorerMode.Normal,
        //        ScriptCommands.RunCommandsInSequence(
        //                UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.RootModels, rootModels.ToArray()),
        //                UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrag, enableDrag),
        //                UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableDrop, enableDrop),
        //                UIScriptCommands.ExplorerSetParameters(ExplorerParameterType.EnableMultiSelect, enableMultiSelect)),
        //        ScriptCommands.RunCommandsInSequence(
        //                ScriptCommands.Assign("{Root}", rootModels.FirstOrDefault(), false),
        //                UIScriptCommands.ExplorerGoTo("{Explorer}", "{Root}")),
        //                windowManagerVariable, eventAggregatorVariable, null, null, null, null, nextCommand);

        //}
    }

    public enum ExplorerMode { Normal, FileOpen, FileSave, DirectoryOpen }

    public class ExplorerShow : ScriptCommandBase
    {
        /// <summary>
        /// IScriptCommand to run when the Explorer model is created.
        /// </summary>
        public string OnModelCreatedKey { get; set; }

        /// <summary>
        /// IScriptCommand to run when Explorer view is attached to Explorer model. (UI commands)
        /// </summary>
        public string OnViewAttachedKey { get; set; }

        public ExplorerMode ExplorerMode { get; set; }

        /// <summary>
        /// WindowManager used to show the window, optional, Default={WindowManager}
        /// </summary>
        public string WindowManagerKey { get; set; }

        public string EventAggregatorKey { get; set; }

        public string DestinationKey { get; set; }

        /// <summary>
        /// Point to a boolean, indicate whether the dialog is confirmed or cancel, Default = {DialogResult}
        /// </summary>
        public string DialogResultKey { get; set; }

        /// <summary>
        /// Point to an IEntryModel of selected file or folders, in FileOpenMode only , Default = {Selection}
        /// </summary>
        public string SelectionEntriesKey { get; set; }

        /// <summary>
        /// Point to a string[] (string if FileSave) of selected file or folders, Default = {SelectionPaths}
        /// </summary>
        public string SelectionPathsKey { get; set; }        

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ExplorerShow>();

        public ExplorerShow()
            : base("ExplorerShow")
        {
            WindowManagerKey = "{WindowManager}";
            EventAggregatorKey = "{Events}";
            ExplorerMode = Script.ExplorerMode.Normal;
            OnModelCreatedKey = "{OnModelCreated}";
            OnViewAttachedKey = "{OnViewAttached}";
            DestinationKey = "{Explorer}";
            DialogResultKey = "{DialogResult}";
            SelectionEntriesKey = "{Selection}";
            SelectionPathsKey = "{SelectionPaths}";
            ContinueOnCaptureContext = true;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IWindowManager wm = pm.GetValue<IWindowManager>(WindowManagerKey) ?? new WindowManager();
            IEventAggregator events = pm.GetValue<IEventAggregator>(EventAggregatorKey) ?? new EventAggregator();

            IScriptCommand modelCreatedCommands = pm.GetValue<IScriptCommand>(OnModelCreatedKey) ?? ResultCommand.NoError;
            IScriptCommand viewAttachedCommands = pm.GetValue<IScriptCommand>(OnViewAttachedKey) ?? ResultCommand.NoError;

            IExplorerInitializer initializer = new ScriptCommandInitializer()
             {
                 StartupParameters = pm,
                 OnModelCreated = new IScriptCommand[] { modelCreatedCommands },
                 OnViewAttached = new IScriptCommand[] { viewAttachedCommands }
             };

            ExplorerViewModel evm = null;
            switch (ExplorerMode)
            {
                case Script.ExplorerMode.Normal:
                    evm = new ExplorerViewModel(wm, events) { Initializer = initializer };
                    break;
                case Script.ExplorerMode.FileOpen:
                    evm = new FilePickerViewModel(wm, events)
                    {
                        Initializer = initializer,
                        PickerMode = FilePickerMode.Open
                    };
                    break;
                case Script.ExplorerMode.FileSave:
                    evm = new FilePickerViewModel(wm, events)
                    {
                        Initializer = initializer,
                        PickerMode = FilePickerMode.Save
                    };
                    break;
                case Script.ExplorerMode.DirectoryOpen :
                    evm = new DirectoryPickerViewModel(wm, events)
                        {
                            Initializer = initializer
                        };
                    break;
                default:
                    return ResultCommand.Error(new NotSupportedException(ExplorerMode.ToString()));
            }

            logger.Info(String.Format("Showing {0}", evm));
            pm.SetValue(DestinationKey, evm, false);

            if (ExplorerMode == Script.ExplorerMode.Normal)
                wm.ShowWindow(evm);
            else
            {
                bool result = wm.ShowDialog(evm).Value;
                pm.SetValue(DialogResultKey, result);
                if (result)
                {
                    FilePickerViewModel fpvm = evm as FilePickerViewModel;
                    DirectoryPickerViewModel dpvm = evm as DirectoryPickerViewModel;
                    switch (ExplorerMode)
                    {
                        case Script.ExplorerMode.FileSave:
                            pm.SetValue(SelectionPathsKey, fpvm.FileName);
                            break;
                        case Script.ExplorerMode.FileOpen:
                            pm.SetValue(SelectionPathsKey, fpvm.SelectedFiles.Select(m => m.FullPath));
                            pm.SetValue(SelectionEntriesKey, fpvm.SelectedFiles);
                            break;
                        case Script.ExplorerMode.DirectoryOpen: 
                            pm.SetValue(SelectionPathsKey, dpvm.SelectedDirectory.FullPath);
                            pm.SetValue(SelectionEntriesKey, dpvm.SelectedDirectory);
                            break;
                    }


                }
            }

            return NextCommand;
        }
    }
}
