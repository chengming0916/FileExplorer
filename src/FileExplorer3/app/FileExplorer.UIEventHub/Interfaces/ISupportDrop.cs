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
        bool IsDraggingOver { get;  set; }
        bool IsDroppable { get; }
        string DropTargetLabel { get; }
        QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects);
        DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects);
    }

    public class NullSupportDrop : ISupportDrop
    {
        public static ISupportDrop Instance = new NullSupportDrop();
        public bool IsDraggingOver { get; set; }
        public bool IsDroppable { get { return false; }}
        public string DropTargetLabel { get { return null; }}
        public QueryDropEffects QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            return QueryDropEffects.None;
        }


        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return DragDropEffects.None;
        }


        public QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return QueryDropEffects.None;
        }
    }
}
