﻿using System;
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
                bool isDraggingOver = isd.IsDraggingOver = _mode == QueryDragDropEffectMode.Enter;
                AttachedProperties.SetIsDraggingOver(ele, isDraggingOver);



              


                if (_mode == QueryDragDropEffectMode.Enter)
                {
                    AttachedProperties.SetDraggingOverItem(ic, ele);
                    eventArgs.Handled = true;
                    return new AttachAdorner();

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
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;
            var parentWindow = Window.GetWindow(ic);
            

            return new UpdateAdorner(false);
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
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;
            Window parentWindow = Window.GetWindow(ic);

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
                            new NotifyDropCompleted(_isDrag, _isDrag.GetDraggables(), _dataObject, _dragAdorner.DragDropEffect)
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
        public BeginDrop() : base("BeginDrop", "EventArgs") { }

        public BeginDrop(DragDropEffects overrideDragDropEffect) : base("BeginDrop", "EventArgs") { _overrideDragDropEffect = overrideDragDropEffect; }

        DragDropEffects? _overrideDragDropEffect;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            AttachedProperties.SetDraggingOverItem(ic, null);
            var eventArgs = pd.EventArgs as DragEventArgs;

            var dragMethod =
                    eventArgs.Data.GetDataPresent(typeof(AttachedProperties.DragMethod)) ?
                    (AttachedProperties.DragMethod)eventArgs.Data.GetData(typeof(AttachedProperties.DragMethod))
                    : AttachedProperties.DragMethod.Normal;

            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                QueryDropResult queryDropResult = isd.QueryDrop(eventArgs.Data, eventArgs.AllowedEffects);
                DragDropEffects supportedEffects = eventArgs.AllowedEffects & queryDropResult.SupportedEffects;
                if (supportedEffects != DragDropEffects.None && !_overrideDragDropEffect.HasValue &&
                    dragMethod == AttachedProperties.DragMethod.Menu && eventArgs.Data.GetDataPresent(typeof(ISupportDrag)))
                {
                    eventArgs.Effects = DragDropEffects.None;
                    eventArgs.Handled = true;
                    ISupportDrag isDrag = eventArgs.Data.GetData(typeof(ISupportDrag)) as ISupportDrag;
                    return new ShowAdornerContextMenu(supportedEffects, queryDropResult.PreferredEffect, isDrag, isd, eventArgs.Data);
                }

                //If OverrideDragDropEffect is set, use it instead.
                supportedEffects = _overrideDragDropEffect.HasValue ? _overrideDragDropEffect.Value : supportedEffects;

                if (supportedEffects != DragDropEffects.None)
                {
                    IEnumerable<IDraggable> draggables = isd.QueryDropDraggables(eventArgs.Data);
                    eventArgs.Effects = isd.Drop(draggables, eventArgs.Data, supportedEffects);
                    eventArgs.Handled = true;
                }

                return new DetachAdorner();
            }

            return ResultCommand.NoError;
        }
    }


    public class UpdateAdorner : ScriptCommandBase
    {
        public UpdateAdorner(bool updateDraggables)
            : base("UpdateAdorner", "EventArgs")
        { _updateDraggables = updateDraggables; }

        private bool _updateDraggables;
        private static IDataObject _previousDataObject = null;
        private static int _draggingItemsCount = 0;

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
                    if (_updateDraggables && !eventArgs.Data.Equals(_previousDataObject))
                    {
                        var newDraggables = isd.QueryDropDraggables(eventArgs.Data);
                        dragAdorner.DraggingItems = newDraggables;
                        pd["DraggingItemsCount"] = _draggingItemsCount = newDraggables.Count();
                        var hintTemplate = AttachedProperties.GetDragItemTemplate(ic);
                        dragAdorner.DraggingItemTemplate = hintTemplate ?? ic.ItemTemplate;


                        _previousDataObject = eventArgs.Data;
                    }
                    else pd["DraggingItemsCount"] = _draggingItemsCount;
                
                    dragAdorner.PointerPosition = eventArgs.GetPosition(parentWindow);
                    dragAdorner.IsDragging = true;

                }

                return new UpdateAdornerText(dragAdorner);
            }

            return ResultCommand.NoError;
        }
    }

    public class UpdateAdornerText : ScriptCommandBase
    {
        public UpdateAdornerText(DragAdorner dragAdorner)
            : base("UpdateAdorner", "EventArgs")
        { _dragAdorner = dragAdorner; }

        private DragAdorner _dragAdorner;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;
            FrameworkElement ele;
            ISupportDrop isd = DataContextFinder.GetDataContext(pm, out ele, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                QueryDropResult queryEffects = isd.QueryDrop(eventArgs.Data, eventArgs.AllowedEffects);

                eventArgs.Effects = eventArgs.AllowedEffects & queryEffects.SupportedEffects;

                if (eventArgs.Effects != DragDropEffects.None)
                {
                    var dragMethod =
                        eventArgs.Data.GetDataPresent(typeof(AttachedProperties.DragMethod)) ?
                        (AttachedProperties.DragMethod)eventArgs.Data.GetData(typeof(AttachedProperties.DragMethod))
                        : AttachedProperties.DragMethod.Normal;

                    _dragAdorner.Text = String.Format("{0} {1} items to {2}",
                        dragMethod == AttachedProperties.DragMethod.Menu ? eventArgs.AllowedEffects : queryEffects.PreferredEffect,
                        (int)pd["DraggingItemsCount"],
                        isd.DropTargetLabel);
                }

                else
                {
                    _dragAdorner.Text = null;
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
                    return new UpdateAdorner(true);

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
                    return new UpdateAdorner(true);
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


}