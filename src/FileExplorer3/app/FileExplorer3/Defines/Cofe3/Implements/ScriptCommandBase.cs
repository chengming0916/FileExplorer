using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cofe.Core.Utils;

namespace Cofe.Core.Script
{

    public abstract class ScriptCommandBase : IScriptCommand
    {


        #region Constructor

        protected ScriptCommandBase(string commandKey, params string[] parameters)
        {
            CommandKey = commandKey;
            CommandParameters = parameters;
        }

        protected ScriptCommandBase(string commandKey, IScriptCommand nextCommand, params string[] parameters)
        {
            CommandKey = commandKey;
            CommandParameters = parameters;

            _nextCommand = nextCommand;
        }

        #endregion

        #region Methods

        public virtual IScriptCommand Execute(ParameterDic pm)
        {
            return AsyncUtils.RunSync(() => ExecuteAsync(pm));            
        }

        public virtual async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            return Execute(pm);
        }

        public virtual bool CanExecute(ParameterDic pm)
        {
            return true;
        }

        #endregion

        #region Data

        protected IScriptCommand _nextCommand;

        #endregion

        #region Public Properties

        public string CommandKey { get; private set; }
        public string[] CommandParameters { get; private set; }

        #endregion






      
    }

    
}
