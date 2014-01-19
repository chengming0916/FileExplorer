﻿using System;
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

        public void Run(IScriptCommand cmd, ParameterDic initialParameters)
        {
            Run(new Queue<IScriptCommand>(new[] { cmd }), initialParameters);
        }

        public async Task RunAsync(Queue<IScriptCommand> cmds, ParameterDic initialParameters)
        {
            ParameterDic pd = initialParameters;

            while (cmds.Any())
            {
                var current = cmds.Dequeue();
                if (current.CanExecute(pd))
                {
                    var retCmd = await current.ExecuteAsync(pd);
                    if (retCmd != null)
                        cmds.Enqueue(retCmd);
                }
                else throw new Exception(String.Format("Cannot execute {0}", current));
            }
        }
       
    }
}
