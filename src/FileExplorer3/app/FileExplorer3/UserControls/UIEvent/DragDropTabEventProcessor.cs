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
    public class DragDropTabEventProcessor : DragDropLiteEventProcessor
    {
        public DragDropTabEventProcessor()
        {
            //_processEvents.AddRange(
            //    new[] {
            //     FrameworkElement.KeyDownEvent,

            //        FrameworkElement.PreviewMouseDownEvent,
            //        UIEventHub.MouseDragEvent,
            //        FrameworkElement.PreviewMouseUpEvent,
            //        FrameworkElement.MouseMoveEvent,
            //        FrameworkElement.MouseLeaveEvent,

            //        FrameworkElement.TouchLeaveEvent,
            //        UIEventHub.TouchDragEvent,
            //        FrameworkElement.PreviewTouchDownEvent,
            //        FrameworkElement.TouchMoveEvent,
            //        FrameworkElement.TouchUpEvent //Not Preview or it would trigger parent's PreviewTouchUp first.
            //    }
            // );
        }

        public override IScriptCommand OnEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "TouchDrag":
                    return EnableDrag && EnableTouch ? (IScriptCommand)new BeginDragTab() : ResultCommand.NoError;
                case "MouseDrag":
                    return EnableDrag && EnableMouse ? (IScriptCommand)new BeginDragTab() : ResultCommand.NoError;
                case "TouchUp":
                    return EnableDrop && EnableTouch ? (IScriptCommand)new EndDragTab() : ResultCommand.NoError;
                case "MouseUp":
                    return EnableDrop && EnableMouse ? (IScriptCommand)new EndDragTab() : ResultCommand.NoError;
                //case "MouseLeave":
                //case "TouchLeave":
                //    return new DetachAdorner();
                //case "TouchMove":
                //    return EnableTouch ? (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
                //case "MouseMove":
                //    return EnableMouse ? (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
            }


            return base.OnEvent(eventId);
        }

        //public static DependencyProperty EnableMouseProperty =
        //  DragDropLiteEventProcessor.EnableMouseProperty.AddOwner(typeof(DragDropTabEventProcessor));

        //public bool EnableMouse
        //{
        //    get { return (bool)GetValue(EnableMouseProperty); }
        //    set { SetValue(EnableMouseProperty, value); }
        //}

        //public static DependencyProperty EnableTouchProperty =
        // DragDropLiteEventProcessor.EnableTouchProperty.AddOwner(typeof(DragDropTabEventProcessor));

        //public bool EnableTouch
        //{
        //    get { return (bool)GetValue(EnableTouchProperty); }
        //    set { SetValue(EnableTouchProperty, value); }
        //}

        //public static DependencyProperty EnableDragProperty =
        //   DragDropEventProcessor.EnableDragProperty.AddOwner(typeof(DragDropTabEventProcessor));

        //public bool EnableDrag
        //{
        //    get { return (bool)GetValue(EnableDragProperty); }
        //    set { SetValue(EnableDragProperty, value); }
        //}

        //public static DependencyProperty EnableDropProperty =
        //    DragDropEventProcessor.EnableDropProperty.AddOwner(typeof(DragDropTabEventProcessor));

        //public bool EnableDrop
        //{
        //    get { return (bool)GetValue(EnableDropProperty); }
        //    set { SetValue(EnableDropProperty, value); }
        //}


    }
}
