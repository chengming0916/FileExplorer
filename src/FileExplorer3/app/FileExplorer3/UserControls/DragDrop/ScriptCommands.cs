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
    }
}
