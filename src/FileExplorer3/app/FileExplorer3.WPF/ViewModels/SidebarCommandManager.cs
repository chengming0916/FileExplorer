using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    public class SidebarCommandManager: CommandManagerBase
    {
        #region Constructor

        public SidebarCommandManager(ISidebarViewModel svm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _svm = svm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("Sidebar", _svm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();

            ScriptCommands.TogglePreviewer = Sidebar.Toggle();

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

        ISidebarViewModel _svm;
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
