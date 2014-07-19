using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.Defines;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.Defines;
using FileExplorer.Models;

namespace FileExplorer.WPF.ViewModels
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

            Commands = new DynamicDictionary<IScriptCommand>();
            Commands.Delete = NullScriptCommand.Instance;
            Commands.ToggleRename = DirectoryTree.ToggleRename;
            Commands.Open = DirectoryTree.ExpandSelected;
            Commands.OpenTab = NullScriptCommand.Instance;
            Commands.NewWindow = NullScriptCommand.Instance;
            Commands.Map = NullScriptCommand.Instance;
            Commands.Unmap = Explorer.DoSelection(ems =>
                Script.ScriptCommands.If(pd => (ems.First() as IDirectoryNodeViewModel).Selection.IsFirstLevelSelector(),
                        Script.WPFScriptCommands.IfOkCancel(windowManager, pd => "Unmap",  
                            pd => String.Format("Unmap {0}?", ems.First().EntryModel.Label), 
                            Explorer.BroadcastRootChanged(RootChangedEvent.Deleted(ems.Select(em => em.EntryModel).ToArray())),
                            ResultCommand.OK),
                        Script.ScriptCommands.NoCommand), Script.ScriptCommands.NoCommand); 

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(
                    
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.Commands.Open, ParameterDicConverter, ScriptBindingScope.Local),           
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.Commands.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.Commands.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.OpenTab, this, (ch) => ch.Commands.OpenTab, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewWindow, this, (ch) => ch.Commands.NewWindow, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Map, this, (ch) => ch.Commands.Map, ParameterDicConverter, ScriptBindingScope.Explorer),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Unmap, this, (ch) => ch.Commands.Unmap, ParameterDicConverter, ScriptBindingScope.Local)
                ));

            _exportBindingSource = exportBindingSource.ToArray();

             ToolbarCommands = new ToolbarCommandsHelper(events, ParameterDicConverter,
                message => new[] { message.NewModel },
                null)
                {
                   ExtraCommandProviders = new[] { 
                        new StaticCommandProvider(
                    new CommandModel(ApplicationCommands.New){ IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.Refresh) { IsVisibleOnMenu = true },
                    new CommandModel(ApplicationCommands.Delete){ IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnMenu = true },
                   
                    new CommandModel(ExplorerCommands.Map)  { 
                        HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE17B),
                        //Symbol = Convert.ToChar(0xE17B), 
                        IsEnabled = true,
                        IsHeaderVisible = false, IsVisibleOnToolbar = true
                    },

                    new CommandModel(ExplorerCommands.Unmap)  {
                        HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE17A),
                     IsVisibleOnToolbar = true, IsVisibleOnMenu = true
                    }
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
