using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer;
using FileExplorer.Script;
using FileExplorer.WPF.BaseControls;
using System.Windows.Input;

namespace FileExplorer.Script
{
    public static partial class ScriptCommands
    {


        public static DebugScriptCommand PrintSourceDC = new DebugScriptCommand(DebugScriptCommand.HandleType.printOrgSource);
        public static DebugScriptCommand PrintSelectedDC = new DebugScriptCommand(DebugScriptCommand.HandleType.printSelector);
        public static DebugScriptCommand PrepareDrag = new DebugScriptCommand(DebugScriptCommand.HandleType.prepareDataObject);
        public static NoScriptCommand NoCommand = new NoScriptCommand();

        public static IScriptCommand If(Func<ParameterDic, bool> condition, IScriptCommand ifTrue, IScriptCommand otherwise)
        {
            return new IfScriptCommand(condition, ifTrue, otherwise);
        }

        public static IScriptCommand IfKeyPressed(Key key, IScriptCommand ifTrue, IScriptCommand otherwise = null)
        {
            Func<ParameterDic, bool> condition = pm =>
            {
                var pd = pm.AsUIParameterDic();
                switch (pd.EventArgs.RoutedEvent.Name)
                {
                    case "KeyDown":
                    case "PreviewKeyDown":
                        return (pd.EventArgs as KeyEventArgs).Key == key;
                }
                return false;
            };
            return If(condition, ifTrue, otherwise);
        }


        public static IScriptCommand RunInSequence(params IScriptCommand[] scriptCommands)
        {
            return new RunInSequenceScriptCommand(scriptCommands);
        }


        public static IScriptCommand RunInSequence(IScriptCommand[] scriptCommands, IScriptCommand nextCommand)
        {
            return new RunInSequenceScriptCommand(scriptCommands, nextCommand);
        }


        public static IScriptCommand ForEach<T>(T[] source, Func<T, IScriptCommand> commandFunc, IScriptCommand nextCommand = null)
        {
            return new ForEachCommand<T>(source, commandFunc, nextCommand);
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
            var sr = new ScriptRunner();
            sr.Run(new Queue<IScriptCommand>(ScriptCommands), pm);
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
            var sr = new ScriptRunner();
            await sr.RunAsync(new Queue<IScriptCommand>(ScriptCommands), pm).ConfigureAwait(true);
            if (pm.Error != null)
                return ResultCommand.Error(pm.Error);
            else return _nextCommand;
        }

        public bool ContinueOnCaptureContext
        {
            get { return _scriptCommands.Any(c => c.ContinueOnCaptureContext); }
        }
    }

    public class DebugScriptCommand : IScriptCommand
    {
        public enum HandleType { printOrgSource, printSelector, prepareDataObject }


        public DebugScriptCommand(HandleType handleType)
        {
            _handleType = handleType;
        }
        private HandleType _handleType;

        public string CommandKey
        {
            get { throw new NotImplementedException(); }
        }

        private static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }

        public IScriptCommand Execute(ParameterDic pm)
        {
            UIParameterDic parameterDic = pm.AsUIParameterDic();
            FrameworkElement control = parameterDic.Sender as FrameworkElement;

            object value = parameterDic.EventArgs.OriginalSource is FrameworkElement ?
                (parameterDic.EventArgs.OriginalSource as FrameworkElement).DataContext : null;
            switch (_handleType)
            {
                case HandleType.prepareDataObject:
                case HandleType.printSelector:
                    if (control is System.Windows.Controls.Primitives.Selector)
                        value = (control as System.Windows.Controls.Primitives.Selector).SelectedValue;
                    break;
            }
            if (value == null) value = "";
            Debug.WriteLine(String.Format("{0} - {1} = {2}", control.Name, parameterDic.EventName, value));

            if (control is System.Windows.Controls.Primitives.Selector && _handleType == HandleType.prepareDataObject)
            {
                DataObject obj = new DataObject(DataFormats.Text, value);
                //Debug.WriteLine(String.Format("{0} - {1} = {2}", control.Name, GetPropertyName(eventExpression), value));
                System.Windows.DragDrop.DoDragDrop(control, obj, DragDropEffects.All);
                parameterDic.EventArgs.Handled = true;
            }
            else
            {
                if (value == null) value = "";

            }

            return null;
        }

        public bool CanExecute(ParameterDic pm)
        {
            return true;
        }
        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            return Execute(pm);
        }
        public bool ContinueOnCaptureContext { get { return false; } }

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
