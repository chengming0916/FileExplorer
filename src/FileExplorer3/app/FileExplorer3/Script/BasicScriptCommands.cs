using MetroLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class ScriptCommands
    {
        public static NoScriptCommand NoCommand = new NoScriptCommand();


        public static IScriptCommand If(Func<ParameterDic, bool> condition, IScriptCommand ifTrue, IScriptCommand otherwise)
        {
            return new IfScriptCommand(condition, ifTrue, otherwise);
        }

        [Obsolete("Use RunCommands")]
        public static IScriptCommand RunInSequence(params IScriptCommand[] scriptCommands)
        {
            return new RunInSequenceScriptCommand(scriptCommands);
        }

        [Obsolete("Use RunCommands")]
        public static IScriptCommand RunInSequence(IScriptCommand[] scriptCommands, IScriptCommand nextCommand)
        {
            return new RunInSequenceScriptCommand(scriptCommands, nextCommand);
        }


        [Obsolete("Use ForEach(NonGeneric)")]
        public static IScriptCommand ForEach<T>(T[] source, Func<T, IScriptCommand> commandFunc, IScriptCommand nextCommand = null)
        {
            return new ForEachCommand<T>(source, commandFunc, nextCommand);
        }

        /// <summary>
        /// Serializable, print content of a variable to debug.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand PrintDebug(string variable, IScriptCommand nextCommand = null)
        {
            return new Print()
            {
                DestinationType = Print.PrintDestinationType.Debug,
                VariableKey = variable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, print content of a variable to logger.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand PrintLogger(string variable, IScriptCommand nextCommand = null)
        {
            return new Print()
            {
                DestinationType = Print.PrintDestinationType.Logger,
                VariableKey = variable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, given an array, iterate it with NextCommand, then run ThenCommand when all iteration is finished.
        /// </summary>
        /// <param name="ItemsVariable"></param>
        /// <param name="currentItemVariable"></param>
        /// <param name="doCommand"></param>
        /// <param name="thenCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ForEach(string ItemsVariable = "{Items}", string currentItemVariable = "{CurrentItem}",
            IScriptCommand doCommand = null, IScriptCommand thenCommand = null)
        {
            return new ForEach()
            {
                ItemsKey = ItemsVariable,
                CurrentItemKey = currentItemVariable,
                NextCommand = (ScriptCommandBase)doCommand,
                ThenCommand = (ScriptCommandBase)thenCommand
            };
        }

        /// <summary>
        /// Serializable, run commands in sequences or in parallel (if async).
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="thenCommand"></param>
        /// <param name="commands"></param>
        /// <returns></returns>
        public static IScriptCommand RunCommands(RunCommands.RunMode mode = Script.RunCommands.RunMode.Sequence,
            IScriptCommand thenCommand = null, params IScriptCommand[] commands)
        {
            return new RunCommands()
            {
                Mode = mode,
                NextCommand = (ScriptCommandBase)thenCommand,
                ScriptCommands = commands.Cast<ScriptCommandBase>().ToArray()
            };
        }
    }

    /// <summary>
    /// Serializable, print content of a variable to debug.
    /// </summary>
    public class Print : ScriptCommandBase
    {
        [Flags]
        public enum PrintDestinationType { Logger = 1 << 0, Debug = 1 << 1 }

        /// <summary>
        /// Variable to print.
        /// </summary>
        public string VariableKey { get; set; }

        /// <summary>
        /// Where to print to.
        /// </summary>
        public PrintDestinationType DestinationType { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<Print>();

        public Print()
            : base("Print")
        {
            DestinationType = PrintDestinationType.Logger;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            string variable = pm.ReplaceVariableInsideBracketed(VariableKey);

            if (DestinationType.HasFlag(PrintDestinationType.Debug))
                Debug.WriteLine(variable);
            if (DestinationType.HasFlag(PrintDestinationType.Logger))
                logger.Info(variable);

            return NextCommand;
        }

    }


    /// <summary>
    /// Serializable, given an array, iterate it with NextCommand, then run ThenCommand when all iteration is finished.
    /// </summary>
    public class ForEach : ScriptCommandBase
    {
        /// <summary>
        /// Array of item to be iterated, support IEnumerable or Array, Default=Items
        /// </summary>
        public string ItemsKey { get; set; }
        /// <summary>
        /// When iterating item (e.g. i in foreach (var i in array)), the current item will be stored in this key.
        /// Default = CurrentItem
        /// </summary>
        public string CurrentItemKey { get; set; }

        /// <summary>
        /// Iteration command is run in NextCommand, when all iteration complete ThenCommand is run.
        /// </summary>
        public ScriptCommandBase ThenCommand { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ForEach>();

        public ForEach()
            : base("ForEach")
        {
            CurrentItemKey = "{CurrentItem}";
            ItemsKey = "{Items}";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {            
            IEnumerable e = pm.GetValue<IEnumerable>(ItemsKey);
            if (e == null)
                return ResultCommand.Error(new ArgumentException(ItemsKey));

            uint counter = 0;
            foreach (var item in e)
            {
                counter++;
                pm.SetValue(CurrentItemKey, item);
                await ScriptRunner.RunScriptAsync(pm, NextCommand);
                if (pm.Error != null)
                {
                    pm.SetValue<Object>(CurrentItemKey, null);
                    return ResultCommand.Error(pm.Error);
                }
            }
            logger.Info("Looped {0} items", counter);
            pm.SetValue<Object>(CurrentItemKey, null);

            return ThenCommand;
        }
    }



    public class IfScriptCommand : IScriptCommand
    {
        private Func<ParameterDic, bool> _condition;
        private IScriptCommand _otherwiseCommand;
        private IScriptCommand _ifTrueCommand;
        private bool _continueOnCaptureContext = false;
        public IfScriptCommand(Func<ParameterDic, bool> condition,
            IScriptCommand ifTrueCommand, IScriptCommand otherwiseCommand)
        { _condition = condition; _ifTrueCommand = ifTrueCommand; _otherwiseCommand = otherwiseCommand; }

        public string CommandKey
        {
            get { return "IfScriptCommand"; }
        }

        public IScriptCommand Execute(ParameterDic pm)
        {
            if (_condition(pm))
                return _ifTrueCommand;
            return _otherwiseCommand;
        }

        public virtual bool CanExecute(ParameterDic pm)
        {
            if (_condition != null && _condition(pm))
                return _ifTrueCommand == null || _ifTrueCommand.CanExecute(pm);
            else return _otherwiseCommand == null || _otherwiseCommand.CanExecute(pm);
        }


        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            if (_condition(pm))
                return _ifTrueCommand;
            return _otherwiseCommand;
        }

        public bool ContinueOnCaptureContext
        {
            get { return _continueOnCaptureContext; }
            protected set { _continueOnCaptureContext = value; }
        }
    }

    public class ForEachCommand<T> : ScriptCommandBase
    {
        private T[] _source;
        private Func<T, IScriptCommand> _commandFunc;
        public ForEachCommand(T[] source, Func<T, IScriptCommand> commandFunc, IScriptCommand nextCommand)
            : base("ForEach", nextCommand)
        {
            _source = source;
            _commandFunc = commandFunc;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            List<IScriptCommand> outputCommands = new List<IScriptCommand>();
            foreach (var s in _source)
            {
                var command = _commandFunc(s);
                var outputCommand = command.Execute(pm);
                if (pm.Error != null)
                    return outputCommand;
                if (outputCommand != ResultCommand.NoError && outputCommand != ResultCommand.OK)
                    outputCommands.Add(outputCommand);
            }
            return new RunInSequenceScriptCommand(outputCommands.ToArray(), _nextCommand);
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            List<IScriptCommand> outputCommands = new List<IScriptCommand>();
            foreach (var s in _source)
            {
                var outputCommand = await _commandFunc(s).ExecuteAsync(pm);
                if (pm.Error != null)
                    return outputCommand;

                if (outputCommand != ResultCommand.NoError && outputCommand != ResultCommand.OK)
                    outputCommands.Add(outputCommand);
            }
            return outputCommands.Count() == 0 ?
                _nextCommand :
                new RunInSequenceScriptCommand(outputCommands.ToArray(), _nextCommand);
        }

        public virtual bool CanExecute(ParameterDic pm)
        {
            return _source.Count() > 0 && _commandFunc(_source.First()).CanExecute(pm);
        }
    }


    /// <summary>
    /// Serializable, run a number of ScriptCommand in a sequence.
    /// </summary>
    public class RunCommands : ScriptCommandBase
    {
        public enum RunMode { Sequence, Parallel }

        /// <summary>
        /// A list of ScriptCommands to run.
        /// </summary>
        public ScriptCommandBase[] ScriptCommands { get; set; }

        /// <summary>
        /// How to run commands, default = Sequence
        /// </summary>
        public RunMode Mode { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<RunCommands>();



        public RunCommands()
            : base("RunCommands")
        {
            Mode = RunMode.Sequence;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            ScriptRunner.RunScript(pm, ScriptCommands);
            if (pm.Error != null)
                return ResultCommand.Error(pm.Error);
            else return NextCommand;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            return ScriptCommands.First().CanExecute(pm);
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            switch (Mode)
            {
                case RunMode.Parallel:
                    await ScriptRunner.RunScriptAsync(pm, ScriptCommands);
                    break;
                case RunMode.Sequence:
                    await Task.WhenAll(ScriptCommands.Select(cmd => ScriptRunner.RunScriptAsync(pm.Clone(), cmd)));
                    break;
                default:
                    return ResultCommand.Error(new NotSupportedException(Mode.ToString()));
            }

            if (pm.Error != null)
                return ResultCommand.Error(pm.Error);
            else return NextCommand;
        }

        public new bool ContinueOnCaptureContext
        {
            get { return ScriptCommands.Any(c => c.ContinueOnCaptureContext); }
            set { }
        }
    }


    /// <summary>
    /// Run a number of ScriptCommand in a sequence.
    /// </summary>
    [Obsolete]
    public class RunInSequenceScriptCommand : IScriptCommand
    {
        private IScriptCommand[] _scriptCommands;
        private IScriptCommand _nextCommand = ResultCommand.NoError;
        public IScriptCommand[] ScriptCommands { get { return _scriptCommands; } }
        public RunInSequenceScriptCommand(params IScriptCommand[] scriptCommands)
        {
            if (scriptCommands.Length == 0) throw new ArgumentException(); _scriptCommands = scriptCommands;
        }

        public RunInSequenceScriptCommand(IScriptCommand[] scriptCommands, IScriptCommand nextCommand)
        {
            if (scriptCommands.Length == 0) throw new ArgumentException(); _scriptCommands = scriptCommands;
            _nextCommand = nextCommand;
        }

        public string CommandKey
        {
            get { return String.Join(",", _scriptCommands.Select(c => c.CommandKey)); }
        }

        public virtual IScriptCommand Execute(ParameterDic pm)
        {
            ScriptRunner.RunScript(pm, ScriptCommands);
            if (pm.Error != null)
                return ResultCommand.Error(pm.Error);
            else return _nextCommand;
        }

        public virtual bool CanExecute(ParameterDic pm)
        {
            return _scriptCommands.First().CanExecute(pm);
        }

        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            await ScriptRunner.RunScriptAsync(pm, ScriptCommands);
            if (pm.Error != null)
                return ResultCommand.Error(pm.Error);
            else return _nextCommand;
        }

        public bool ContinueOnCaptureContext
        {
            get { return _scriptCommands.Any(c => c.ContinueOnCaptureContext); }
        }
    }






    public class NoScriptCommand : IScriptCommand
    {

        public string CommandKey
        {
            get { return "None"; }
        }

        public IScriptCommand Execute(ParameterDic pm)
        {
            return null;
        }

        public bool CanExecute(ParameterDic pm)
        {
            return false;
        }

        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            return Execute(pm);
        }

        public bool ContinueOnCaptureContext { get { return false; } }

    }
}
