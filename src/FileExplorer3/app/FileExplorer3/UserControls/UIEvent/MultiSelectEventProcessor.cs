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
                    FrameworkElement.MouseUpEvent,
                    FrameworkElement.MouseMoveEvent
                }
            );
        }

        public override Cofe.Core.Script.IScriptCommand OnEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "PreviewMouseDown": return new SetHandledIfNotFocused();
                case "MouseDrag": return new BeginSelect() { UnselectCommand = UnselectAllCommand };
                case "MouseMove": return new ContinueSelect();
                case "MouseUp": return new EndSelect() { UnselectCommand = UnselectAllCommand };
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
    }
}
