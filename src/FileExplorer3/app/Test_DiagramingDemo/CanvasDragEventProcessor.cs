using FileExplorer.Script;
using FileExplorer.WPF.BaseControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiagramingDemo
{
    public class CanvasDragEventProcessor : UIEventProcessorBase
    {
        public CanvasDragEventProcessor()
        {
            _processEvents.AddRange(
                new[] {
                 FrameworkElement.KeyDownEvent,

                    FrameworkElement.PreviewMouseDownEvent,
                    //UIEventHub.MouseDragEvent,
                    //FrameworkElement.PreviewMouseUpEvent,
                    //FrameworkElement.MouseMoveEvent,
                    //FrameworkElement.MouseLeaveEvent,

                    //FrameworkElement.TouchLeaveEvent,
                    //UIEventHub.TouchDragEvent,
                    //FrameworkElement.PreviewTouchDownEvent,
                    //FrameworkElement.TouchMoveEvent,
                    //FrameworkElement.TouchUpEvent //Not Preview or it would trigger parent's PreviewTouchUp first.
                }
             );
        }

        protected override IScriptCommand onEvent(RoutedEvent eventId)
        {
            //switch (eventId.Name)
            //{
            //    case "KeyDown":
            //        return WPFScriptCommands.IfKeyPressed(System.Windows.Input.Key.Escape,
            //            SetDragLiteState.Reset(ResultCommand.OK), null);

            //    case "PreviewTouchDown":
            //    case "PreviewMouseDown":
            //        return EnableDrag ? (IScriptCommand)new RecordStartSelectedItem()
            //             : ResultCommand.NoError;
            //    //case "TouchDrag":
            //    //    return EnableDrag && EnableTouch ? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
            //    //case "MouseDrag":
            //    //    return EnableDrag && EnableMouse ? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
            //    //case "TouchUp":
            //    //    return (IScriptCommand)new EndDragLite();
            //    //case "MouseUp":
            //    //    return (IScriptCommand)new EndDragLite();
            //    ////case "MouseLeave":
            //    ////case "TouchLeave":
            //    ////    return new DetachAdorner();
            //    //case "TouchMove":
            //    //    return EnableTouch ? (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
            //    //case "MouseMove":
            //    //    return EnableMouse ? (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
            //}


            return base.onEvent(eventId);
        }        

        //public static DependencyProperty EnableDragProperty =
        //   DragDropEventProcessor.EnableDragProperty.AddOwner(typeof(CanvasDragEventProcessor));

        //public bool EnableDrag
        //{
        //    get { return (bool)GetValue(EnableDragProperty); }
        //    set { SetValue(EnableDragProperty, value); }
        //}
    }
}
