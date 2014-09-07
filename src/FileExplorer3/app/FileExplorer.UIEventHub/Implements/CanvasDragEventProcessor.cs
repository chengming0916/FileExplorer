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
                                    //If ISupportDrag is assigned to a non-null value,                                     
                                    //Initialize DragLiteParameters (in {DragDrop} global parameterDic).
                                        HubScriptCommands.StartDragDropLite("{ISupportDrag}",
                                        //then set {EventArgs.Handled} to true.
                                        HubScriptCommands.SetRoutedEventHandled( 
                                            //And attach/update adorner.
                                            HubScriptCommands.AttachSelectedItemsAdorner("{SelectedItemsAdorner}", 
                                              HubScriptCommands.UpdateSelectedItemsAdorner("{SelectedItemsAdorner}")))))
                           , null)));
                case "MouseMove" :
                case "TouchMove":
                    return  HubScriptCommands.ThrottleTouchDrag(5, 
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false, 
                            ScriptCommands.IfEquals(DragDropLiteCommand.DragDropModeKey, "Canvas",
                                HubScriptCommands.ObtainPointerPosition(HubScriptCommands.UpdateSelectedItemsAdorner("{SelectedItemsAdorner}")))));
                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return 
                                HubScriptCommands.SetDependencyPropertyIfDifferent("{Sender}", AttachedProperties.IsDraggingProperty, false,
                                HubScriptCommands.SetDependencyPropertyTyped<object>("{Sender}", AttachedProperties.StartDraggingItemProperty, null,                                  
                                    HubScriptCommands.EndDragDropCanvas(
                                  //Set {EventArgs.Handled} to true.
                                    HubScriptCommands.SetRoutedEventHandled(
                                           HubScriptCommands.DettachSelectedItemsAdorner("SelectedItemsAdorner}")))));
            
            }

            return base.onEvent(eventId);
        }
    }
}
