using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.UIEventHub
{

    public static partial class HubScriptCommands
    {

        public static IScriptCommand IfMouseGesture(string MouseGestureVariable = "{MouseGesture}",
            IScriptCommand nextCommand = null, IScriptCommand otherwiseCommand = null)
        {
            return new IfMouseGesture()
            {
                MouseGestureKey = MouseGestureVariable,
                NextCommand = (ScriptCommandBase)nextCommand,
                OtherwiseCommand = (ScriptCommandBase)otherwiseCommand
            };
        }

        public static IScriptCommand IfMouseGesture(MouseGesture gesture,
            IScriptCommand nextCommand = null, IScriptCommand otherwiseCommand = null)
        {
            string MouseGestureVariable = "{IfMouseGesture-Gesture}";
            return
                ScriptCommands.Assign(MouseGestureVariable, gesture, false,
                HubScriptCommands.IfMouseGesture(MouseGestureVariable, nextCommand, otherwiseCommand));
        }
    }
    public class IfMouseGesture : UIScriptCommandBase
    {
        /// <summary>
        /// Point to a key gesture (MouseGesture) or string based gesture (e.g. Ctrl+C), 
        /// Default=Ctrl+C
        /// </summary>
        public string MouseGestureKey { get; set; }

        public ScriptCommandBase OtherwiseCommand { get; set; }

        public IfMouseGesture()
            : base("IfKeyPressed")
        {
            MouseGestureKey = "{EventArgs.Key}";
        }

        private static MouseGestureConverter converter = new MouseGestureConverter();

        public override IScriptCommand Execute(ParameterDic pm)
        {
            MouseEventArgs mouseEvent = pm.GetValue<MouseEventArgs>(RoutedEventArgsKey);            
            UIElement sender  = pm.GetValue<UIElement>(SenderKey);
            if (mouseEvent == null)
                return ResultCommand.Error(new ArgumentException("EventArgs"));

            object MouseGestureObject = !MouseGestureKey.StartsWith("{") ? MouseGestureKey : pm.GetValue(MouseGestureKey);
            MouseGesture gesture = null;

            if (MouseGestureObject is MouseGesture)
                gesture = (MouseGesture)MouseGestureObject;

            if (MouseGestureObject is string)
                gesture = (MouseGesture)converter.ConvertFrom(MouseGestureObject);


            return gesture != null && 
                gesture.Modifiers == Keyboard.Modifiers && gesture.MouseAction == GetMouseAction(mouseEvent) ?                        
                NextCommand : OtherwiseCommand;
        }


        internal static MouseAction GetMouseAction(InputEventArgs inputArgs)
        {
            MouseAction MouseAction = MouseAction.None;

            MouseEventArgs mouseArgs = inputArgs as MouseEventArgs;
            if (mouseArgs != null)
            {
                if (inputArgs is MouseWheelEventArgs)
                {
                    MouseAction = MouseAction.WheelClick;
                }
                else if (mouseArgs is MouseButtonEventArgs)
                {
                    MouseButtonEventArgs args = inputArgs as MouseButtonEventArgs;

                    switch (args.ChangedButton)
                    {
                        case MouseButton.Left:
                            {
                                if (args.ClickCount == 2)
                                    MouseAction = MouseAction.LeftDoubleClick;
                                else if (args.ClickCount == 1)
                                    MouseAction = MouseAction.LeftClick;
                            }
                            break;

                        case MouseButton.Right:
                            {
                                if (args.ClickCount == 2)
                                    MouseAction = MouseAction.RightDoubleClick;
                                else if (args.ClickCount == 1)
                                    MouseAction = MouseAction.RightClick;
                            }
                            break;

                        case MouseButton.Middle:
                            {
                                if (args.ClickCount == 2)
                                    MouseAction = MouseAction.MiddleDoubleClick;
                                else if (args.ClickCount == 1)
                                    MouseAction = MouseAction.MiddleClick;
                            }
                            break;
                    }
                }
            }
            return MouseAction;
        }
 
    }


}
