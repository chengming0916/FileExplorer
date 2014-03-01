﻿using FileExplorer.BaseControls;
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

        public static bool IsDragThresholdReached(this IUIInput input, IUIInput input2)
        {
            return
                input.IsSameSource(input2) && input.IsValidPositionForLisView(true) &&
                (
                     Math.Abs(input.Position.X - input2.Position.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(input.Position.Y - input2.Position.Y) > SystemParameters.MinimumVerticalDragDistance
                );
        }

        public static void Update(this IEnumerable<IInputProcessor> processors, IUIInput input)
        {
            foreach (var p in processors)
                p.Update(input);
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