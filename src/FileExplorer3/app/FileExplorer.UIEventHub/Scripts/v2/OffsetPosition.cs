using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        public static IScriptCommand OffsetPosition(string positionVariable = "{Position}", string offsetVariable = "{Offset}", 
            string destinationVariable = "{Position}", IScriptCommand nextCommand = null)
        {
            return new OffsetPosition()
            {
                PositionKey = positionVariable,
                OffsetKey = offsetVariable,
                DestinationKey = destinationVariable,
                NextCommand =
                    (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand OffsetPositionValue(string positionVariable = "{Position}", Point offset = default(Point),
           string destinationVariable = "{Position}", IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign("{OffsetPositionValue}", offset, false,
                OffsetPosition(positionVariable, "{OffsetPositionValue}", destinationVariable));
        }
    }

    public class OffsetPosition : ScriptCommandBase
    {
        /// <summary>
        /// Position (Point) to process, Default = {Position}.
        /// </summary>
        public string PositionKey { get; set; }

        /// <summary>
        /// Offset position (Point), Default = {Offset}.
        /// </summary>
        public string OffsetKey { get; set; }

        /// <summary>
        /// Store result to (Point), Default = {Position}.
        /// </summary>
        public string DestinationKey { get; set; }


        public OffsetPosition() : base("OffsetPosition")
        {
            PositionKey = "{Position}";
            OffsetKey = "{Offset}";
            DestinationKey = "{Position}";
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            Point pos = pm.GetValue<Point>(PositionKey);
            Point offsetpos = pm.GetValue<Point>(OffsetKey);

            pm.SetValue(DestinationKey, new Point(pos.X + offsetpos.X, pos.Y + offsetpos.Y));

            return NextCommand;
        }
    }
}
