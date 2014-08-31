using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.UIEventHub;
using FileExplorer.WPF.BaseControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF.BaseControls
{
    public class CanvasDragEventProcessor : UIEventProcessorBase
    {
        public CanvasDragEventProcessor()
        {
            _processEvents.AddRange(
               new[] {
                    FrameworkElement.PreviewMouseDownEvent,
                    UIEventHub.MouseDragEvent,
                    UIEventHub.TouchDragEvent,
                    FrameworkElement.PreviewMouseUpEvent,
                    FrameworkElement.MouseMoveEvent,

                    FrameworkElement.PreviewTouchDownEvent,
                    FrameworkElement.TouchMoveEvent,
                    FrameworkElement.PreviewTouchUpEvent
                }
            );
        }


        //case "KeyDown":
        //            return WPFScriptCommands.IfKeyPressed(System.Windows.Input.Key.Escape, 
        //                SetDragLiteState.Reset(ResultCommand.OK), null);

        //        case "PreviewTouchDown":
        //            return EnableDrag && EnableTouch ? (IScriptCommand)new RecordStartSelectedItem()
        //              : ResultCommand.NoError;
        //        case "PreviewMouseDown":
        //            return EnableDrag && EnableMouse ? (IScriptCommand)new RecordStartSelectedItem()
        //                 : ResultCommand.NoError;
        //        case "TouchDrag":
        //            return EnableDrag && EnableTouch ? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
        //        case "MouseDrag":
        //            return EnableDrag && EnableMouse? (IScriptCommand)new BeginDragLite() : ResultCommand.NoError;
        //        case "TouchUp":
        //            return EnableDrop && EnableTouch ? (IScriptCommand)new EndDragLite() : new DetachAdorner();
        //        case "MouseUp":
        //            return EnableDrop && EnableMouse ? (IScriptCommand)new EndDragLite() : new DetachAdorner();
        //        //case "MouseLeave":
        //        //case "TouchLeave":
        //        //    return new DetachAdorner();
        //        case "TouchMove":
        //            return EnableTouch ?  (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
        //        case "MouseMove":
        //            return EnableMouse ? (IScriptCommand)new ContinueDragLite(EnableDrag, EnableDrop) : ResultCommand.NoError;
        protected override FileExplorer.Script.IScriptCommand onEvent(RoutedEvent eventId)
        {            

            switch (eventId.Name)
            {
                case "PreviewTouchDown":
                case "PreviewMouseDown":
                    return HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportDrag, "{ISupportDrag}", false,
                          ScriptCommands.IfAssigned("{ISupportDrag}",
                            HubScriptCommands.ObtainPointerPosition(
                           HubScriptCommands.AssignItemUnderMouse("{ItemUnderMouse}", false,
                             HubScriptCommands.SetDependencyProperty("{Sender}",
                                AttachedProperties.StartDraggingItemProperty, "{ItemUnderMouse}")))));
                case "TouchDrag":
                case "MouseDrag":
                    return 
                        HubScriptCommands.IfNotRoutedEventHandled(
                        HubScriptCommands.IfDependencyPropertyEqualDefaultValue<object>("{Sender}", AttachedProperties.StartDraggingItemProperty, 
                            //If StartDraggingProperty = null, return.
                            ResultCommand.NoError,
                            //If StartDraggingProperty != null, Check and set IsDraggingProperty to true.
                            HubScriptCommands.SetDependencyPropertyIfDifferent("{Sender}",
                                AttachedProperties.IsDraggingProperty, true,
                                //If changed IsDraggingProperty, Find DataContext that support ISupportDrag to {ISupportDrag} variable.
                                HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportDrag, "{ISupportDrag}", false,                                
                                    //If ISupportDrag is assigned to a non-null value, Initialize DragLiteParameters.
                                    HubScriptCommands.StartDragDropLite("{ISupportDrag}",
                                        //then set {EventArgs.Handled} to true.
                                        HubScriptCommands.SetRoutedEventHandled( 
                                            //And attach/update adorner.
                                            ScriptCommands.PrintDebug("ToDo:AttachAdorner/UpdateAdorner/UpdateAdornerText"))))
                           , null)));
                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return HubScriptCommands.SetDependencyPropertyIfDifferent("{Sender}",
                               AttachedProperties.IsDraggingProperty, false,
                               HubScriptCommands.SetDependencyPropertyTyped<object>("{Sender}",
                               AttachedProperties.StartDraggingItemProperty, null,
                                  //Set {EventArgs.Handled} to true.
                                  HubScriptCommands.SetRoutedEventHandled( 
                                           ScriptCommands.PrintDebug("ToDo:DetachAdorner"))));
            
            }

            return base.onEvent(eventId);
        }
    }
}
