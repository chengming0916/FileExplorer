using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{

    public interface IScriptCommand
    {
        string CommandKey { get; }

        IScriptCommand Execute(ParameterDic pm);
        Task<IScriptCommand> ExecuteAsync(ParameterDic pm);

        bool CanExecute(ParameterDic pm);
    }

   
}
