using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{
    public interface IDraggablePositionAware : IDraggable
    {
        /// <summary>
        /// toggle hidden when drag leave.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// If MouseDown and not IsSelected, then IsSelected = true.
        /// If MouseDown and IsSelected, then Drag.
        /// </summary>
        bool IsSelected { get; set; }

        Point Position { get; set; }
        Point OriginalPosition { get; set; }
    }
}
