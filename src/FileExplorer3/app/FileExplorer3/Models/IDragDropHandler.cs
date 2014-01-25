﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Models
{
    /// <summary>
    /// Owned by IProfile, for drag drop handling.
    /// </summary>
    public interface IDragDropHandler
    {
        IDataObject GetDataObject(IEnumerable<IEntryModel> entries);

        DragDropEffects QueryDrag(IEnumerable<IEntryModel> entries);

        void OnDragCompleted(IEnumerable<IEntryModel> entries, IDataObject da, DragDropEffects effect);

        IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject);

        bool QueryCanDrop(IEntryModel dest);
        QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffects allowedEffects);
        DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel dest, DragDropEffects allowedEffects);

    }
}