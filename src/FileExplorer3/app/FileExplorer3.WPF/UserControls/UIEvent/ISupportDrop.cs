using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF.ViewModels.Helpers
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
        QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects);
        IEnumerable<IDraggable> QueryDropDraggables(IDataObject da);
        DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects);
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

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            return new List<IDraggable>();
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            return DragDropEffects.None;
        }
    }
}
