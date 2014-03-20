﻿using Cofe.Core.Script;
using FileExplorer.BaseControls.DragnDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.BaseControls
{
    public class DragDropLiteEventProcessor : UIEventProcessorBase
    {
        public DragDropLiteEventProcessor()
        {
            _processEvents.AddRange(
                new[] {
                 
                    FrameworkElement.PreviewMouseDownEvent,
                    UIEventHub.MouseDragEvent,
                    FrameworkElement.PreviewMouseUpEvent,
                    FrameworkElement.MouseMoveEvent,
                    FrameworkElement.MouseLeaveEvent,

                    FrameworkElement.TouchLeaveEvent,
                    UIEventHub.TouchDragEvent,
                    FrameworkElement.PreviewTouchDownEvent,
                    FrameworkElement.TouchMoveEvent,
                    FrameworkElement.TouchUpEvent //Not Preview or it would trigger parent's PreviewTouchUp first.
                }
             );
        }

        public override IScriptCommand OnEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "PreviewTouchDown":
                case "PreviewMouseDown":
                    return EnableDrag ? (IScriptCommand)new RecordStartSelectedItem() : ResultCommand.NoError;
                case "TouchDrag":
                case "MouseDrag":
                    return EnableDrag ? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
                case "TouchUp":
                case "MouseUp": 
                    return EnableDrop ? (IScriptCommand)new EndDragLite() : new DetachAdorner();
                //case "MouseLeave":
                //case "TouchLeave":
                //    return new DetachAdorner();
                case "TouchMove":
                case "MouseMove":
                    return new ContinueDragLite(EnableDrag, EnableDrop);
            }


            return base.OnEvent(eventId);
        }

        public static DependencyProperty EnableDragProperty =
           DragDropEventProcessor.EnableDragProperty.AddOwner(typeof(DragDropLiteEventProcessor));

        public bool EnableDrag
        {
            get { return (bool)GetValue(EnableDragProperty); }
            set { SetValue(EnableDragProperty, value); }
        }

        public static DependencyProperty EnableDropProperty =
            DragDropEventProcessor.EnableDropProperty.AddOwner(typeof(DragDropLiteEventProcessor));

        public bool EnableDrop
        {
            get { return (bool)GetValue(EnableDropProperty); }
            set { SetValue(EnableDropProperty, value); }
        }


    }


}
