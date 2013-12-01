using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        #endregion

        #region Methods

        public abstract IScriptCommand Execute(ParameterDic pm);

        public virtual bool CanExecute(ParameterDic pm)
        {
            return true;
        }
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        public string CommandKey { get; private set; }
        public string[] CommandParameters { get; private set; }        

        #endregion




    }
}
