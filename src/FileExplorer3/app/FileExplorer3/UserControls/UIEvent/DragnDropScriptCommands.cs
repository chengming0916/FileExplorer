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

namespace FileExplorer.UserControls.DragnDrop
{
    public static class DragnDropScriptCommands
    {
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
            if (ic.DataContext is ISupportDrag)
            {
                return new SetItemUnderMouse(ic, AttachedProperties.StartDraggingItemProperty,
                    new IfItemUnderMouseSelected(new SetSelectedDraggables(), ResultCommand.NoError));
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

            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;

            if (ic.DataContext is ISupportDrag)
            {
                var isd = ic.DataContext as ISupportDrag;
                var previousDraggables = AttachedProperties.GetSelectedDraggables(ic);
                var currentDraggables = isd.GetDraggables().ToList();


                if (currentDraggables.Any() && previousDraggables != null &&
                    currentDraggables.SequenceEqual(previousDraggables))
                {
                    //Set it handled so it wont call multi-select.
                    pd.EventArgs.Handled = true;
                    pd.IsHandled = true;
                    return new UpdateIsDragging(true, new DoDragDrop(ic, isd));
                }
            }

            return ResultCommand.NoError; //Not supported
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

            return new UpdateIsDragging(false, new DetachAdorner());
        }
    }
    
    public class GetDataContext : ScriptCommandBase
    {
        Func<object, bool> _filter;
        public GetDataContext(Func<object, bool> filter = null, 
            IScriptCommand nextCommand = null) : base("GetDataContext", nextCommand, "EventArgs")             
        { _filter = filter; }

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
                else return ResultCommand.Error(new Exception("No matched datacontext."));

            if (_nextCommand != null)
                return _nextCommand;
            else return ResultCommand.NoError;

        }
    }

    public class QueryDragDropEffects : ScriptCommandBase
    {
        public QueryDragDropEffects() : base("QueryDragDropEffects", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;            
            var eventArgs = pd.EventArgs as DragEventArgs;

            if (ic.DataContext is ISupportDrop)
            {
                var isd = ic.DataContext as ISupportDrop;
                eventArgs.Effects = (eventArgs.AllowedEffects & isd.QueryDrop(eventArgs.Data));
                eventArgs.Handled = true;
                return new AttachAdorner();
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

            if (ic.DataContext is ISupportDrop)
            {
                var isd = ic.DataContext as ISupportDrop;
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
            
            var eventArgs = pd.EventArgs as DragEventArgs;
            if (ic.DataContext is ISupportDrop)
            {
                var isd = ic.DataContext as ISupportDrop;
                isd.Drop(eventArgs.Data, eventArgs.AllowedEffects);
                eventArgs.Handled = true;
            }

            return ResultCommand.NoError;
        }
    }

    #region Update/CheckIsDragging

    public class UpdateIsDragging : ScriptCommandBase
    {
        private bool _toValue;
        public UpdateIsDragging(bool toValue, IScriptCommand nextCommand = null) : base("UpdateIsDragging", nextCommand) { _toValue = toValue; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            if (AttachedProperties.GetIsDragging(c) != _toValue)
            {
                AttachedProperties.SetIsDragging(c, _toValue);
                if (_nextCommand != null)
                    return _nextCommand;
            }
            return ResultCommand.NoError;
        }
    }

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
            if (ic.DataContext is ISupportDrag)
            {
                var startSelectedItem = AttachedProperties.GetStartDraggingItem(ic);
                if (startSelectedItem != null)
                {
                    if ((ic.DataContext as ISupportDrag).GetDraggables().Contains(
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
            if (ic.DataContext is ISupportDrag)
            {

                AttachedProperties.SetSelectedDraggables(ic,
                    (ic.DataContext as ISupportDrag).GetDraggables().ToList());
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
