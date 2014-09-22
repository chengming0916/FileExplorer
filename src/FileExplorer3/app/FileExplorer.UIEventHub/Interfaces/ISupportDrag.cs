using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{

    public interface ISupportDragHelper
    {
        ISupportDrag DragHelper { get; }
    }

    public interface ISupportDrag
    {        
        bool HasDraggables { get; }
        IDraggable[] GetDraggables();
        DragDropEffects QueryDrag(IDraggable[] draggables);
        void OnDragCompleted(IDraggable[] draggables, DragDropEffects effect);
    }

    public class NullSupportDrag : ISupportDrag
    {
        public static NullSupportDrag Instance = new NullSupportDrag();
        public bool HasDraggables { get { return false; } }

        public IDraggable[] GetDraggables()
        {
            return new List<IDraggable>().ToArray();
        }

        public DragDropEffects QueryDrag(IDraggable[] draggables)
        {
            return DragDropEffects.None;
        }

        public IDataObject GetDataObject(IDraggable[] draggables)
        {
            return null;
        }

        public void OnDragCompleted(IDraggable[] draggables, DragDropEffects effect)
        {
           
        }
    }
}
