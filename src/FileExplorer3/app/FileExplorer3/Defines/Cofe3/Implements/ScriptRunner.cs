using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{        
    public class ScriptRunner : IScriptRunner
    {
        public void Run(Queue<IScriptCommand> cmds, ParameterDic initialParameters) 
        {
            ParameterDic pd = initialParameters;

            while (cmds.Any())
            {
                var current = cmds.Dequeue();
                if (current.CanExecute(pd))
                {
                    var retCmd = current.Execute(pd);
                    if (retCmd != null)
                        cmds.Enqueue(retCmd);
                }
                else throw new Exception(String.Format("Cannot execute {0}", current));
            }
        }
    }
}
