using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IDragDropHandler
    {        
        DragDropEffectsEx QueryDrag(IEnumerable<IEntryModel> entries);
        void OnDragCompleted(IEnumerable<IEntryModel> entries, DragDropEffectsEx effect);

        bool QueryCanDrop(IEntryModel dest);
        QueryDropEffects QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffectsEx allowedEffects);
        DragDropEffectsEx OnDropCompleted(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffectsEx allowedEffects);

    }
}
