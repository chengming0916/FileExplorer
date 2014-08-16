using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.BaseControls;
using System.Windows.Input;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Defines;
using FileExplorer.WPF.Defines;
using FileExplorer.Utils;

namespace FileExplorer.WPF.ViewModels
{
    public class TabbedExplorerCommandManager : CommandManagerBase
    {
        #region Constructor

        public TabbedExplorerCommandManager(ITabbedExplorerViewModel nvm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
            : base(additionalBindingExportSource)
        {
            _tevm = nvm;
            _events = events;
            InitCommandManager();

             ToolbarCommands = new ToolbarCommandsHelper(events,
                null,
                null)
                {
                };

             
        }

        #endregion

        #region Methods

        protected override IParameterDicConverter setupParamDicConverter()
        {
            return ParameterDicConverters.ConvertVMParameter(new Tuple<string, object>("TabbedExplorer", _tevm), 
                new Tuple<string, object>("Events", _events));
        }



        protected override IEnumerable<string> getScriptCommands()
        {
            yield return "NewTab";
            yield return "CloseTab";
        }

        protected override void setupScriptCommands(dynamic commandDictionary)
        {            
            commandDictionary.NewTab = UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", null);
            commandDictionary.CloseTab =
                    UIScriptCommands.TabExplorerCloseTab("{TabbedExplorer}");
        }

        protected override IExportCommandBindings[] setupExportBindings()
        {
            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.Add(
                new ExportCommandBindings(
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewTab, this, (ch) => ch.CommandDictionary.NewTab, ParameterDicConverter, ScriptBindingScope.Application),
                //ScriptCommandBinding.FromScriptCommand(ApplicationCommands.New, this, (ch) => ch.ScriptCommands.NewTab, ParameterDicConverter, ScriptBindingScope.Application),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.CloseTab, this, (ch) => ch.CommandDictionary.CloseTab, ParameterDicConverter, ScriptBindingScope.Application)
                ));

            return exportBindingSource.ToArray();
        }

        #endregion

        #region Data

        private ITabbedExplorerViewModel _tevm;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        #endregion
    }
}
