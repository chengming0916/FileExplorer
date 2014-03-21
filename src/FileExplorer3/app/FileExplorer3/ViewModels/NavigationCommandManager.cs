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

namespace FileExplorer.ViewModels
{
    public class NavigationCommandManager : CommandManagerBase
    {
        #region Constructor

        public NavigationCommandManager(INavigationViewModel nvm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _nvm = nvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("Navigation", _nvm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();
            ScriptCommands.Back = new SimpleScriptCommand("GoBack", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoBack();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoBack);

            ScriptCommands.Next = new SimpleScriptCommand("GoNext", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoNext();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoNext);

            ScriptCommands.Up = new SimpleScriptCommand("GoUp", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoUp();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoUp);
 

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(                    
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseBack, this, (ch) => ch.ScriptCommands.Back, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseForward, this, (ch) => ch.ScriptCommands.Next, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseHome, this, (ch) => ch.ScriptCommands.Up, ParameterDicConverter, ScriptBindingScope.Explorer)
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

        private INavigationViewModel _nvm;

        #endregion

        #region Public Properties

        #endregion
    }
}
