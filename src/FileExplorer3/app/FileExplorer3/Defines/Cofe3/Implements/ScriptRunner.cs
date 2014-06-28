using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public class ScriptRunner : IScriptRunner
    {
        public void Run(Queue<IScriptCommand> cmds, ParameterDic initialParameters)
        {
            ParameterDic pd = initialParameters;

            while (cmds.Any())
            {
                var current = cmds.Dequeue();
                //Debug.WriteLine("ScriptRunner:" + current.CommandKey);
                if (current.CanExecute(pd))
                {
                    pd.CommandHistory.Add(current.CommandKey);
                    var retCmd = current.Execute(pd);
                    if (retCmd != null)
                    {
                        if (pd.Error != null)
                            return;
                        cmds.Enqueue(retCmd);
                    }
                }
                else
                    if (!(current is NullScriptCommand))
                        throw new Exception(String.Format("Cannot execute {0}", current));
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
                    pd.CommandHistory.Add(current.CommandKey);
                    Debug.WriteLine("ScriptRunner:" + current.CommandKey);
                    var retCmd = await current.ExecuteAsync(pd)
                        .ConfigureAwait(current.ContinueOnCaptureContext);                    
                    
                    if (retCmd != null)
                    {
                        if (pd.Error != null)
                            return;
                        cmds.Enqueue(retCmd);
                    }
                }
                else throw new Exception(String.Format("Cannot execute {0}", current));



            }
        }

    }
}
