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
        /// <summary>
        /// Serializable, Set cursor during UIElement.GiveFeedback event.
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand SetCustomCursor(Cursor cursor, IScriptCommand nextCommand = null)
        {
            return new SetCustomCursor()
            {
                CursorType = cursor,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }

    /// <summary>
    /// Serializable, Set cursor during UIElement.GiveFeedback event.
    /// </summary>
    public class SetCustomCursor : UIScriptCommandBase<UIElement, GiveFeedbackEventArgs>
    {
        public Cursor CursorType { get; set; }

        public SetCustomCursor()
            : base("SetCustomCursor")
        {

        }

        protected override IScriptCommand executeInner(ParameterDic pm, UIElement sender, GiveFeedbackEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            if (CursorType != null)
            {
                evnt.UseDefaultCursors = false;
                Mouse.SetCursor(CursorType);
                evnt.Handled = true;
            }
            else evnt.UseDefaultCursors = true;

            return NextCommand;
        }
    }
}
