using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.ViewModels.Helpers
{
    public interface IDraggable
    {
        bool IsSelected { get; }
        
    }

    //public interface IShellDraggable : IDraggable
    //{
        
    //}

    //public interface ITypedDraggable<T> : IDraggable
    //{
    //    Type[] SupportedTypes { get; }
    //    IEnumerable<T> GetDataObject<T>();
    //}

    public interface ISupportDrag
    {        
        bool HasDraggables { get; }
        IEnumerable<IDraggable> GetDraggables();
        DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables);
        IDataObject GetDataObject(IEnumerable<IDraggable> draggables);        
        void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect);
    }

    public class QueryDropResult
    {
        public QueryDropResult(DragDropEffects supportedEffects, DragDropEffects preferredEffects)
        {
            SupportedEffects = supportedEffects;
            PreferredEffect = preferredEffects;
        }

        public static QueryDropResult None = new QueryDropResult(DragDropEffects.None, DragDropEffects.None);            

        public static QueryDropResult CreateNew(DragDropEffects supportedEffects, DragDropEffects preferredEffects)
        {
            return new QueryDropResult(supportedEffects, preferredEffects);
        }

         public static QueryDropResult CreateNew(DragDropEffects supportedEffects)
        {
            return new QueryDropResult(supportedEffects, supportedEffects);
        }


        public DragDropEffects SupportedEffects { get; set; }
        public DragDropEffects PreferredEffect { get; set; }
    }

    public interface ISupportDrop
    {
        bool IsDroppable { get; }
        QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects);
        IEnumerable<IDraggable> QueryDropDraggables(IDataObject da);
        DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects);
    }
}
