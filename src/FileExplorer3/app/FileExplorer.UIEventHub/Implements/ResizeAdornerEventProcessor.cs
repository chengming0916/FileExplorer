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

        private static IScriptCommand resizeNorthCommand = ScriptCommands.Subtract("{CanvasResize.ResizeItem.Height}", "{DiffY}", "{CanvasResize.ResizeItem.Height}",
                                         ScriptCommands.Add("{CanvasResize.ResizeItem.Top}", "{DiffY}", "{CanvasResize.ResizeItem.Top}")) ;
        private static IScriptCommand resizeSouthCommand = ScriptCommands.Add("{CanvasResize.ResizeItem.Height}", "{DiffY}", "{CanvasResize.ResizeItem.Height}");
        private static IScriptCommand resizeWestCommand = ScriptCommands.Subtract("{CanvasResize.ResizeItem.Width}", "{DiffX}", "{CanvasResize.ResizeItem.Width}",
                                                        ScriptCommands.Add("{CanvasResize.ResizeItem.Left}", "{DiffX}", "{CanvasResize.ResizeItem.Left}"));
        private static IScriptCommand resizeEastCommand = ScriptCommands.Add("{CanvasResize.ResizeItem.Width}", "{DiffX}", "{CanvasResize.ResizeItem.Width}");

        //   //ScriptCommands.Subtract("{CanvasResize.CurrentPosition.X}", "{CanvasResize.StartPosition.X}", "{CanvasResize.ResizeItemAdorner.OffsetX}", 
        //ScriptCommands.Subtract("{CanvasResize.CurrentPosition.Y}", "{CanvasResize.StartPosition.Y}", "{CanvasResize.ResizeItemAdorner.OffsetY}")                                                                         
        //private static IScriptCommand previewNorthCommand =
        //    ScriptCommands.Multiply<double>("{DiffY}", -1, "{DiffY1}",
        //    ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetY}", "{DiffY1}", false,
        //    ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetTop}", "{DiffY1}")));
        //private static IScriptCommand previewSouthCommand = ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetY}", "{DiffY}");
        //private static IScriptCommand previewWestCommand = ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetX}", "{DiffX}", false,
        //                                                    ScriptCommands.Multiply<double>("{DiffX}", 1, "{DiffX1}",
        //                                                    ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetLeft}", "{DiffX1}")));
        //private static IScriptCommand previewEastCommand = ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetX}", "{DiffX}");
        
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
                    return
                      ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,
                        ScriptCommands.IfTrue("{CanvasResize.IsResizing}",
                            HubScriptCommands.AssignCursorPosition(PositionRelativeToType.Panel, "{CanvasResize.CurrentPosition}", false,
                               HubScriptCommands.UpdateResizeItemAdorner("{CanvasResize.ResizeItemAdorner}",
                                 "{CanvasResize.ResizeMode}", "{CanvasResize.StartPosition}", "{CanvasResize.CurrentPosition}"))));
                              //ScriptCommands.Subtract("{CanvasResize.CurrentPosition.X}", "{CanvasResize.StartPosition.X}", "{DiffX}", 
                              //ScriptCommands.Subtract("{CanvasResize.CurrentPosition.Y}", "{CanvasResize.StartPosition.Y}", "{DiffY}",
                              //ScriptCommands.AbsoluteValue("{DiffX}", "{AbsDiffX}",
                              //ScriptCommands.AbsoluteValue("{DiffY}", "{DiffY}", 
                              // ScriptCommands.Switch<string>("{CanvasResize.ResizeMode}", 
                              //          new Dictionary<string,IScriptCommand>()
                              //          {
                              //             { "N" , previewNorthCommand },
                              //             { "NE", ScriptCommands.RunSequence(previewNorthCommand, previewEastCommand) },
                              //             { "E" , previewEastCommand }, 
                              //             { "SE", ScriptCommands.RunSequence(previewSouthCommand, previewEastCommand) },
                              //             { "S" , previewSouthCommand },
                              //             { "SW", ScriptCommands.RunSequence(previewSouthCommand, previewWestCommand) },
                              //             { "W" , previewWestCommand },                                           
                              //             { "NW", ScriptCommands.RunSequence(previewNorthCommand, previewWestCommand) },
                              //          }, 
                              //          ScriptCommands.PrintConsole("Not supported : {CanvasResize.ResizeMode}, {DiffX},{DiffY}")))))))));
                             
                      
                //case "MouseDrag":
                //case "TouchDrag":
                case "PreviewTouchDown":
                case "PreviewMouseDown":
                    return
                       ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,
                           HubScriptCommands.SetRoutedEventHandled(
                                ScriptCommands.Assign("{CanvasResize.IsResizing}", true, false,
                                   HubScriptCommands.AssignCursorPosition(PositionRelativeToType.Panel, "{CanvasResize.StartPosition}", false,                                        
                                        //Assign Source's Name (e.g. N, NW) to CanvasResize.ResizeMode.
                                        ScriptCommands.Assign("{CanvasResize.ResizeMode}", "{EventArgs.Source.Name}", false, 
                                            HubScriptCommands.CaptureMouse(CaptureMouseMode.UIElement))))));


                case "PreviewTouchUp":
                case "PreviewMouseUp":
                    return
                        ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetX}", 0, false, 
                        ScriptCommands.Assign("{CanvasResize.ResizeItemAdorner.OffsetY}", 0, false, 
                        ScriptCommands.AssignGlobalParameterDic("{CanvasResize}", false,
                            ScriptCommands.Assign("{CanvasResize.IsResizing}", false, false,
                                HubScriptCommands.SetRoutedEventHandled(
                                    HubScriptCommands.CaptureMouse(CaptureMouseMode.Release,
                                      ScriptCommands.Subtract("{CanvasResize.CurrentPosition.X}", "{CanvasResize.StartPosition.X}", "{DiffX}", 
                                      ScriptCommands.Subtract("{CanvasResize.CurrentPosition.Y}", "{CanvasResize.StartPosition.Y}", "{DiffY}",                                       
                                      ScriptCommands.Switch<string>("{CanvasResize.ResizeMode}", 
                                        new Dictionary<string,IScriptCommand>()
                                        {
                                           { "N" , resizeNorthCommand },
                                           { "NE", ScriptCommands.RunSequence(resizeNorthCommand, resizeEastCommand) },
                                           { "E" , resizeEastCommand }, 
                                           { "SE", ScriptCommands.RunSequence(resizeSouthCommand, resizeEastCommand) },
                                           { "S" , resizeSouthCommand },
                                           { "SW", ScriptCommands.RunSequence(resizeSouthCommand, resizeWestCommand) },
                                           { "W" , resizeWestCommand },                                           
                                           { "NW", ScriptCommands.RunSequence(resizeNorthCommand, resizeWestCommand) },
                                        }, 
                                        ScriptCommands.PrintConsole("Not supported : {CanvasResize.ResizeMode}, {DiffX},{DiffY}"))))))))));
                default:
                    return ScriptCommands.PrintConsole(eventId.Name);
            }


            return base.onEvent(eventId);
        }
    }
}
