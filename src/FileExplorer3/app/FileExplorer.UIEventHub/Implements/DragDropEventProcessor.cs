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
using System.Windows.Input;

namespace FileExplorer.WPF.BaseControls
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

        protected override FileExplorer.Script.IScriptCommand onEvent(RoutedEvent eventId)
        {
            if (EnableDrag)
                switch (eventId.Name)
                {
                    case "PreviewMouseDown":
                        return //Find a DataContext that implement SupportDrag.
                        HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}",
                            DataContextType.SupportShellDrag, "{ISupportDrag}", false,
                            //And If there's one.
                            ScriptCommands.IfAssigned("{ISupportDrag}",
                            //Calculate a number of positions.
                                HubScriptCommands.ObtainPointerPosition(
                            //Assign the datacontext item to {ItemUnderMouse}
                                    HubScriptCommands.AssignItemUnderMouse("{ItemUnderMouse}", false,
                            //And set Sender's StartDraggingItem to {ItemUnderMouse}        
                                        ScriptCommands.IfAssigned("{ItemUnderMouse}",
                                          ScriptCommands.IfTrue("{ItemUnderMouse.IsSelected}",
                                        HubScriptCommands.SetDependencyProperty("{Sender}",
                                            AttachedProperties.StartDraggingItemProperty, "{ItemUnderMouse}", 
                                            HubScriptCommands.SetRoutedEventHandled())))))));
                    case "MouseDrag":
                        return //If event not marked handled.
                        HubScriptCommands.IfNotRoutedEventHandled(
                        HubScriptCommands.IfDependencyPropertyEqualDefaultValue<object>("{Sender}",
                                AttachedProperties.StartDraggingItemProperty,
                            //If StartDraggingProperty = null, return.
                            ResultCommand.NoError,
                            ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false, 
                            //If StartDraggingProperty != null, Check and set IsDraggingProperty to true.
                            HubScriptCommands.SetDependencyPropertyIfDifferent("{Sender}",
                                AttachedProperties.IsDraggingProperty, true,                                 
                            //If changed IsDraggingProperty, Find DataContext that support ISupportDrag to {ISupportDrag} variable.
                                HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrag, "{ISupportDrag}", false,
                                  ScriptCommands.RunSequence(
                                        HubScriptCommands.IfMouseGesture(new MouseGesture() { MouseAction = MouseAction.RightClick },
                                            ScriptCommands.Assign(DragDropCommand.DragMethodKey, DragMethod.Menu),
                                            ScriptCommands.Assign(DragDropCommand.DragMethodKey, DragMethod.Normal)),
                                        HubScriptCommands.SetRoutedEventHandled(
                                        //Initialize shell based drag drop
                                        HubScriptCommands.StartDragDrop("{ISupportDrag}",
                                          ScriptCommands.PrintDebug("NotifyDragCompleted")))))))));
                                    //If ISupportDrag is assigned to a non-null value,                                     
                                    //Initialize DragLiteParameters (in {DragDrop} global parameterDic)
                                    //DoDragDrop
                                    //new NotifyDropCompleted(_isd, draggables, dataObj, resultEffect);

                    case "PreviewMouseUp":
                        return
                            ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                                HubScriptCommands.SetDependencyProperty("{Sender}", AttachedProperties.StartDraggingItemProperty, null,
                                    HubScriptCommands.SetDependencyPropertyTyped("{Sender}", AttachedProperties.IsDraggingProperty, false,
                                        ScriptCommands.IfEquals(DragDropCommand.DragMethodKey, DragMethod.Menu,
                                            ResultCommand.NoError,
                                            HubScriptCommands.DetachDragDropAdorner()))));
                    case "MouseMove":
                        return HubScriptCommands.IfDependencyProperty("{Sender}", AttachedProperties.IsDraggingProperty, 
                                    ComparsionOperator.Equals, true,
                                    HubScriptCommands.SetRoutedEventHandled());
                }

            //if (EnableDrop)
            //    switch (eventId.Name)
            //    {
            //        case "DragEnter": return new QueryDragDropEffects(QueryDragDropEffectMode.Enter);
            //        case "DragOver": return new ContinueDrop();
            //        case "DragLeave": return new QueryDragDropEffects(QueryDragDropEffectMode.Leave);
            //        case "Drop": return new BeginDrop();
            //    }

            return base.onEvent(eventId);
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
