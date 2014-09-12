using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        public static IScriptCommand AssignCursorPosition(PositionRelativeToType positionRelativeToType,
            string destinationVariable = "{Position}", bool skipIfExists = false,
            IScriptCommand nextCommand = null)
        {
            return new AssignCursorPosition()
            {
                PositionRelativeTo = positionRelativeToType,
                DestinationKey = destinationVariable,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)nextCommand                
            };
        }
    }


    public enum PositionRelativeToType
    {
        Null,        
        Sender
    }

    /// <summary>
    /// Serializable, Assign position of cursor (mouse/stylus/touch) to a variable (Point).
    /// </summary>
    public class AssignCursorPosition : UIScriptCommandBase<UIElement, RoutedEventArgs>
    {
        /// <summary>
        /// Calculate fixed position or relative position based on type.
        /// </summary>
        public PositionRelativeToType PositionRelativeTo { get; set; }

        /// <summary>
        /// Point to where to store the cursor position. Default = {Position}
        /// </summary>
        public string DestinationKey { get; set; }


        public bool SkipIfExists { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<AssignCursorPosition>();

        public AssignCursorPosition()
            : base("AssignCursorPosition")
        {
            PositionRelativeTo = PositionRelativeToType.Null;
            DestinationKey = "{Position}";            
            SkipIfExists = false;
        }


        protected override IScriptCommand executeInner(ParameterDic pm, UIElement sender, RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            Point position = input.Position;
            switch (PositionRelativeTo)
            {
                case PositionRelativeToType.Sender :
                    position = input.PositionRelativeTo(sender);
                    break;
            }
            //Console.WriteLine(position);
            return ScriptCommands.Assign(DestinationKey, position, SkipIfExists, NextCommand);
        }


    }
}
