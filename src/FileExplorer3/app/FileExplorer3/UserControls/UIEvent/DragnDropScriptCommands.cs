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
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.BaseControls.DragnDrop
{
    public static class DataContextFinder
    {
        public static Func<ISupportDrag, bool> SupportDrag = dc => dc is ISupportDrag && dc.HasDraggables;
        public static Func<ISupportDrop, bool> SupportDrop = dc => dc is ISupportDrop && dc.IsDroppable;

        public static T GetDataContext<T>(ParameterDic pm, Func<T, bool> filter = null)
        {
            FrameworkElement ele;
            return GetDataContext(pm, out ele, filter);
        }


        public static FrameworkElement GetDataContextOwner(FrameworkElement ele)
        {
            var tvItem = UITools.FindAncestor<TreeViewItem>(ele);
            if (tvItem != null)
                return tvItem;

            var lvItem = UITools.FindAncestor<ListViewItem>(ele);
            if (lvItem != null)
                return lvItem;

            var ic = UITools.FindAncestor<ItemsControl>(ele);
            if (ic != null)
                return ic;

            return ele;
        }

        public static T GetDataContext<T>(ParameterDic pm, out FrameworkElement ele, Func<T, bool> filter = null)
        {
            var pd = pm.AsUIParameterDic();
            var eventArgs = pd.EventArgs as RoutedEventArgs;            
            var origSource = eventArgs.OriginalSource as FrameworkElement;
            ele = null;

            object dataContext = origSource.DataContext;
            if (dataContext is T && filter((T)dataContext))
            {
                ele = GetDataContextOwner(origSource);
                return (T)dataContext;
            }
            else
            {
                var ic = UITools.FindAncestor<ItemsControl>(origSource);
                while (ic != null)
                {
                    if (ic.DataContext is T && filter((T)ic.DataContext))
                    {
                        ele = GetDataContextOwner(ic);
                        return (T)ic.DataContext;
                    }
                    ic = UITools.FindAncestor<ItemsControl>(VisualTreeHelper.GetParent(ic));
                }

            }

                
            

            return default(T);

        }

        

    }

    /// <summary>
    /// Set StartSelectedItem when PreviewMouseDown.
    /// </summary>
    public class RecordStartSelectedItem : ScriptCommandBase
    {
        public RecordStartSelectedItem() : base("RecordStartSelectedItem", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);
            var eventArgs = pd.EventArgs as MouseEventArgs;
            var scp = ControlUtils.GetScrollContentPresenter(ic);

            if (isd != null)
            {
                UITools.SetItemUnderMouseToAttachedProperty(ic, eventArgs.GetPosition(scp),
                    AttachedProperties.StartDraggingItemProperty);

                return new IfItemUnderMouseSelected(new SetSelectedDraggables(), ResultCommand.NoError);
            }
            return ResultCommand.NoError;
        }
    }



    public class BeginDrag : ScriptCommandBase
    {
        public BeginDrag() : base("BeginDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);

            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;

            if (isd != null)
            {                
                var previousDraggables = AttachedProperties.GetSelectedDraggables(ic);
                var currentDraggables = isd.GetDraggables().ToList();


                if (currentDraggables.Any() && previousDraggables != null &&
                    currentDraggables.SequenceEqual(previousDraggables))
                {
                    //Set it handled so it wont call multi-select.
                    pd.EventArgs.Handled = true;
                    pd.IsHandled = true;
                    AttachedProperties.SetIsDragging(ic, true);
                    return new DoDragDrop(ic, isd);
                }
            }

            return ResultCommand.NoError; //Not supported
        }
    }

    public class ContinueDrag : ScriptCommandBase
    {
        public ContinueDrag() : base("ContinueDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            if (AttachedProperties.GetIsDragging(ic))
                pd.EventArgs.Handled = true;

            return ResultCommand.OK;
        }
    }

    public class EndDrag : ScriptCommandBase
    {
        public EndDrag() : base("EndDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            AttachedProperties.SetStartDraggingItem(ic, null);
            AttachedProperties.SetIsDragging(ic, false);

            return new DetachAdorner();
        }
    }


    public enum QueryDragDropEffectMode { Enter, Leave }
    public class QueryDragDropEffects : ScriptCommandBase
    {
        private QueryDragDropEffectMode _mode;
        public QueryDragDropEffects(QueryDragDropEffectMode mode) :
            base("QueryDragDropEffects", "EventArgs") { _mode = mode; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;            
            var eventArgs = pd.EventArgs as DragEventArgs;

            FrameworkElement ele;
            var isd = DataContextFinder.GetDataContext(pm, out ele, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                if (_mode == QueryDragDropEffectMode.Enter)
                {
                    AttachedProperties.SetDraggingOverItem(ic, ele);
                    eventArgs.Effects = (eventArgs.AllowedEffects & isd.QueryDrop(eventArgs.Data));                    
                    eventArgs.Handled = true;                    
                    return new AttachAdorner();
                }
                else
                {
                    AttachedProperties.SetDraggingOverItem(ic, null);
                    return new HideAdorner();
                }
            }
            return ResultCommand.NoError;
        }
    }

    public class UpdateAdorner : ScriptCommandBase
    {
        public UpdateAdorner() : base("UpdateAdorner", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;            
            var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = Window.GetWindow(ic);
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);

            if (isd != null)
            {                
                var dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);
                if (dragAdorner != null)
                {
                    dragAdorner.DraggingItems = isd.QueryDropDraggables(eventArgs.Data);
                    dragAdorner.PointerPosition = eventArgs.GetPosition(parentWindow);
                    dragAdorner.IsDragging = true;
                    var hintTemplate = AttachedProperties.GetDragItemTemplate(ic);                    
                    dragAdorner.DraggingItemTemplate = hintTemplate ?? ic.ItemTemplate;
                }
            }

            return ResultCommand.NoError;
        }
    }


    public class HideAdorner : ScriptCommandBase
    {
        public HideAdorner() : base("HideAdorner", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = Window.GetWindow(ic);

            var dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);
            if (dragAdorner != null)            
                dragAdorner.IsDragging = false;            

            return ResultCommand.NoError;
        }
    }


    public class AttachAdorner : ScriptCommandBase
    {
        public AttachAdorner() : base("AttachAdorner", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            Window parentWindow = Window.GetWindow(ic);

            if (parentWindow != null)
            {
                pm["DragDropAdorner"] = AttachedProperties.GetDragAdorner(parentWindow);
                if (pm["DragDropAdorner"] != null)
                    return new UpdateAdorner();

                AdornerDecorator decorator = UITools.FindVisualChildByName<AdornerDecorator>
                    (parentWindow, "PART_DragDropAdorner");

                if (decorator == null)
                    return ResultCommand.Error(new KeyNotFoundException("PART_DragDropAdorner"));
                else
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(decorator);
                    DragAdorner adorner = new DragAdorner(adornerLayer);
                    pm["DragDropAdorner"] = adorner;
                    adornerLayer.Add(adorner);
                    AttachedProperties.SetDragAdorner(parentWindow, adorner);
                    return new UpdateAdorner();
                }
            }

            return ResultCommand.Error(new Exception("Control do not have parent window."));
        }
    }

    public class DetachAdorner : ScriptCommandBase
    {
        public DetachAdorner() : base("DetachAdorner", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            Window parentWindow = Window.GetWindow(ic);

            if (parentWindow != null)
            {
                var dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);

                if (dragAdorner != null)
                {
                    AdornerDecorator decorator = UITools.FindVisualChildByName<AdornerDecorator>
                        (parentWindow, "PART_DragDropAdorner");

                    if (decorator == null)
                        return ResultCommand.Error(new KeyNotFoundException("PART_DragDropAdorner"));
                    else
                    {
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(decorator);
                        adornerLayer.Remove(dragAdorner);
                        AttachedProperties.SetDragAdorner(parentWindow, null);
                        return ResultCommand.NoError;
                    }
                }
            }

            return ResultCommand.Error(new Exception("Control do not have parent window."));
        }
    }

    public class BeginDrop : ScriptCommandBase
    {
        public BeginDrop() : base("BeginDrop", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            AttachedProperties.SetDraggingOverItem(ic, null);
            var eventArgs = pd.EventArgs as DragEventArgs;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                eventArgs.Effects = eventArgs.AllowedEffects & isd.QueryDrop(eventArgs.Data);
                if (eventArgs.Effects != DragDropEffects.None)
                {
                    isd.Drop(eventArgs.Data, eventArgs.AllowedEffects);
                    eventArgs.Handled = true;
                }                
            }

            return ResultCommand.NoError;
        }
    }

    #region Update/CheckIsDragging

    //public class UpdateIsDragging : ScriptCommandBase
    //{
    //    private bool _toValue;
    //    public UpdateIsDragging(bool toValue, IScriptCommand nextCommand = null) : base("UpdateIsDragging", nextCommand) { _toValue = toValue; }

    //    public override IScriptCommand Execute(ParameterDic pm)
    //    {
    //        var pd = pm.AsUIParameterDic();
    //        var c = pd.Sender as Control;
    //        if (AttachedProperties.GetIsDragging(c) != _toValue)
    //        {
    //            AttachedProperties.SetIsDragging(c, _toValue);
    //            if (_nextCommand != null)
    //                return _nextCommand;
    //        }
    //        return ResultCommand.NoError;
    //    }
    //}

    public class IfIsDragging : IfScriptCommand
    {
        public IfIsDragging(IScriptCommand ifTrueCommand, IScriptCommand otherwiseCommand)
            : base((pm) =>
                {
                    var pd = pm.AsUIParameterDic();
                    var c = pd.Sender as Control;
                    return AttachedProperties.GetIsDragging(c);
                }, ifTrueCommand, otherwiseCommand) { }

    }
    #endregion

    public class IfItemUnderMouseSelected : IfScriptCommand
    {
        private static bool conditionFunc(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);

            if (isd != null)
            {
                var startSelectedItem = AttachedProperties.GetStartDraggingItem(ic);
                if (startSelectedItem != null)
                {
                    if (isd.GetDraggables().Contains(
                        startSelectedItem.DataContext))
                        return true;
                }
            }
            return false;
        }

        public IfItemUnderMouseSelected(IScriptCommand ifTrueCommand, IScriptCommand otherwiseCommand) :
            base(conditionFunc, ifTrueCommand, otherwiseCommand)
        { }
    }

    public class SetSelectedDraggables : ScriptCommandBase
    {
        #region Constructor

        public SetSelectedDraggables() :
            base("SetPreviousSelectedDraggables", "EventArgs")
        { }

        #endregion

        #region Methods

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);
            if (isd != null)
            {

                AttachedProperties.SetSelectedDraggables(ic,
                    isd.GetDraggables().ToList());
                pd.EventArgs.Handled = true;
            }

            return ResultCommand.NoError;
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion


    }


    public class DoDragDrop : ScriptCommandBase
    {
        private IDataObject _dataObj;
        private ItemsControl _ic;
        private ISupportDrag _isd;


        public DoDragDrop(ItemsControl ic, ISupportDrag isd) : base("DoDragDrop") { _ic = ic; _isd = isd; }

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            //The control, like treeview or listview
            FrameworkElement control = sender as FrameworkElement;

            //ESC pressed
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                control.AllowDrop = true;

                //control.QueryContinueDrag -= new QueryContinueDragEventHandler(OnQueryContinueDrag);
                //HideAdorner(control);
            }
            else
                //Drop!
                if (e.KeyStates == DragDropKeyStates.None)
                {
                    _dataObj.SetData(ShellClipboardFormats.CFSTR_INDRAGLOOP, 0);
                    e.Action = DragAction.Drop;
                    control.AllowDrop = true;

                    //control.QueryContinueDrag -= new QueryContinueDragEventHandler(OnQueryContinueDrag);
                    //HideAdorner(control);
                }
                else
                    e.Action = DragAction.Continue;

            e.Handled = true;
            //Debug.WriteLine(e.Action);
            //base.OnQueryContinueDrag(e);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            
            var dataObjectTup = _isd.GetDataObject();
            _dataObj = dataObjectTup.Item1;

            System.Windows.DragDrop.AddQueryContinueDragHandler(_ic,
                  new QueryContinueDragEventHandler(OnQueryContinueDrag));

            DragDropEffects resultEffect = System.Windows.DragDrop.DoDragDrop(_ic,
                 _dataObj, dataObjectTup.Item2);

            System.Windows.DragDrop.RemoveQueryContinueDragHandler(_ic,
                  new QueryContinueDragEventHandler(OnQueryContinueDrag));

            if (resultEffect != DragDropEffects.None)
                _isd.OnDataObjectDropped(_dataObj, resultEffect);

            _dataObj = null;

            //pd.EventArgs.Handled = true;

            return ResultCommand.NoError;
        }
    }


    //public class SetHandledIfDragging : ScriptCommandBase
    //{
    //    public SetHandledIfDragging() : base("SetHandledIfDragging", "EventArgs") { }

    //    public override IScriptCommand Execute(ParameterDic pm)
    //    {
    //        var pd = pm.AsUIParameterDic();
    //        var ic = pd.Sender as ItemsControl;
    //        Control c = pd["StartSelectedItem"] as Control;
    //        if (c != null && c.DataContext is IDraggable && (c.DataContext as IDraggable).IsSelected)
    //            return new SetEventIsHandled(ResultCommand.NoError);
    //        else return ResultCommand.NoError;
    //    }
    //}


    public class PrepareDataObject : ScriptCommandBase
    {
        public PrepareDataObject() : base("PrepareDataObject") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = ic.DataContext as ISupportDrag;

            pm["DataObject"] = isd.GetDataObject();

            return ResultCommand.NoError;
        }

    }

}
