using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{

   

    public interface ISupportShellDrag : ISupportDrag
    {
        IDataObject GetDataObject(IDraggable[] draggables);
        void OnDragCompleted(IDraggable[] draggables, IDataObject da, DragDropEffects effect);
    }
    
}
