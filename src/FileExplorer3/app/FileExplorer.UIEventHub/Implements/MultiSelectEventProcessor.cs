using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.UIEventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.WPF.BaseControls
{

    public class MultiSelectEventProcessor : UIEventProcessorBase
    {
        public MultiSelectEventProcessor()
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
                    return HubScriptCommands.ObtainPointerPosition(
                        HubScriptCommands.SetHandledIfNotFocused());
                case "MouseDrag":
                case "TouchDrag":
                    if (EnableMultiSelect)
                        return 
                           
                            HubScriptCommands.IfNotRoutedEventHandled(
                            HubScriptCommands.CaptureMouse(CaptureMouseMode.ScrollContentPresenter,
                              HubScriptCommands.SetDependencyPropertyIfDifferent("{Sender}",
                                AttachedProperties.IsSelectingProperty, true,
                                    HubScriptCommands.ObtainPointerPosition(
                                      HubScriptCommands.AttachSelectionAdorner("{SelectionAdorner}",
                                        HubScriptCommands.FindSelectedItems(
                                            HubScriptCommands.HighlightItems()))),
                                ResultCommand.NoError)));
                    break;
                case "TouchMove":
                case "MouseMove":
                    return
                         HubScriptCommands.ThrottleTouchDrag(5, 
                        HubScriptCommands.IfDependencyPropertyEquals("{Sender}",
                        AttachedProperties.IsSelectingProperty, true,
                        HubScriptCommands.ObtainPointerPosition(
                            HubScriptCommands.UpdateSelectionAdorner("{SelectionAdorner}",
                                        HubScriptCommands.FindSelectedItems(
                                            HubScriptCommands.HighlightItems(
                                                HubScriptCommands.AutoScroll()))))));
                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return HubScriptCommands.IfNotRoutedEventHandled(
                       HubScriptCommands.ReleaseMouse(
                          HubScriptCommands.ObtainPointerPosition(
                            HubScriptCommands.DettachSelectionAdorner("{SelectionAdorner}",
                            //Assign IsSelecting attached property to {WasSelecting}
                            HubScriptCommands.GetDependencyProperty("{Sender}", AttachedProperties.IsSelectingProperty, "{WasSelecting}",
                                //Reset IsSelecting attached property.
                                HubScriptCommands.SetDependencyPropertyTyped("{Sender}", AttachedProperties.IsSelectingProperty, false,
                                ScriptCommands.IfTrue("{WasSelecting}",
                                    //WasSelecting
                                    HubScriptCommands.FindSelectedItems(HubScriptCommands.SelectItems()),
                                    //WasNotSelecting
                                    HubScriptCommands.AssignItemUnderMouse("{ItemUnderMouse}", false,
                                      ScriptCommands.IfAssigned("{ItemUnderMouse}",
                                        //If an Item Under Mouse
                                        ScriptCommands.RunICommand(UnselectAllCommand, null, false,
                                            ScriptCommands.SetProperty("{ItemUnderMouse}", "IsSelected", true)),
                                        //If click on empty area
                                        ScriptCommands.RunICommand(UnselectAllCommand, null, false))
                                      ))))))));
            }

            return base.onEvent(eventId);
        }

        public static DependencyProperty UnselectAllCommandProperty =
            DependencyProperty.Register("UnselectAllCommand", typeof(ICommand),
            typeof(MultiSelectEventProcessor));

        public ICommand UnselectAllCommand
        {
            get { return (ICommand)GetValue(UnselectAllCommandProperty); }
            set { SetValue(UnselectAllCommandProperty, value); }
        }

        public static DependencyProperty EnableMultiSelectProperty =
        DependencyProperty.Register("EnableMultiSelect", typeof(bool),
        typeof(MultiSelectEventProcessor), new PropertyMetadata(true));

        public bool EnableMultiSelect
        {
            get { return (bool)GetValue(EnableMultiSelectProperty); }
            set { SetValue(EnableMultiSelectProperty, value); }
        }

        public static DependencyProperty IsCheckboxEnabledProperty =
        DependencyProperty.Register("IsCheckboxEnabled", typeof(bool),
        typeof(MultiSelectEventProcessor), new PropertyMetadata(true));

        public bool IsCheckboxEnabled
        {
            get { return (bool)GetValue(IsCheckboxEnabledProperty); }
            set { SetValue(IsCheckboxEnabledProperty, value); }
        }

    }
}
