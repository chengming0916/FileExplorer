using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels
{


    public interface INavigationScriptCommandContainer : IScriptCommandContainer
    {
        IScriptCommand Back { get; set; }
        IScriptCommand Next { get; set; }
        IScriptCommand Up { get; set; }   
    }

    public class NavigationScriptCommandContainer : INavigationScriptCommandContainer, IExportCommandBindings
    {
        #region Constructor

        public NavigationScriptCommandContainer(INavigationViewModel nvm, IEventAggregator events)
        {
            ParameterDicConverter =
                ParameterDicConverters.ConvertVMParameter(
                    new Tuple<string, object>("Navigation", nvm),
                    new Tuple<string, object>("Events", events));                

            Back = new SimpleScriptCommand("GoBack", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoBack();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoBack);

            Next = new SimpleScriptCommand("GoNext", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoNext();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoNext);

            Up = new SimpleScriptCommand("GoUp", (pd) =>
            {
                pd.AsVMParameterDic().Navigation.GoUp();
                return ResultCommand.OK;
            }, pd => pd.AsVMParameterDic().Navigation.CanGoUp);

            ExportedCommandBindings = new[] 
            {
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseBack, this, (ch) => ch.Back, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseForward, this, (ch) => ch.Next, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.BrowseHome, this, (ch) => ch.Up, ParameterDicConverter, ScriptBindingScope.Explorer),
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
        public IScriptCommand Back { get; set; }
        public IScriptCommand Next { get; set; }
        public IScriptCommand Up { get; set; }    

        #endregion
    }


}
