using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Models;
using FileExplorer.Defines;

namespace FileExplorer.WPF.Models
{
    public class NullDragDropHandler : IDragDropHandler
    {
        public virtual async Task<IDataObject> GetDataObject(IEnumerable<IEntryModel> entries)
        {
            return null;
        }

        public virtual DragDropEffects QueryDrag(IEnumerable<IEntryModel> entries)
        {
            return DragDropEffects.None;
        }

        public virtual void OnDragCompleted(IEnumerable<IEntryModel> entries, DragDropEffects effect)
        {
        }

        public virtual IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject)
        {
            yield break;
        }

        public virtual bool QueryCanDrop(IEntryModel dest)
        {
            return false;
        }

        public virtual QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffects allowedEffects)
        {
            return QueryDropResult.None;
        }

        public virtual DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IEntryModel dest,
            DragDropEffects allowedEffects)
        {
            return DragDropEffects.None;
        }
    }
}
