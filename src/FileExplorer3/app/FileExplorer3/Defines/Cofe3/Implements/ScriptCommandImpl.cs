using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{
    public class ResultCommand : ScriptCommandBase
    {        
        /// <summary>
        /// Represent no error and IsHandled.
        /// </summary>
        public static ResultCommand OK = new ResultCommand(null, true);
        /// <summary>
        /// Represent no error and does not mark IsHandled.
        /// </summary>
        public static ResultCommand NoError = new ResultCommand(null, false);        

        /// <summary>
        /// Represent there's an error.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static ResultCommand Error(Exception ex) { return new ResultCommand(ex); }

        Action<ParameterDic> _executeFunc = (pm) => { };
        private ResultCommand(Exception ex = null, bool markHandled = true)
            : base( ex == null ? "OK" : "FAIL")
        {
            if (ex == null)
                _executeFunc = (pm) => { if (markHandled) pm.IsHandled = true; };
            else _executeFunc = (pm) => { pm.Error = ex; };
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            _executeFunc(pm);
            return null;
        }
        
    }

    /// <summary>
    /// A script command that cannot execute.
    /// </summary>
    public class NullScriptCommand : IScriptCommand
    {

        public static NullScriptCommand Instance = new NullScriptCommand();

        public string CommandKey { get { return "Null"; } }        
        public IScriptCommand Execute(ParameterDic pm)
        {
            return ResultCommand.Error(new Exception("NullScriptCommand should not be called."));
        }

        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            return ResultCommand.Error(new Exception("NullScriptCommand should not be called."));
        }

        public bool CanExecute(ParameterDic pm)
        {
            return false;
        }
    }


}
