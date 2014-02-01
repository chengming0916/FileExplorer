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

namespace FileExplorer.ViewModels
{
    public class DirectoryTreeCommandManager : CommandManagerBase
    {
        
        #region Constructor

        public DirectoryTreeCommandManager(IDirectoryTreeViewModel dlvm, IEventAggregator events,
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

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(                                    
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.ScriptCommands.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.ScriptCommands.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local)
                ));

            _exportBindingSource = exportBindingSource.ToArray();

             ToolbarCommands = new ToolbarCommandsHelper(events,
                message => new[] { message.NewModel },
                null)
                {
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
