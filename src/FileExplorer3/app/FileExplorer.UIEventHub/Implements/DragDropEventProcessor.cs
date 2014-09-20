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
                            DataContextType.SupportShellDrag, "{ISupportDrag}", null, false,
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
                                HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrag, "{ISupportDrag}", null, false,
                                  ScriptCommands.RunSequence(
                                        HubScriptCommands.IfMouseGesture(new MouseGesture() { MouseAction = MouseAction.RightClick },
                                            ScriptCommands.Assign(QueryDrag.DragMethodKey, DragMethod.Menu),
                                            ScriptCommands.Assign(QueryDrag.DragMethodKey, DragMethod.Normal)),
                                        HubScriptCommands.SetRoutedEventHandled(
                            //Initialize shell based drag drop
                                        HubScriptCommands.QueryDrag("{ISupportDrag}", "{DragResult}", 
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
                                        ScriptCommands.IfEquals(QueryDrag.DragMethodKey, DragMethod.Menu,
                                            ResultCommand.NoError,
                            //This is defined in drop.
                                            HubScriptCommands.DetachDragDropAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}")))));
                    case "MouseMove":
                        return HubScriptCommands.IfDependencyProperty("{Sender}", AttachedProperties.IsDraggingProperty,
                                    ComparsionOperator.Equals, true,
                                    HubScriptCommands.SetRoutedEventHandled());
                }

            if (EnableDrop)
            {
                FileExplorer.Script.Print.PrintConsoleAction = t => Console.WriteLine(t);
                IScriptCommand updateAdornerPosition = HubScriptCommands.AssignCursorPosition(PositionRelativeToType.Window, "{DragDrop.CursorPosition}", false,
                             ScriptCommands.SetProperty("{DragDrop.Adorner}", (DragAdorner a) => a.PointerPosition, "{DragDrop.CursorPosition}"));

                IScriptCommand updateAdornerText =
                            ScriptCommands.FormatText("{DragDrop.QueryDropResult.PreferredEffect} " + 
                                                      "{DragDrop.Draggables.Length} items " + 
                                                      "to {ISupportDrop.DropTargetLabel}", "{Label}", false,
                            ScriptCommands.SetProperty("{DragDrop.Adorner}", (DragAdorner a) => a.Text, "{Label}"));


                IScriptCommand updateAdornerDraggables =
                            ScriptCommands.SetProperty("{DragDrop.Adorner}", (DragAdorner a) => a.DraggingItems, "{DragDrop.Draggables}");
                IScriptCommand updateAdorner = ScriptCommands.RunSequence(null,
                        updateAdornerPosition,
                        updateAdornerDraggables,
                        updateAdornerText);
                IScriptCommand detachAdorner = HubScriptCommands.DetachAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}",
                    ScriptCommands.Reset(null, "{DragDrop.Adorner}", "{DragDrop.AdornerLayer}"));
                IScriptCommand resetDragDrop =
                    ScriptCommands.Reset(null, "{DragDrop.Adorner}", "{DragDrop.AdornerLayer}",
                    "{DragDrop.PreviousSupportDrop}", "{DragDrop.PreviousElementSupportDrop}", "{DragDrop.Draggables}");
                IScriptCommand attachAdorner =
                    ScriptCommands.RunSequence(null,
                    detachAdorner,
                    HubScriptCommands.AttachDragDropAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}",
                        updateAdorner));


                switch (eventId.Name)
                {
                    case "DragEnter": return
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                            //Find DataContext that support IShellDrop
                        HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrop, "{ISupportDrop}", "{ElementSupportDrop}", false,
                            //And if so,
                          ScriptCommands.IfAssigned("{ISupportDrop}",
                            //ScriptCommands.IfEquals("{ISupportDrop}", "{DragDrop.PreviousSupportDrop}",
                            //    ResultCommand.NoError,
                                ScriptCommands.Assign(new Dictionary<string, object>()
                                {
                                    { "{DragDrop.PreviousSupportDrop}", "{ISupportDrop}" }, 
                                    { "{DragDrop.PreviousElementSupportDrop}", "{ElementSupportDrop}" }, 
                                }, false,

                            //Set DataContext.IsDraggingOver to truw.
                            ScriptCommands.SetPropertyValue("{ISupportDrop}", "IsDraggingOver", true,
                            //Mark event handled.
                              HubScriptCommands.SetRoutedEventHandled(
                            //Get Draggables from {ISupportDrop} and set to {DragDrop.Draggables}
                            HubScriptCommands.AssignDataObject("{DataObj}", false,
                                HubScriptCommands.QueryDraggablesFromDataObject("{ISupportDrop}", "{DataObj}", "{DragDrop.Draggables}", false,
                                    HubScriptCommands.QueryDropEffects("{ISupportDrop}", "{DragDrop.Draggables}", "{DataObj}",
                                        "{DragDrop.QueryDropResult}", false,
                                  ScriptCommands.IfAssigned("{DragDrop.Draggables}",
                                         attachAdorner))))))))));

                    case "DragOver": return
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                            HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrop, "{ISupportDrop}", "{ElementSupportDrop}", false,
                            //And if so,                            
                                ScriptCommands.IfEquals("{ISupportDrop}", "{DragDrop.PreviousSupportDrop}",
                                    updateAdornerPosition,
                                    updateAdorner
                             )));

                    case "DragLeave": return
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                          ScriptCommands.IfAssigned("{DragDrop.PreviousSupportDrop}",
                        ScriptCommands.RunSequence(null,
                            detachAdorner,
                            resetDragDrop)));

                    case "Drop": return
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                           HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrop,
                                "{ISupportDrop}", "{ElementSupportDrop}", false,
                            ScriptCommands.IfAssigned("{ISupportDrop}",
                            ScriptCommands.RunSequence(null,
                            HubScriptCommands.AssignDataObject("{DataObj}", false, 
                                  HubScriptCommands.QueryDraggablesFromDataObject("{ISupportDrop}", "{DataObj}", "{DragDrop.Draggables}", false,
                                  HubScriptCommands.QueryDrop("{ISupportDrop}", "{DragDrop.Draggables}", "{DataObj}", 
                                  "{DragDrop.QueryDropResult.PreferredEffect}", "{DragDrop.DropResult}", false))),
                                detachAdorner, resetDragDrop))));

                }
            }

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
