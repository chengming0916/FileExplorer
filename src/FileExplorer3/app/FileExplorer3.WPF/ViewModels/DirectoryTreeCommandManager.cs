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
using FileExplorer.Utils;

namespace FileExplorer.WPF.ViewModels
{
    public class DirectoryTreeCommandManager : CommandManagerBase
    {

        #region Constructor

        public DirectoryTreeCommandManager(IDirectoryTreeViewModel dlvm, IWindowManager windowManager, 
            IEventAggregator events,  params IExportCommandBindings[] additionalBindingExportSource)
            : base(additionalBindingExportSource)
        {
            _dlvm = dlvm;
            InitCommandManager();

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

        protected override IParameterDicConverter setupParamDicConverter()
        {
            return ParameterDicConverters.ConvertVMParameter(new Tuple<string, object>("DirectoryTree", _dlvm));
        }



        protected override IEnumerable<string> getScriptCommands()
        {
            yield return "Delete";
            yield return "ToggleRename";
            yield return "Open";
            yield return "OpenTab";
            yield return "NewWindow";
            yield return "Map";
            yield return "Unmap";
        }

        protected override void setupScriptCommands(dynamic commandDictionary)
        {
            commandDictionary.ToggleRename = DirectoryTree.ToggleRename;
            commandDictionary.Open = DirectoryTree.ExpandSelected;
            commandDictionary.Unmap = Explorer.DoSelection(ems =>
                Script.ScriptCommands.If(pd => (ems.First() as IDirectoryNodeViewModel).Selection.IsFirstLevelSelector(),
                        Script.WPFScriptCommands.IfOkCancel(new WindowManager(), pd => "Unmap",
                            pd => String.Format("Unmap {0}?", ems.First().EntryModel.Label),
                            Explorer.BroadcastRootChanged(RootChangedEvent.Deleted(ems.Select(em => em.EntryModel).ToArray())),
                            ResultCommand.OK),
                        Script.ScriptCommands.NoCommand), Script.ScriptCommands.NoCommand);
        }

        protected override IExportCommandBindings[] setupExportBindings()
        {
            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.Add(
                new ExportCommandBindings(

                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.CommandDictionary.Open, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.CommandDictionary.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.CommandDictionary.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.OpenTab, this, (ch) => ch.CommandDictionary.OpenTab, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewWindow, this, (ch) => ch.CommandDictionary.NewWindow, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Map, this, (ch) => ch.CommandDictionary.Map, ParameterDicConverter, ScriptBindingScope.Explorer),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Unmap, this, (ch) => ch.CommandDictionary.Unmap, ParameterDicConverter, ScriptBindingScope.Local)
                ));

            return exportBindingSource.ToArray();
        }

        #endregion

        #region Data

        private IDirectoryTreeViewModel _dlvm;

        #endregion

        #region Public Properties

        #endregion
    }
}
