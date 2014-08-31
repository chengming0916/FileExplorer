using FileExplorer.UIEventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagramingDemo
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

        int Left { get; set; }
        int Top { get; set; }        
    }
}
