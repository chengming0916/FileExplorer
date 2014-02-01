using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Manage script commands for ViewModels
    /// </summary>
    public interface IScriptCommandManager : IExportCommandBindings
    {
        /// <summary>
        /// A number of parameter have to be added to ParameterDic to run commands in ScriptCommands (e.g. FileList), 
        /// use this converter to add the parameter.
        /// </summary>
        IParameterDicConverter ParameterDicConverter { get; }
        
        /// <summary>
        /// An IScriptCommand DynamicDictionary, include changable commands.
        /// </summary>
        dynamic ScriptCommands { get; }

        /// <summary>
        /// Return a list of Commands for Toolbar and ContextMenu.
        /// </summary>
        IToolbarCommandsHelper ToolbarCommands { get; }
    }

    //public class ScriptCommandManager : IScriptCommandManager
    //{
    //    #region Constructor

    //    public ScriptCommandManager()
    //    {
    //        Commands = new DynamicDictionary<IScriptCommand>();
    //        Toolbar = null;
    //    }

    //    #endregion

    //    #region Methods

    //    #endregion

    //    #region Data

    //    #endregion

    //    #region Public Properties

    //    public dynamic Commands { get; private set; }        
    //    public IToolbarCommandsHelper Toolbar { get; private set; }

        
    //    #endregion


        
    //}
}
