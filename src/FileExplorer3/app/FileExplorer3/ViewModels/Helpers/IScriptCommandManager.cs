using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;
using Cofe.Core;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Manage script commands for ViewModels
    /// </summary>
    public interface ICommandManager : IExportCommandBindings
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

        /// <summary>
        /// Execute an Explorer command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterDic">Will be merged with parameters from ParameterDicConverter</param>
        /// <param name="scriptRunner"></param>
        void Execute(IScriptCommand[] commands, ParameterDic parameterDic = null, IScriptRunner scriptRunner = null);

        /// <summary>
        /// Execute an Explorer command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterDic"></param>
        /// <param name="scriptRunner"></param>
        /// <returns></returns>
        Task ExecuteAsync(IScriptCommand[] commands, ParameterDic parameterDic = null, IScriptRunner scriptRunner = null);
    }

    public class CommandManagerBase : ICommandManager
    {
        #region Constructor

        public CommandManagerBase()
        {
        }

        #endregion

        #region Methods

        private IEnumerable<IScriptCommandBinding> getCommandBindings()
        {
            return _exportBindingSource.SelectMany(eb => eb.ExportedCommandBindings);
        }

        public void Execute(IScriptCommand[] commands, ParameterDic parameterDic = null, IScriptRunner scriptRunner = null)
        {
            scriptRunner = scriptRunner ?? new ScriptRunner();
            scriptRunner.Run(ParameterDicConverter.ConvertAndMerge(parameterDic), commands);
        }

        public async Task ExecuteAsync(IScriptCommand[] commands, ParameterDic parameterDic = null, IScriptRunner scriptRunner = null)
        {
            scriptRunner = scriptRunner ?? new ScriptRunner();
            await scriptRunner.RunAsync(ParameterDicConverter.ConvertAndMerge(parameterDic), commands);
        }

        #endregion

        #region Data

        protected IExportCommandBindings[] _exportBindingSource = new IExportCommandBindings[] { };

        #endregion

        #region Public Properties

        public IParameterDicConverter ParameterDicConverter { get; protected set; }
        public dynamic ScriptCommands { get; protected set; }

        public IToolbarCommandsHelper ToolbarCommands { get; protected set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get { return getCommandBindings(); } }

        


        #endregion





    
    }

    public interface ISupportCommandManager 
    {
        ICommandManager Commands { get; }
    }
}
