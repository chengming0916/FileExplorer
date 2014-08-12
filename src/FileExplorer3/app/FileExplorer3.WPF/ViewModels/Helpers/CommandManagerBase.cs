using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer;
using System.Windows.Input;

namespace FileExplorer.WPF.ViewModels
{

    public abstract class CommandManagerBase : ICommandManager, IExportCommandBindings
    {
        #region Constructor

        protected CommandManagerBase(IExportCommandBindings[] additionalBindingExportSource)
        {
            _additionalBindingExportSource = additionalBindingExportSource;
        }

        #endregion

        #region Methods        

        public void InitCommandManager()
        {            
            _parameterDicConverter = setupParamDicConverter();
            _commandDictionary = setupCommandDictionary(ParameterDicConverter);
            _exportBindingSource = setupExportBindings().Concat(_additionalBindingExportSource).ToArray();
        }

        private IEnumerable<IScriptCommandBinding> getCommandBindings()
        {
            return _exportBindingSource.SelectMany(eb => eb.ExportedCommandBindings);
        }

        protected abstract IParameterDicConverter setupParamDicConverter();

        /// <summary>
        /// Create and initialize (get&setupScriptCommands()) a ScriptCommandDictionary and return.
        /// </summary>
        /// <returns></returns>
        private dynamic setupCommandDictionary(IParameterDicConverter parameterDicConverter)
        {
            DynamicRelayCommandDictionary dic = new DynamicRelayCommandDictionary()
            {
                ParameterDicConverter = parameterDicConverter
            };

            foreach (var cmd in getScriptCommands())
                dic[cmd] = NullScriptCommand.Instance;
            setupScriptCommands(dic);
            return dic;
        }

        protected abstract IEnumerable<string> getScriptCommands();
        protected virtual void setupScriptCommands(dynamic commandDictionary) { }

        /// <summary>
        /// Setup ScriptCommandBindings to be exported, obsoluting.
        /// </summary>
        /// <returns></returns>
        protected virtual IExportCommandBindings[] setupExportBindings() { return new IExportCommandBindings[] { }; }       

        #endregion

        #region Data        
        private IParameterDicConverter _parameterDicConverter = null;
        private DynamicRelayCommandDictionary _commandDictionary = null;
        protected IExportCommandBindings[] _exportBindingSource = new IExportCommandBindings[] { };
        private IExportCommandBindings[] _additionalBindingExportSource;        

        #endregion

        #region Public Properties

        public IParameterDicConverter ParameterDicConverter { get { return _parameterDicConverter; } }
        public dynamic CommandDictionary { get { return _commandDictionary; } }
        [Obsolete("Use CommandDictionary")]
        public dynamic Commands { get { return CommandDictionary; } }

        public IToolbarCommandsHelper ToolbarCommands { get; protected set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get { return getCommandBindings(); } }




        #endregion






    }

}
