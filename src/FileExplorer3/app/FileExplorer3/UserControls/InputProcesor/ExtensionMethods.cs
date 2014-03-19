using FileExplorer.BaseControls;
using FileExplorer.UserControls.InputProcesor;
using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FileExplorer
{
    public static partial class ExtensionMethods
    {

        /// <summary>
        /// Cursor position relative to inputElement.
        /// </summary>
        /// <param name="inputElement"></param>
        /// <returns></returns>
        public static Point PositionRelativeTo(this IUIInput input, IInputElement inputElement,
            Func<Point, Point> adjustFunc)
        {
            return adjustFunc(input.PositionRelativeTo(inputElement));
        }

        public static bool IsSameSource(this IUIInput input, IUIInput input2)
        {
            return input != null && input2 != null &&
                 input.Sender.Equals(input2.Sender);
        }

        private static Size getMiniumDragDistance(Defines.UIInputType inputType)
        {
            if (inputType == Defines.UIInputType.Touch)
                return new Size(5, 5);
            else return new Size(SystemParameters.MinimumHorizontalDragDistance,
                SystemParameters.MinimumVerticalDragDistance);
        }

        public static bool IsDragThresholdReached(this IUIInput input, IUIInput input2)
        {
            var minDragDist = getMiniumDragDistance(input.InputType);

            return
                input.IsSameSource(input2) && input.IsValidPositionForLisView(true) &&
                (
                     Math.Abs(input.Position.X - input2.Position.X) > minDragDist.Width ||
                     Math.Abs(input.Position.Y - input2.Position.Y) > minDragDist.Height
                );
        }

        public static bool IsWithin(this IUIInput input, IUIInput input2, int x, int y)
        {
            return
                input.IsSameSource(input2) && input.IsValidPositionForLisView(true) &&

                (
                     Math.Abs(input.Position.X - input2.Position.X) < x &&
                     Math.Abs(input.Position.Y - input2.Position.Y) < y
                )
                ;
        }

        public static bool IsDragThresholdReached(this TouchInput input, TouchInput input2)
        {
            return
                input.IsSameSource(input2) && input.IsValidPositionForLisView(true) &&

                (
                     Math.Abs(input.Position.X - input2.Position.X) < 10 &&
                     Math.Abs(input.Position.Y - input2.Position.Y) < 10
                )
                ;
        }

        public static void Update(this IEnumerable<IUIInputProcessor> processors, ref IUIInput input)
        {
            foreach (var p in processors)
                if (p.ProcessAllEvents || p.ProcessEvents.Contains(input.EventArgs.RoutedEvent))
                    p.Update(ref input);
        }

        public static bool IsValid(this IUIInput input)
        {
            return IsValidPositionForLisView(input, true);
        }

        public static bool IsValidPositionForLisView(this IUIInput input, bool validIfNotListView = false)
        {
            var sender = input.Sender as ListView;
            if (sender == null)
                return validIfNotListView;
            var originalSource = input.EventArgs.OriginalSource as DependencyObject;
            if (sender == null)
                return false;
            var scp = ControlUtils.GetScrollContentPresenter(sender);
            
            //Make sure return false for ContextMenu items 
            if (UITools.FindAncestor<ListView>(originalSource) == null)
                return false;
            if (scp == null ||
                //ListViewEx contains ContentBelowHeader, allow placing other controls in it, this is to avoid that)
                (!scp.Equals(UITools.FindAncestor<ScrollContentPresenter>(originalSource)) &&
                //This is for handling user click in empty area of a panel.
                 !(originalSource is ScrollViewer))
                )
                return false;


            bool isOverGridViewHeader = UITools.FindAncestor<GridViewColumnHeader>(originalSource) != null;
            bool isOverScrollBar = UITools.FindAncestor<ScrollBar>(originalSource) != null;
            if (isOverGridViewHeader || isOverScrollBar)
                return false;

            return true;
        }

    }
}
