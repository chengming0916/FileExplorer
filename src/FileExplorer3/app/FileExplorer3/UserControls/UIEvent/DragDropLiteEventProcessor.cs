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

                    UIEventHub.TouchDragEvent,
                    FrameworkElement.PreviewTouchDownEvent,
                    FrameworkElement.TouchMoveEvent,
                    FrameworkElement.PreviewTouchUpEvent
                }
             );
        }

        public override Cofe.Core.Script.IScriptCommand OnEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "PreviewTouchDown":
                case "PreviewMouseDown":
                    return new RecordStartSelectedItem();
                case "TouchDrag":
                case "MouseDrag":
                    return new BeginDragLite();
                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return new EndDragLite();
                case "TouchMove":
                    case "MouseMove":
                    return new ContinueDragLite();
            }


            return base.OnEvent(eventId);
        }


    }


}
