using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.ViewModels.Helpers
{

    public interface ISupportDragHelper
    {
        ISupportDrag DragHelper { get; }
    }

    public interface ISupportDrag
    {        
        bool HasDraggables { get; }
        IEnumerable<IDraggable> GetDraggables();
        DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables);
        IDataObject GetDataObject(IEnumerable<IDraggable> draggables);        
        void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect);
    }
}
