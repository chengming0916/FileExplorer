using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.Defines;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using System.Windows.Input;
using FileExplorer.WPF.Defines;
using FileExplorer.Utils;

namespace FileExplorer.WPF.ViewModels
{
    public class ExplorerCommandManager : CommandManagerBase
    {
        #region Constructor

        public ExplorerCommandManager(IExplorerViewModel evm, IEventAggregator events,
             params ISupportCommandManager[] cMs)
            : this(evm, events, cMs.Select(cm => cm.Commands).ToArray())
        {

        }

        public ExplorerCommandManager(IExplorerViewModel evm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _evm = evm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("Explorer", _evm),                 
                 new Tuple<string, object>("DirectoryTree", _evm.DirectoryTree),
                 new Tuple<string, object>("FileList", _evm.FileList),
                 new Tuple<string, object>("Statusbar", _evm.Statusbar),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            CommandDictionary = new DynamicRelayCommandDictionary() { ParameterDicConverter = ParameterDicConverter };
            CommandDictionary.Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                IExplorerViewModel elvm = pd.AsVMParameterDic().Explorer;
                elvm.FileList.ProcessedEntries.EntriesHelper.LoadAsync(UpdateMode.Replace, true);
                elvm.DirectoryTree.Selection.RootSelector.SelectedViewModel.Entries.LoadAsync(UpdateMode.Replace, true);
                return ResultCommand.NoError;
            });
            
            CommandDictionary.Transfer = NullScriptCommand.Instance;
            CommandDictionary.ZoomIn = Explorer.Zoom(ZoomMode.ZoomIn);
            CommandDictionary.ZoomOut = Explorer.Zoom(ZoomMode.ZoomOut);
            CommandDictionary.CloseTab = NullScriptCommand.Instance;

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.Add(
              new ExportCommandBindings(
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.IncreaseZoom, this, (ch) => ch.CommandDictionary.ZoomIn, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.DecreaseZoom, this, (ch) => ch.CommandDictionary.ZoomOut, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.CommandDictionary.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.CloseTab, this, (ch) => ch.CommandDictionary.CloseTab, ParameterDicConverter, ScriptBindingScope.Explorer)
                //ScriptCommandBinding.FromScriptCommand(ExplorerCommands.CloseWindow, this, (ch) => ch.ScriptCommands.Close, ParameterDicConverter, ScriptBindingScope.Explorer)
              ));
            exportBindingSource.AddRange(additionalBindingExportSource);
          

            _exportBindingSource = exportBindingSource.ToArray();

             ToolbarCommands = new ToolbarCommandsHelper(events,
                null,
                null)
                {
                };
        }
        
        #endregion

        #region Methods
        
        #endregion

        #region Data

        IExplorerViewModel _evm;
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
