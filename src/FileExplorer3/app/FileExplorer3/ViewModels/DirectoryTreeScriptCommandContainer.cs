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
using FileExplorer.Defines;

namespace FileExplorer.ViewModels
{
    public interface IDirectoryTreeScriptCommandContainer : IScriptCommandContainer
    {
        IScriptCommand Refresh { get; set; }
        IScriptCommand Delete { get; set; }
        IScriptCommand ToggleRename { get; set; }
    }

    public class DirectoryTreeScriptCommandContainer : IDirectoryTreeScriptCommandContainer, IExportCommandBindings
    {
        #region Constructor

        public DirectoryTreeScriptCommandContainer(IDirectoryTreeViewModel dlvm, IEventAggregator events)
        {
            ParameterDicConverter =
                ParameterDicConverters.ConvertVMParameter(
                    new Tuple<string, object>("DirectoryTree", dlvm),
                    new Tuple<string, object>("Events", events));
            
            Delete = NullScriptCommand.Instance;

            Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                return ResultCommand.OK;
            });

            ToggleRename = DirectoryTree.ToggleRename;

            ExportedCommandBindings = new[] 
            {                
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),
            };
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IParameterDicConverter ParameterDicConverter { get; private set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get; private set; }
        public IScriptCommand Refresh { get; set; }
        public IScriptCommand Delete { get; set; }
        public IScriptCommand ToggleRename { get; set; }

        #endregion
    }
}
