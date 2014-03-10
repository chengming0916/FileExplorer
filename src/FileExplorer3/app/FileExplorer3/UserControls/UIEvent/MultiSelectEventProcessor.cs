using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FileExplorer.BaseControls.MultiSelect;

namespace FileExplorer.BaseControls
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

        public override Cofe.Core.Script.IScriptCommand OnEvent(RoutedEvent eventId)
        {

            switch (eventId.Name)
            {
                case "PreviewTouchDown": 
                case "PreviewMouseDown": 
                    return new ObtainPointerPosition(new SetHandledIfNotFocused());
                case "MouseDrag":
                case "TouchDrag":
                    if (EnableMultiSelect)
                        return new BeginSelect() { UnselectCommand = UnselectAllCommand }; break;
                case "TouchMove": 
                case "MouseMove": 
                    return new ContinueSelect();
                case "PreviewTouchUp": 
                case "PreviewMouseUp": return new EndSelect() 
                    { UnselectCommand = UnselectAllCommand, IsCheckBoxEnabled = IsCheckboxEnabled };
            }

            return base.OnEvent(eventId);
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
