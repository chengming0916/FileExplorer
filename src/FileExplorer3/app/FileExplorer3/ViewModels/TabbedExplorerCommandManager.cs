using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Utils;
using FileExplorer.BaseControls;
using System.Windows.Input;
using FileExplorer.ViewModels.Helpers;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels
{
    public class TabbedExplorerCommandManager : CommandManagerBase
    {
        #region Constructor

        public TabbedExplorerCommandManager(ITabbedExplorerViewModel nvm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _tevm = nvm;
            
            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("TabbedExplorer", _tevm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();
            ScriptCommands.NewWindow = TabbedExplorer.NewWindow;
            ScriptCommands.CloseWindow = TabbedExplorer.CloseWindow;

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(                    
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewWindow, this, (ch) => ch.ScriptCommands.NewWindow, ParameterDicConverter, ScriptBindingScope.Application),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.New, this, (ch) => ch.ScriptCommands.NewWindow, ParameterDicConverter, ScriptBindingScope.Application),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.CloseWindow, this, (ch) => ch.ScriptCommands.CloseWindow, ParameterDicConverter, ScriptBindingScope.Application)
                ));

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

        private ITabbedExplorerViewModel _tevm;

        #endregion

        #region Public Properties

        #endregion
    }
}
