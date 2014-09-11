using FileExplorer.Script;
using FileExplorer.UIEventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace FileExplorer.WPF.BaseControls
{
    public class ResizeAdornerEventProcessor : UIEventProcessorBase
    {
        public ResizeAdornerEventProcessor()
        {
            _processEvents.AddRange(
               new[] {                    
                   FrameworkElement.PreviewMouseDownEvent,
                    FrameworkElement.PreviewTouchDownEvent,
                   FrameworkElement.MouseMoveEvent,
                    FrameworkElement.TouchMoveEvent,
                    UIEventHub.MouseDragEvent,
                    UIEventHub.TouchDragEvent,
                    FrameworkElement.PreviewMouseUpEvent,
                    FrameworkElement.PreviewTouchUpEvent
                }
            );

            Print.PrintConsoleAction = msg => Console.WriteLine(msg);
        }

        protected override Script.IScriptCommand onEvent(RoutedEvent eventId)
        {
            

            //if (eventId.Name.Contains("Move"))
            //    return ResultCommand.NoError;
            //return ScriptCommands.PrintConsole(eventId.Name);
            switch (eventId.Name)
            {

                case "PreviewTouchDown":
                case "PreviewMouseDown":
                    return 
                        HubScriptCommands.SetRoutedEventHandled(
                        HubScriptCommands.CaptureMouse(CaptureMouseMode.UIElement, ScriptCommands.PrintConsole(eventId.Name)));

                //case "MouseDrag":
                //case "TouchDrag":
                //    return Scriptcomm

                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return 
                        HubScriptCommands.SetRoutedEventHandled(
                        HubScriptCommands.CaptureMouse(CaptureMouseMode.Release, ScriptCommands.PrintConsole(eventId.Name)));
            }
            return base.onEvent(eventId);
        }
    }
}
