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

namespace FileExplorer.WPF.ViewModels
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

            Commands = new DynamicDictionary<IScriptCommand>();
            Commands.Back = new SimpleScriptCommand("GoBack", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoBack();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoBack);

            Commands.Next = new SimpleScriptCommand("GoNext", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoNext();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoNext);

            Commands.Up = new SimpleScriptCommand("GoUp", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoUp();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoUp);
 

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(                    
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseBack, this, (ch) => ch.Commands.Back, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseForward, this, (ch) => ch.Commands.Next, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseHome, this, (ch) => ch.Commands.Up, ParameterDicConverter, ScriptBindingScope.Explorer)
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
