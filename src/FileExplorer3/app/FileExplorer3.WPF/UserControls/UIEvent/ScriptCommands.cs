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
    public static partial class WPFScriptCommands
    {


        public static DebugScriptCommand PrintSourceDC = new DebugScriptCommand(DebugScriptCommand.HandleType.printOrgSource);
        public static DebugScriptCommand PrintSelectedDC = new DebugScriptCommand(DebugScriptCommand.HandleType.printSelector);
        public static DebugScriptCommand PrepareDrag = new DebugScriptCommand(DebugScriptCommand.HandleType.prepareDataObject);

        public static IScriptCommand IfKeyPressed(Key key, IScriptCommand ifTrue, IScriptCommand otherwise = null)
        {
            Func<IParameterDic, bool> condition = pm =>
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
            return ScriptCommands.If(condition, ifTrue, otherwise);
        }
       
    }

    [Obsolete("Use ScriptCommands.PrintDebug")]
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

        public IScriptCommand Execute(IParameterDic pm)
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

        public bool CanExecute(IParameterDic pm)
        {
            return true;
        }
        public async Task<IScriptCommand> ExecuteAsync(IParameterDic pm)
        {
            return Execute(pm);
        }
        public bool ContinueOnCaptureContext { get { return false; } }


        public ScriptCommandBase NextCommand
        {
            get;
            set;
        }
    }


    
}
