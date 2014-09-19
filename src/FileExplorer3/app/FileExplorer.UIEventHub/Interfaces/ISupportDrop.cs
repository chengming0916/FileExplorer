using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{

    public interface ISupportDropHelper
    {
        ISupportDrop DropHelper { get; }
    }

    public interface ISupportDrop
    {
        bool IsDraggingOver { set; }
        bool IsDroppable { get; }
        string DropTargetLabel { get; }
        QueryDropResult QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects);        
        DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects);
    }

    public class NullSupportDrop : ISupportDrop
    {
        public static ISupportDrop Instance = new NullSupportDrop();
        public bool IsDraggingOver { set { } }
        public bool IsDroppable { get { return false; }}
        public string DropTargetLabel { get { return null; }}
        public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            return QueryDropResult.None;
        }


        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return DragDropEffects.None;
        }


        public QueryDropResult QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return QueryDropResult.None;
        }
    }
}
