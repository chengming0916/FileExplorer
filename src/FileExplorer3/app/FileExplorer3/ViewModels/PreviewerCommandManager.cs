using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Defines;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public class PreviewerCommandManager: CommandManagerBase
    {
        #region Constructor

        public PreviewerCommandManager(IPreviewerViewModel pvm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _pvm = pvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("Previewer", _pvm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();

            ScriptCommands.TogglePreviewer = Previewer.Toggle();

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.Add(
              new ExportCommandBindings(
                  ScriptCommandBinding.FromScriptCommand(ExplorerCommands.TogglePreviewer, this, (ch) => ch.ScriptCommands.TogglePreviewer, ParameterDicConverter, ScriptBindingScope.Explorer)
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

        IPreviewerViewModel _pvm;
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
