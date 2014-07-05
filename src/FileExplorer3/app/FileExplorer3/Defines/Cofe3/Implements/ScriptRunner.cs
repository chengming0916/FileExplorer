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
        public static ScriptRunner Instance = new ScriptRunner();

        public static Task RunScriptAsync(ParameterDic initialParameters, bool cloneParameters, params IScriptCommand[] commands)
        {
            if (cloneParameters)
                initialParameters = initialParameters.Clone();

            IScriptRunner runner = Instance;
            if (initialParameters.ContainsKey("ScriptRunner") && initialParameters["ScriptRunner"] is IScriptRunner)
                runner = (initialParameters["ScriptRunner"] as IScriptRunner);            
            return runner.RunAsync(new Queue<IScriptCommand>(commands), initialParameters);
        }

        public static void RunScript(ParameterDic initialParameters, bool cloneParameters, params IScriptCommand[] commands)
        {
            if (cloneParameters)
                initialParameters = initialParameters.Clone();

            IScriptRunner runner = Instance;
            if (initialParameters.ContainsKey("ScriptRunner") && initialParameters["ScriptRunner"] is IScriptRunner)
                runner = (initialParameters["ScriptRunner"] as IScriptRunner);            
            runner.Run(new Queue<IScriptCommand>(commands), initialParameters);
        }

        public static Task RunScriptAsync(ParameterDic initialParameters, params IScriptCommand[] commands)
        {
            return RunScriptAsync(initialParameters, false, commands);
        }

        public static void RunScript(ParameterDic initialParameters, params IScriptCommand[] commands)
        {
            RunScript(initialParameters, commands);
        }

        public static Task RunScriptAsync(params IScriptCommand[] commands)
        {
            return RunScriptAsync(new ParameterDic(), commands);
        }

        public static void RunScript(params IScriptCommand[] commands)
        {
            RunScript(new ParameterDic(), commands);
        }
        
        public ScriptRunner()
        {

        }


        public void Run(Queue<IScriptCommand> cmds, ParameterDic initialParameters)
        {
            ParameterDic pd = initialParameters;
            pd["ScriptRunner"] = this;

            while (cmds.Any())
            {
                try
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
                catch (Exception ex)
                {

                    throw;
                }
            }
        }

        public void Run(IScriptCommand cmd, ParameterDic initialParameters)
        {
            Run(new Queue<IScriptCommand>(new[] { cmd }), initialParameters);
        }

        public async Task RunAsync(Queue<IScriptCommand> cmds, ParameterDic initialParameters)
        {
            ParameterDic pd = initialParameters;
            pd["ScriptRunner"] = this;

            while (cmds.Any())
            {

                try
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
                catch (Exception ex)
                {
                    throw;
                }

            }
        }

    }
}
