using FileExplorer.Script;
using FileExplorer.UIEventHub;
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
                    return ScriptCommands.FilterArray("{Sender.Items}", "IsSelected", ComparsionOperator.Equals, true, "{SelectedItems}", 
                        ScriptCommands.IfArrayLength(ComparsionOperator.Equals, "{SelectedItems}", 1,
                            HubScriptCommands.AttachResizeItemAdorner("{ResizeItemAdorner}", 
                                HubScriptCommands.UpdateResizeItemAdorner("{ResizeItemAdorner}", "{SelectedItems[0]}")), 
                            HubScriptCommands.DettachResizeItemAdorner("{ResizeItemAdorner}")
                        ));
            }
            return base.onEvent(eventId);
        }
    }
}
