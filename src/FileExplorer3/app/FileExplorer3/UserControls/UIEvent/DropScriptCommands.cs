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
using FileExplorer.UserControls.InputProcesor;

namespace FileExplorer.BaseControls.DragnDrop
{


    public enum QueryDragDropEffectMode { Enter, Leave }
    public class QueryDragDropEffects : ScriptCommandBase
    {
        private QueryDragDropEffectMode _mode;
        public QueryDragDropEffects(QueryDragDropEffectMode mode) :
            base("QueryDragDropEffects", "EventArgs") { _mode = mode; }

        public override IScriptCommand Execute(ParameterDic pm)
        {

            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as UIElement;
            var eventArgs = pd.EventArgs as DragEventArgs;
            FrameworkElement ele;
            var isd = DataContextFinder.GetDataContext(pm, out ele, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                bool isDraggingOver = isd.IsDraggingOver = _mode == QueryDragDropEffectMode.Enter;
                AttachedProperties.SetIsDraggingOver(ele, isDraggingOver);

                if (_mode == QueryDragDropEffectMode.Enter)
                {
                    AttachedProperties.SetDraggingOverItem(ic, ele);
                    eventArgs.Handled = true;
                    return new AttachAdorner(new UpdateAdorner(new UpdateAdornerText()));

                }
                else
                {
                    AttachedProperties.SetDraggingOverItem(Window.GetWindow(ic), null);
                    return new HideAdorner();
                }


            }
            else
            {
                eventArgs.Effects = DragDropEffects.None;
            }
            return ResultCommand.NoError;
        }
    }


    public class ContinueDrop : ScriptCommandBase
    {
        public ContinueDrop()
            : base("ContinueDrop", "EventArgs")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            //var pd = pm.AsUIParameterDic();
            //var ic = pd.Sender as ItemsControl;
            //var eventArgs = pd.EventArgs as DragEventArgs;
            //var parentWindow = Window.GetWindow(ic);


