using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Defines;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public class DirectoryTreeCommandManager : CommandManagerBase
    {
        
        #region Constructor

        public DirectoryTreeCommandManager(IDirectoryTreeViewModel dlvm, IWindowManager windowManager, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _dlvm = dlvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("DirectoryTree", _dlvm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();
            ScriptCommands.Delete = NullScriptCommand.Instance;
            ScriptCommands.ToggleRename = DirectoryTree.ToggleRename;
            ScriptCommands.Open = DirectoryTree.ExpandSelected;
            ScriptCommands.NewWindow = NullScriptCommand.Instance;
            ScriptCommands.Map = NullScriptCommand.Instance;
            ScriptCommands.Unmap = Explorer.DoSelection(ems =>
                ViewModels.ScriptCommands.If(pd => (ems.First() as IDirectoryNodeViewModel).Selection.IsFirstLevelSelector(),
                        ViewModels.ScriptCommands.IfOkCancel(windowManager, pd => "Unmap",  
                            pd => String.Format("Unmap {0}?", ems.First().EntryModel.Label), 
                            Explorer.BroadcastRootChanged(RootChangedEvent.Deleted(this._dlvm, ems.Select(em => em.EntryModel).ToArray())),
                            ResultCommand.OK),
                        ViewModels.ScriptCommands.NoCommand), ViewModels.ScriptCommands.NoCommand); 

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(
                    
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.ScriptCommands.Open, ParameterDicConverter, ScriptBindingScope.Local),           
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.ScriptCommands.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.ScriptCommands.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewWindow, this, (ch) => ch.ScriptCommands.NewWindow, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Map, this, (ch) => ch.ScriptCommands.Map, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Unmap, this, (ch) => ch.ScriptCommands.Unmap, ParameterDicConverter, ScriptBindingScope.Local)
                ));

            _exportBindingSource = exportBindingSource.ToArray();

             ToolbarCommands = new ToolbarCommandsHelper(events,
                message => new[] { message.NewModel },
                null)
                {
                   ExtraCommandProviders = new[] { 
                        new StaticCommandProvider(
                    new CommandModel(ApplicationCommands.New) { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Refresh) { IsVisibleOnToolbar = false },
                    new CommandModel(ApplicationCommands.Delete)  { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Map)  { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Unmap)  { IsVisibleOnToolbar = false }
                    )}
                };
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private IDirectoryTreeViewModel _dlvm;

        #endregion

        #region Public Properties

        #endregion
    }
}
