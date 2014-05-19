using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.Models;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    /// <summary>
    /// Owned by IProfile, for drag drop handling.
    /// </summary>
    public interface IDragDropHandler
    {

        DragDropResult QueryDrag(IEnumerable<IEntryModel> entries);

        void OnDragCompleted(IEnumerable<IEntryModel> entries, DragDropResult effect);

        bool QueryCanDrop(IEntryModel dest);
        QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropResult allowedEffects);
        DragDropResult OnDropCompleted(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropResult allowedEffects);

    }
}