            return new UpdateAdorner(null);
        }
    }

    public class ShowAdornerContextMenu : ScriptCommandBase
    {
        private IDataObject _dataObject;
        private ISupportDrop _isDrop;
        private ISupportDrag _isDrag;
        private DragDropEffects _supportedEffects, _defaultEffect;
        private DragAdorner _dragAdorner;

        public ShowAdornerContextMenu(DragDropEffects supportedEffects, DragDropEffects defaultEffect,
            ISupportDrag isDrag, ISupportDrop isDrop, IDataObject dataObject)
            : base("ShowAdornerContextMenu", "EventArgs")
        {
            _supportedEffects = supportedEffects; _defaultEffect = defaultEffect;
            _isDrag = isDrag; _isDrop = isDrop; _dataObject = dataObject;
        }


        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as UIElement;
            var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = Window.GetWindow(c);

            _dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);
            if (_dragAdorner != null)
            {
                RoutedEventHandler ContextMenu_Closed = null;
                ContextMenu_Closed = (o, e) =>
                    {
                        _dragAdorner.ContextMenu.RemoveHandler(ContextMenu.ClosedEvent, (RoutedEventHandler)ContextMenu_Closed);

                        new ScriptRunner().Run(new Queue<IScriptCommand>(
                        new IScriptCommand[] {
                            new BeginDrop(_dragAdorner.DragDropEffect),
                            new NotifyDropCompleted(_isDrag, _isDrop.QueryDropDraggables(_dataObject), _dataObject, _dragAdorner.DragDropEffect)
                        }), pm);
                    };

                _dragAdorner.SetSupportedDragDropEffects(_supportedEffects, _defaultEffect);
                _dragAdorner.ContextMenu.AddHandler(ContextMenu.ClosedEvent, (RoutedEventHandler)ContextMenu_Closed);
                _dragAdorner.ContextMenu.IsOpen = true;
            }
            return ResultCommand.OK;
        }
    }

    public class BeginDrop : ScriptCommandBase
    {
        public BeginDrop()
            : base("BeginDrop", "EventArgs")
        {
        }


        public BeginDrop(DragDropEffects overrideDragDropEffect)
            : this()
        {
            _overrideDragDropEffect = overrideDragDropEffect;
        }

        DragDropEffects? _overrideDragDropEffect;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as UIElement;
            var inp = pd.Input as IUIDragInput;

            AttachedProperties.SetDraggingOverItem(c, null);
            //var eventArgs = pd.EventArgs as DragEventArgs;
            var dragMethod =
                    inp.Data.GetDataPresent(typeof(AttachedProperties.DragMethod)) ?
                    (AttachedProperties.DragMethod)inp.Data.GetData(typeof(AttachedProperties.DragMethod))
                    : AttachedProperties.DragMethod.Normal;

            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                QueryDropResult queryDropResult = isd.QueryDrop(inp.Data, inp.AllowedEffects);
                DragDropEffects supportedEffects = inp.AllowedEffects & queryDropResult.SupportedEffects;
                if (supportedEffects != DragDropEffects.None && !_overrideDragDropEffect.HasValue &&
                    dragMethod == AttachedProperties.DragMethod.Menu && inp.Data.GetDataPresent(typeof(ISupportDrag)))
                {
                    inp.Effects = DragDropEffects.None;
                    inp.EventArgs.Handled = true;
                    ISupportDrag isDrag = inp.Data.GetData(typeof(ISupportDrag)) as ISupportDrag;
                    return new ShowAdornerContextMenu(supportedEffects, queryDropResult.PreferredEffect, isDrag, isd, inp.Data);
                }

                //If OverrideDragDropEffect is set, use it instead.
                supportedEffects = _overrideDragDropEffect.HasValue ? _overrideDragDropEffect.Value : supportedEffects;

                if (supportedEffects != DragDropEffects.None)
                {
                    IEnumerable<IDraggable> draggables = isd.QueryDropDraggables(inp.Data);
                    inp.Effects = isd.Drop(draggables, inp.Data, supportedEffects);
                    inp.EventArgs.Handled = true;
                }

                return new DetachAdorner();
            }

            return ResultCommand.NoError;
        }
    }



    public class UpdateAdorner : ScriptCommandBase
    {
        public UpdateAdorner(IScriptCommand nextCommand)
            : base("UpdateAdorner", nextCommand, "EventArgs")
        { }

        private static IDataObject _previousDataObject = null;
        private static int _draggingItemsCount = 0;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as UIElement;
            var inp = pd.Input as IUIDragInput;
            //var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = Window.GetWindow(ic);
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);

            if (isd != null)
            {
                var dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);
                pd["DraggingItemsCount"] = _draggingItemsCount;
                if (dragAdorner != null && ic is ItemsControl)
                {
                    if (inp.Data != null)
                    {
                        var newDataObject = inp.Data;
                        if (newDataObject != null && !newDataObject.Equals(_previousDataObject))
                        {
                            var newDraggables = isd.QueryDropDraggables(newDataObject);
                            dragAdorner.DraggingItems = newDraggables;
                            pd["DraggingItemsCount"] = _draggingItemsCount = newDraggables.Count();
                            var hintTemplate = AttachedProperties.GetDragItemTemplate(ic);
                            dragAdorner.DraggingItemTemplate = hintTemplate ?? (ic as ItemsControl).ItemTemplate;
                        }

                        _previousDataObject = newDataObject;
                    }
                }
                pd["ParentWindow"] = parentWindow;
                pd["DragAdorner"] = dragAdorner;
                return new UpdateAdornerPosition(_nextCommand);
            }

            return ResultCommand.NoError;
        }
    }



    public class UpdateAdornerPosition : ScriptCommandBase
    {
        public UpdateAdornerPosition(IScriptCommand nextCommand)
            : base("UpdateAdornerPosition", nextCommand, "EventArgs")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as UIElement;
            var inp = pd.Input as IUIDragInput;
            //var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = pd.ContainsKey("ParentWindow") ? pd["ParentWindow"] as Window : Window.GetWindow(ic);
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);

            if (isd != null)
            {
                var offsetPosition = pd.ContainsKey("PointerOffsetPosition") ? (Point)pd["PointerOffsetPosition"] :
                    new Point(0, 0);
                var dragAdorner = pd.ContainsKey("DragAdorner") ? pd["DragAdorner"] as DragAdorner :
                    AttachedProperties.GetDragAdorner(parentWindow); ;
                if (dragAdorner != null)
                {
                    var adornerPos = pd.Input.PositionRelativeTo(parentWindow);

                    adornerPos.Offset(offsetPosition.X, offsetPosition.Y);
                    dragAdorner.PointerPosition = adornerPos;

                    dragAdorner.IsDragging = true;
                    return _nextCommand;
                }
            }

            return ResultCommand.NoError;
        }
    }

    public class UpdateAdornerText : ScriptCommandBase
    {
        public UpdateAdornerText()
            : base("UpdateAdorner", "EventArgs")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as UIElement;
            Window parentWindow = pd.ContainsKey("ParentWindow") ? pd["ParentWindow"] as Window : Window.GetWindow(c);
            DragAdorner dragAdorner = pd.ContainsKey("DragAdorner") ? pd["DragAdorner"] as DragAdorner :
                    AttachedProperties.GetDragAdorner(parentWindow); ;
            var inp = pd.Input as IUIDragInput;

            FrameworkElement ele;
            ISupportDrop isd = DataContextFinder.GetDataContext(pm, out ele, DataContextFinder.SupportDrop);
            if (isd != null && inp != null && dragAdorner != null)
            {
                QueryDropResult queryEffects = isd.QueryDrop(inp.Data, inp.AllowedEffects);

                DragDropEffects eff = inp.Effects = inp.AllowedEffects & queryEffects.SupportedEffects;

                if (eff != DragDropEffects.None)
                {
                    var dragMethod =
                        inp.Data.GetDataPresent(typeof(AttachedProperties.DragMethod)) ?
                        (AttachedProperties.DragMethod)inp.Data.GetData(typeof(AttachedProperties.DragMethod))
                        : AttachedProperties.DragMethod.Normal;

                    dragAdorner.Text = String.Format("{0} {1} items to {2}",
                        dragMethod == AttachedProperties.DragMethod.Menu ? inp.AllowedEffects : queryEffects.PreferredEffect,
                        (int)pd["DraggingItemsCount"],
                        isd.DropTargetLabel);
                }

                else
                {
                    dragAdorner.Text = null;
                    AttachedProperties.SetIsDraggingOver(ele, false);
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
            var c = pd.Sender as UIElement;
            var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = Window.GetWindow(c);

            var dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);
            if (dragAdorner != null)
                dragAdorner.IsDragging = false;

            return ResultCommand.NoError;
        }
    }


    public class AttachAdorner : ScriptCommandBase
    {
        public AttachAdorner(IScriptCommand nextCommand)
            : base("AttachAdorner", nextCommand, "EventArgs")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as UIElement;

            Window parentWindow = Window.GetWindow(c);

            if (parentWindow != null)
            {
                pm["DragDropAdorner"] = AttachedProperties.GetDragAdorner(parentWindow);
                if (pm["DragDropAdorner"] != null)
                    return _nextCommand;

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
                    return _nextCommand;
                }
            }

            return ResultCommand.Error(new Exception("Control do not have parent window."));
        }
    }

    public class DetachAdorner : ScriptCommandBase
    {
        public DetachAdorner(IScriptCommand nextCommand = null) : base("DetachAdorner", nextCommand, "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as UIElement;

            Window parentWindow = Window.GetWindow(c);

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
                        return _nextCommand;
                    }
                }
            }

            return _nextCommand;
        }
    }


}
