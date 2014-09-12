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


            //Console.WriteLine(eventId.Name);
            switch (eventId.Name)
            {

                //case "PreviewTouchDown":
                //case "PreviewMouseDown":
                //    return 
                //        ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,                         
                //            HubScriptCommands.SetRoutedEventHandled(
                //                ScriptCommands.Assign("{CanvasResize.IsResizing}", true, false, 
                //                    HubScriptCommands.AssignCursorPosition(PositionRelativeToType.Null, "{CanvasResize.StartPosition}", false, 
                //                        HubScriptCommands.CaptureMouse(CaptureMouseMode.UIElement)))));

                case "MouseMove":
                case "TouchMove":
                    return ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,
                        ScriptCommands.IfTrue("{CanvasResize.IsResizing}", 
                                    HubScriptCommands.AssignCursorPosition(PositionRelativeToType.Null, 
                                    "{CanvasResize.CurrentPosition}", false)));
                //case "MouseDrag":
                //case "TouchDrag":
                case "PreviewTouchDown":
                case "PreviewMouseDown":
                    return
                       ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,
                           HubScriptCommands.SetRoutedEventHandled(
                                ScriptCommands.Assign("{CanvasResize.IsResizing}", true, false,
                                   HubScriptCommands.AssignCursorPosition(PositionRelativeToType.Null, "{CanvasResize.StartPosition}", false,
                                        //Assign Source's Name (e.g. N, NW) to CanvasResize.Mode.
                                        ScriptCommands.Assign("{CanvasResize.Mode}", "{EventArgs.Source.Name}", false, 
                                            HubScriptCommands.CaptureMouse(CaptureMouseMode.UIElement))))));


                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return
                        ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,
                            ScriptCommands.Assign("{CanvasResize.IsResizing}", false, false,
                                HubScriptCommands.SetRoutedEventHandled(
                                    HubScriptCommands.CaptureMouse(CaptureMouseMode.Release,
                                      ScriptCommands.Subtract("{CanvasResize.CurrentPosition.X}", "{CanvasResize.StartPosition.X}", "{DiffX}", 
                                      ScriptCommands.Subtract("{CanvasResize.CurrentPosition.Y}", "{CanvasResize.StartPosition.Y}", "{DiffY}",   
                                      ScriptCommands.PrintConsole("{CanvasResize.Mode}, {DiffX},{DiffY}")))))));
                default:
                    return ScriptCommands.PrintConsole(eventId.Name);
            }


            return base.onEvent(eventId);
        }
    }
}
