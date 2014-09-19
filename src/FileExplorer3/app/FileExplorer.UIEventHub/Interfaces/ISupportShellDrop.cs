using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{
    
    public interface ISupportShellDrop : ISupportDrop
    {        
        IEnumerable<IDraggable> QueryDropDraggables(IDataObject da);
        DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects);
    }

}
