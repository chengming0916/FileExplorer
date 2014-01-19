using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core;
using Cofe.Core.Script;

namespace FileExplorer.BaseControls
{
    public static class ScriptCommands
    {
        public static DebugScriptCommand PrintSourceDC = new DebugScriptCommand(DebugScriptCommand.HandleType.printOrgSource);
        public static DebugScriptCommand PrintSelectedDC = new DebugScriptCommand(DebugScriptCommand.HandleType.printSelector);
        public static DebugScriptCommand PrepareDrag = new DebugScriptCommand(DebugScriptCommand.HandleType.prepareDataObject);
        public static NoScriptCommand NoCommand = new NoScriptCommand();
    }


    public class IfScriptCommand : IScriptCommand
    {
        private Func<ParameterDic, bool> _condition;
        private IScriptCommand _otherwiseCommand;
        private IScriptCommand _ifTrueCommand;
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

        public bool CanExecute(ParameterDic pm)
        {
            return true;
        }


        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            return Execute(pm);
        }
    }    

    public class RunInSequenceScriptCommand : IScriptCommand
    {
        private IScriptCommand[] _scriptCommands;
        public IScriptCommand[] ScriptCommands { get { return _scriptCommands; } }
        public RunInSequenceScriptCommand(params IScriptCommand[] scriptCommands)
        { if (scriptCommands.Length == 0) throw new ArgumentException(); _scriptCommands = scriptCommands; }

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
            else return ResultCommand.NoError;
        }

        public virtual bool CanExecute(ParameterDic pm)
        {
            return _scriptCommands.First().CanExecute(pm);
        }

        public async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            return Execute(pm);
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
    }
}
