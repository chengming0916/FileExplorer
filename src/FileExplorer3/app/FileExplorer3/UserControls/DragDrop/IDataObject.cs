using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UserControls.DragDrop
{
    public interface IDraggable
    {
        DragDropEffects SupportedEffects { get; }
        DataObject GetDataObject();
    }

    public interface IShellDraggable : IDraggable
    {
        
    }

    public interface ITypedDraggable<T> : IDraggable
    {
        Type[] SupportedTypes { get; }
        IEnumerable<T> GetDataObject<T>();
    }

    public interface ISupportDrag
    {
        bool HasDraggables { get; }
        Task<IDraggable> GetDraggables();
    }

    public interface ISupportDrop
    {
        DragDropEffects QueryDrop(IDraggable draggable);
        bool Drop(IDraggable draggable);
    }
}
