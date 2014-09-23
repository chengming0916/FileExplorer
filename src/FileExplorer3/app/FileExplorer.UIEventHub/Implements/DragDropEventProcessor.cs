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
        #region Constructor
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
                    FrameworkElement.DropEvent,
                    FrameworkElement.GiveFeedbackEvent
                }
             );

            Print.PrintConsoleAction = c => Console.WriteLine(c);
        }
        #endregion


       

        protected override FileExplorer.Script.IScriptCommand onEvent(RoutedEvent eventId)
        {            
            if (EnableDrag)
                switch (eventId.Name)
                {
                    case "PreviewMouseDown":
                        return 
                            ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,                                
                                    //Set Default value for CanDrag first.
                                    ScriptCommands.Assign("{DragDrop.CanDrag}", false, false,                                        
                                    //Find a DataContext that implement SupportDrag, and if assigned
                                    HubScriptCommands.IfAssignedDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrag, "{ISupportDrag}", null,                                     
                                           //and item under mouse (ISelectable) IsSelected.
                                           DragDropScriptCommands.IfItemUnderMouseIsSelected(
                                            ScriptCommands.Assign("{DragDrop.CanDrag}", true, false, 
                                                HubScriptCommands.SetRoutedEventHandled())
                                            ))));
                    case "MouseDrag":
                        return                            
                            ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                                //If not handled.
                                HubScriptCommands.IfNotRoutedEventHandled(                                
                                    //IfCanDrag (assigned from PreviewMouseDown)
                                    ScriptCommands.IfTrue("{DragDrop.CanDrag}", 
                                        //Reset CanDrag
                                        ScriptCommands.Assign("{DragDrop.CanDrag}", false, false,                                                       
                                            //If CanDrag (ItemUnderMouse is selected), Check and set IsDraggingProperty to true.
                                            HubScriptCommands.SetDependencyPropertyIfDifferentValue("{Sender}", 
                                                AttachedProperties.IsDraggingProperty, true,                                                
                                                //If changed IsDraggingProperty, Find DataContext that support ISupportDrag to {ISupportDrag} variable.
                                                HubScriptCommands.IfAssignedDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrag, "{DragDrop.SupportDrag}", null, 
                                                    //If RightMouse, then Set DragMethod to Menu, otherwise Normal
                                                    HubScriptCommands.AssignDragMethod(QueryDrag.DragMethodKey,                                                     
                                                        //Mark handled.
                                                        HubScriptCommands.SetRoutedEventHandled(                                                        
                                                            //Initialize shell based drag drop
                                                            HubScriptCommands.QueryDrag("{DragDrop.SupportDrag}", "{DragResult}", 
                                                                //Then reset SupportDrag parameter.
                                                                ScriptCommands.Assign("{DragDrop.SupportDrag}", null, false, 
                                                                    HubScriptCommands.SetDependencyPropertyValue("{Sender}", 
                                                                        AttachedProperties.IsDraggingProperty, false)))))))))));                    

                    case "PreviewMouseUp":
                        return
                            ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,                                
                                    HubScriptCommands.SetDependencyPropertyValue("{Sender}", AttachedProperties.IsDraggingProperty, false,
                                        ScriptCommands.IfEquals(QueryDrag.DragMethodKey, DragMethod.Menu,
                                            ResultCommand.NoError,
                                            //This is defined in drop.
                                            DragDropScriptCommands.DetachAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}"))));
                    case "MouseMove":
                        return HubScriptCommands.IfDependencyProperty("{Sender}", AttachedProperties.IsDraggingProperty,
                                    ComparsionOperator.Equals, true,
                                    HubScriptCommands.SetRoutedEventHandled());
                }

            if (EnableDrop)
            {                               
                switch (eventId.Name)
                {                   
                    case "DragEnter": return
                    ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                        //Find DataContext that support IShellDrop
                        HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrop, "{ISupportDrop}", "{ElementSupportDrop}", false,
                            //And if there's one,
                            ScriptCommands.IfAssigned("{ISupportDrop}",                                
                                //Obtain DataObject from event and Call ISupportDrop.QueryDrop() to get IDraggables[] 
                                HubScriptCommands.QueryDraggablesAndDataObject("{ISupportDrop}", "{DataObj}", "{DragDrop.Draggables}", false,
                                //And if there's draggables,
                                ScriptCommands.IfAssigned("{DragDrop.Draggables}",
                                    //Call ISupportDrop.QueryDropEffects() to get QueryDropEffect.
                                    HubScriptCommands.QueryDropEffects("{ISupportDrop}", "{DragDrop.Draggables}", "{DataObj}", "{DragDrop.QueryDropResult}", false,
                                        //And Check if QueryDropEffect is None,
                                        ScriptCommands.IfEquals("{DragDrop.QueryDropResult}", QueryDropEffects.None,                                          
                                            //If no match effect, Detach adorner if attached.
                                            DragDropScriptCommands.DetachAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}"), 
                                            //Otherwise, 
                                            ScriptCommands.Assign(new Dictionary<string, object>()
                                            {
                                                { "{DragDrop.SupportDrop}", "{ISupportDrop}" }, //Store SupportDrop property to global for future use.
                                                { "{ISupportDrop.IsDraggingOver}", true }, //Set DataContext.IsDraggingOver to true.                                                
                                                { "{EventArgs.Handled}", true }  //Mark RoutedEvent handled.                                                                                           
                                            }, false,   
                                                //Attach DragAdorner and update it.                               
                                                DragDropScriptCommands.AttachAdorner(                                                    
                                                    "{DragDrop.AdornerLayer}", "{DragDrop.Adorner}", 
                                                    "{ISupportDrop}", "{DragDrop.DragMethod}", "{DragDrop.Draggables}")))))))));

                    case "GiveFeedback": return
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                            //If QueryDropResult returns none, set cursor to not droppable.
                            ScriptCommands.IfEquals("{DragDrop.QueryDropResult}", QueryDropEffects.None,
                                HubScriptCommands.SetCustomCursor(Cursors.No)));

                    case "DragOver": return ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                                        DragDropScriptCommands.UpdateAdornerPointerPosition("{DragDrop.Adorner}"));

                    case "DragLeave": return
                         ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                            //Detach adorner if DragLeave current element.
                            ScriptCommands.IfAssigned("{DragDrop.SupportDrop}",                                
                                    DragDropScriptCommands.DetachAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}")));

                    case "Drop": 
                        IScriptCommand detachAdornerAndResetDragDrop = 
                            DragDropScriptCommands.DetachAdorner("{DragDrop.AdornerLayer}", "{DragDrop.Adorner}", 
                                                DragDropScriptCommands.ResetDragDropVariables());

                        return
                        ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false,
                            //Find DataContext that support ISupportShellDrop
                            HubScriptCommands.AssignDataContext("{EventArgs.OriginalSource}", DataContextType.SupportShellDrop, "{ISupportDrop}", "{ElementSupportDrop}", false,
                                //If ISupportDrop found.
                                ScriptCommands.IfAssigned("{ISupportDrop}",                                                                
                                    ////Obtain DataObject from event and Call ISupportDrop.QueryDrop() to get IDraggables[] 
                                    HubScriptCommands.QueryDraggablesAndDataObject("{ISupportDrop}", "{DataObj}", "{DragDrop.Draggables}", false,
                                        //Call ISupportDrop.QueryDropEffects() to get QueryDropEffect.
                                        HubScriptCommands.QueryDropEffects("{ISupportDrop}", "{DragDrop.Draggables}", "{DataObj}", "{DragDrop.QueryDropResult}", false,
                                            ScriptCommands.IfEquals("{DragDrop.QueryDropResult}", QueryDropEffects.None,   
                                                //If QueryDropEffects is None, drag failed, detach adorner.
                                                detachAdornerAndResetDragDrop,                                                    
                                                //Otherwise, if DragMethod...
                                                ScriptCommands.IfEquals(QueryDrag.DragMethodKey, DragMethod.Menu,                                                     
                                                    //is Menu, then Show Menu.
                                                    ScriptCommands.Assign("{DragDrop.SupportDragBackup}", "{DragDrop.SupportDrag}", false, 
                                                        HubScriptCommands.ShowDragAdornerContextMenu("{DragDrop.Adorner}", 
                                                            "{DragDrop.QueryDropResult.SupportedEffects}", 
                                                            "{DragDrop.QueryDropResult.PreferredEffect}", 
                                                            "{ResultEffect}", 
                                                            //After menu closed...
                                                            ScriptCommands.IfEquals("{ResultEffect}", DragDropEffects.None, 
                                                                //If User choose None, detach and reset.
                                                                detachAdornerAndResetDragDrop, 
                                                                //Otherwise, call ISupportDrop.Drop()
                                                                HubScriptCommands.QueryDrop("{DragDrop.SupportDragBackup}", "{ISupportDrop}", "{DragDrop.Draggables}", "{DataObj}", 
                                                                    "{ResultEffect}", "{DragDrop.DropResult}", false,                                                                    
                                                                    //And detach adorner.
                                                                    detachAdornerAndResetDragDrop)))),
                                                    //otherwise, Call ISupportDrop.Drop() immediately.
                                                    HubScriptCommands.QueryDrop("{DragDrop.SupportDrag}", "{ISupportDrop}", "{DragDrop.Draggables}", "{DataObj}", 
                                                    "{DragDrop.QueryDropResult.PreferredEffect}", "{DragDrop.DropResult}", false, detachAdornerAndResetDragDrop)))))
                                )));

                }
            }

            return base.onEvent(eventId);
        }

        #region Public Properties

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

        #endregion
    }
}
