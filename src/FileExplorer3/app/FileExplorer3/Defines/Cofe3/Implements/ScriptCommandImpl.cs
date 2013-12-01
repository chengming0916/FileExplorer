using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{
    public class ResultCommand : ScriptCommandBase
    {
        public static ResultCommand OK = new ResultCommand();
        public static ResultCommand Error(Exception ex) { return new ResultCommand(ex); }

        Action<ParameterDic> _executeFunc = (pm) => { };
        private ResultCommand(Exception ex = null)
            : base( ex == null ? "OK" : "FAIL")
        {
            if (ex == null)
                _executeFunc = (pm) => { pm.IsHandled = true; };
            else _executeFunc = (pm) => { pm.Error = ex; };
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            _executeFunc(pm);
            return null;
        }
        
    }

   


}
