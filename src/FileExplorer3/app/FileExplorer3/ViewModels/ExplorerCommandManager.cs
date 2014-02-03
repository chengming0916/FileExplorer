using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Defines;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
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
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();
            ScriptCommands.Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                IExplorerViewModel elvm = pd.AsVMParameterDic().Explorer;
                elvm.FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                elvm.DirectoryTree.Selection.RootSelector.SelectedViewModel.Entries.LoadAsync(true);
                return ResultCommand.NoError;
            });
            
            ScriptCommands.Transfer = NullScriptCommand.Instance;

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.Add(
              new ExportCommandBindings(
              ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.ScriptCommands.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer)
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
