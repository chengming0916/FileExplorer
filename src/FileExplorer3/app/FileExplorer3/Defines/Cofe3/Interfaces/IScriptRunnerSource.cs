using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{
    public interface IScriptRunnerSource
    {
        ScriptRunner GetScriptRunner();
    }
}
