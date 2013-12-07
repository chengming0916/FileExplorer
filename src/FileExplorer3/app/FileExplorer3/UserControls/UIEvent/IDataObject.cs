﻿using System;
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
        Tuple<IDataObject, DragDropEffects> GetDataObject();

        void OnDataObjectDropped(IDataObject da, DragDropEffects effect);
    }

    public interface ISupportDrop
    {
        DragDropEffects QueryDrop(IDataObject da);
        IEnumerable<IDraggable> QueryDropDraggables(IDataObject da);
        DragDropEffects Drop(IDataObject da, DragDropEffects allowedEffects);
    }
}
