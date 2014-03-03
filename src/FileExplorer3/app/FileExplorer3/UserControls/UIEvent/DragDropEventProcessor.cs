﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileExplorer.BaseControls;
using FileExplorer.BaseControls.DragnDrop;

namespace FileExplorer.BaseControls
{
    public class DragDropEventProcessor : UIEventProcessorBase
    {
        public DragDropEventProcessor()
        {
            _processEvents.AddRange(
                new[] {
                    FrameworkElement.PreviewMouseDownEvent,
                    UIEventHub.MouseDragEvent,
                    FrameworkElement.PreviewMouseUpEvent,
                    FrameworkElement.MouseMoveEvent,

                    FrameworkElement.DragEnterEvent,
                    FrameworkElement.DragOverEvent,
                    FrameworkElement.DragLeaveEvent,
                    FrameworkElement.DropEvent

                    
                    //FrameworkElement.PreviewTouchDownEvent,
                    //FrameworkElement.TouchMoveEvent,
                    //FrameworkElement.PreviewTouchUpEvent
                }
             );
        }

        public override Cofe.Core.Script.IScriptCommand OnEvent(RoutedEvent eventId)
        {
            if (EnableDrag)
                switch (eventId.Name)
                {
                    case "PreviewMouseDown":
                    //case "PreviewTouchDown":
                        return new RecordStartSelectedItem();
                    case "MouseDrag": 
                        return new BeginDrag();
                    case "PreviewMouseUp":
                    //case "PreviewTouchUp": 
                        return new EndDrag();
                    case "MouseMove":
                    //case "TouchMove": 
                        return new ContinueDrag();
                }

            if (EnableDrop)
                switch (eventId.Name)
                {
                    case "DragEnter": return new QueryDragDropEffects(QueryDragDropEffectMode.Enter);
                    case "DragOver": return new ContinueDrop();
                    case "DragLeave": return new QueryDragDropEffects(QueryDragDropEffectMode.Leave);
                    case "Drop": return new BeginDrop();
                }

            return base.OnEvent(eventId);
        }


        public static DependencyProperty EnableDragProperty =
            DependencyProperty.Register("EnableDrag", typeof(bool),
            typeof(DragDropEventProcessor), new PropertyMetadata(false));

        public bool EnableDrag
        {
            get { return (bool)GetValue(EnableDragProperty); }
            set { SetValue(EnableDragProperty, value); }
        }

        public static DependencyProperty EnableDropProperty =
            DependencyProperty.Register("EnableDrop", typeof(bool),
            typeof(DragDropEventProcessor), new PropertyMetadata(false));

        public bool EnableDrop
        {
            get { return (bool)GetValue(EnableDropProperty); }
            set { SetValue(EnableDropProperty, value); }
        }
    }
}
