using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF.BaseControls
{
    public class ResizeAdornerEventProcessor : UIEventProcessorBase
    {
        public ResizeAdornerEventProcessor()
        {
            _processEvents.AddRange(
               new[] {
                    FrameworkElement.PreviewMouseUpEvent,
                    FrameworkElement.PreviewTouchUpEvent
                }
            );
        }

        protected override Script.IScriptCommand onEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return ScriptCommands.ForEachIfAnyValue<bool>("{Sender.Items}", "IsSelected", ComparsionOperator.Equals, true, 
                        ScriptCommands.PrintDebug("Selected"));
            }
            return base.onEvent(eventId);
        }
    }
}
