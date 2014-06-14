using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileExplorer.Utils;

namespace FileExplorer.Script
{
    public abstract class ScriptCommandBase : IScriptCommand
    {
        #region Constructor

        protected ScriptCommandBase(string commandKey, params string[] parameters)
        {
            CommandKey = commandKey;
            CommandParameters = parameters;
            _nextCommand = ResultCommand.NoError;
        }

        protected ScriptCommandBase(string commandKey, IScriptCommand nextCommand, params string[] parameters)
        {
            CommandKey = commandKey;
            CommandParameters = parameters;

            _nextCommand = nextCommand ?? ResultCommand.NoError;
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
        private bool _continueOnCaptureContext = false;

        #endregion

        #region Public Properties

        public bool ContinueOnCaptureContext
        {
            get { return _continueOnCaptureContext; }
            protected set { _continueOnCaptureContext = value; }
        }
        public string CommandKey { get; private set; }
        public string[] CommandParameters { get; private set; }

        #endregion








    }


}
