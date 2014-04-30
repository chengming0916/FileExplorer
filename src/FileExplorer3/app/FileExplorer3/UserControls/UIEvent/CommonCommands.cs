using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

namespace FileExplorer.BaseControls
{


    public static partial class UITools
    {
        public static Control GetItemUnderMouse(ItemsControl ic, Point position)
        {
            if (ic is ListView)
            {
                var scp = ControlUtils.GetScrollContentPresenter(ic);
                if (scp != null)
                    return UITools.GetSelectedListBoxItem(scp, position);
                return null;
            }
            else if (ic is TreeView)
                return UITools.GetItemByPosition<TreeViewItem, TreeView>(ic as TreeView, position);
            else if (ic is TabControl)
            {
                var panel = ic.Template.FindName("HeaderPanel", ic) as Panel;
                foreach (TabItem item in panel.Children)
                    if (item.IsMouseOver)
                        return item;
            }

            else if (ic is ItemsControl)
                return UITools.GetItemByPosition<Control, ItemsControl>(ic as ItemsControl, position);

            return null;

            //else if (ic is TreeView)
            //{
            //    return UITools.GetTreeViewItem(ic as TreeView, position);
            //}
            return UITools.GetItemByPosition<Selector, Control>(ic, position);
        }

        public static void SetItemUnderMouseToAttachedProperty(ItemsControl ic, Point position, DependencyProperty property)
        {
            ic.SetValue(property, GetItemUnderMouse(ic, position));
        }
    }

    public class MarkEventHandled : ScriptCommandBase
    {
        public MarkEventHandled() : base("MarkEventHandled") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            pd.EventArgs.Handled = true;

            return ResultCommand.NoError;
        }
    }

    public class GetDataContext : ScriptCommandBase
    {
        Func<object, bool> _filter;
        IScriptCommand _notFoundCommand;
        public GetDataContext(Func<object, bool> filter = null,
            IScriptCommand nextCommand = null, IScriptCommand notFoundCommand = null)
            : base("GetDataContext", nextCommand, "EventArgs")
        { _filter = filter; _notFoundCommand = notFoundCommand; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;

            object dataContext = (eventArgs.OriginalSource as FrameworkElement).DataContext;
            if (_filter(dataContext))
                pd["DataContext"] = dataContext;
            else
                if (_filter(ic.DataContext))
                    pd["DataContext"] = ic.DataContext;
                else return _notFoundCommand ?? ResultCommand.Error(new Exception("No matched datacontext."));

            return _nextCommand ?? ResultCommand.NoError;

        }
    }

    public class SetEventIsHandled : ScriptCommandBase
    {
        public SetEventIsHandled(bool toVal = true, IScriptCommand nextCommand = null) :
            base("SetIsHandled")
        { _nextCommand = nextCommand; _toVal = toVal; }

        private IScriptCommand _nextCommand;
        private bool _toVal;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            pd.EventArgs.Handled = _toVal;
            return _nextCommand == null ? ResultCommand.NoError : _nextCommand;
        }
    }

    public class SetIsHandled : ScriptCommandBase
    {
        public SetIsHandled(IScriptCommand nextCommand = null, bool toVal = true) :
            base("SetIsHandled")
        { _nextCommand = nextCommand; _toVal = toVal; }

        private IScriptCommand _nextCommand;
        private bool _toVal;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            pm.IsHandled = _toVal;
            return _nextCommand == null ? ResultCommand.NoError : _nextCommand;
        }
    }

    public class CapturePointer : ScriptCommandBase
    {
        public static IScriptCommand Capture(IInputElement ele, CaptureMode mode = CaptureMode.Element, IScriptCommand nextCommand = null)
        {
            return new CapturePointer(ele, mode, nextCommand);
        }
        public static IScriptCommand Release(IScriptCommand nextCommand)
        { return new CapturePointer(null, CaptureMode.None, nextCommand); }

        private IInputElement _ele;
        private CaptureMode _mode;
        public CapturePointer(IInputElement ele, CaptureMode mode = CaptureMode.Element, IScriptCommand nextCommand = null)
            : base("CapturePointer", nextCommand)
        {
            _ele = ele;
            _mode = mode;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            switch (pd.Input.InputType)
            {
                case UIInputType.Touch:
                    (pd.Input.EventArgs as TouchEventArgs).TouchDevice.Capture(_ele, _mode);
                    break;
                case UIInputType.Mouse:
                case UIInputType.MouseLeft:
                case UIInputType.MouseRight:
                case UIInputType.Stylus:
                    Mouse.Capture(_ele, _mode);
                    break;
                default:
                    return ResultCommand.Error(new NotSupportedException());
            }
            return _nextCommand;
        }
    }

}
