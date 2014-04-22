using Cofe.Core.Script;
using FileExplorer.BaseControls.DragnDrop;
using FileExplorer.ViewModels;
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
                 FrameworkElement.KeyDownEvent,

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
                case "KeyDown":
                    return ScriptCommands.IfKeyPressed(System.Windows.Input.Key.Escape, 
                        SetDragLiteState.Reset(ResultCommand.OK), null);

                case "PreviewTouchDown":
                    return EnableDrag && EnableTouch ? (IScriptCommand)new RecordStartSelectedItem()
                      : ResultCommand.NoError;
                case "PreviewMouseDown":
                    return EnableDrag && EnableMouse ? (IScriptCommand)new RecordStartSelectedItem()
                         : ResultCommand.NoError;
                case "TouchDrag":
                    return EnableDrag && EnableTouch ? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
                case "MouseDrag":
                    return EnableDrag && EnableMouse? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
                case "TouchUp":
                    return EnableDrop && EnableTouch ? (IScriptCommand)new EndDragLite() : new DetachAdorner();
                case "MouseUp":
                    return EnableDrop && EnableMouse ? (IScriptCommand)new EndDragLite() : new DetachAdorner();
                //case "MouseLeave":
                //case "TouchLeave":
                //    return new DetachAdorner();
                case "TouchMove":
                    return EnableTouch ?  (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
                case "MouseMove":
                    return EnableMouse ? (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
            }


            return base.OnEvent(eventId);
        }

        public static DependencyProperty EnableMouseProperty =
          DependencyProperty.Register("EnableMouse", typeof(bool), typeof(DragDropLiteEventProcessor), 
          new PropertyMetadata(true));

        public bool EnableMouse
        {
            get { return (bool)GetValue(EnableMouseProperty); }
            set { SetValue(EnableMouseProperty, value); }
        }

        public static DependencyProperty EnableTouchProperty =
         DependencyProperty.Register("EnableTouch", typeof(bool), typeof(DragDropLiteEventProcessor),
         new PropertyMetadata(true));

        public bool EnableTouch
        {
            get { return (bool)GetValue(EnableTouchProperty); }
            set { SetValue(EnableTouchProperty, value); }
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
