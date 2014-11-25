using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Models;
using FileExplorer.Defines;
using System.Windows;

namespace FileExplorer.WPF.Models
{
    public class NullDragDropHandler : IDragDropHandler
    {            
        public DragDropEffectsEx QueryDrag(IEnumerable<IEntryModel> entries)
        {
            return DragDropEffectsEx.None;
        }

        public void OnDragCompleted(IEnumerable<IEntryModel> entries, DragDropEffectsEx effect)
        {            
        }

        public bool QueryCanDrop(IEntryModel dest)
        {
            return false;
        }

        public QueryDropEffects QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            return QueryDropEffects.None;
        }

        public DragDropEffectsEx OnDropCompleted(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            return DragDropEffectsEx.None;
        }
    }

    public class NullShellDragDropHandler : IShellDragDropHandler
    {
        public Task<IDataObject> GetDataObject(IEnumerable<IEntryModel> entries)
        {
            return null;
        }

        public IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject)
        {
            yield break;
        }

        public DragDropEffectsEx QueryDrag(IEnumerable<IEntryModel> entries)
        {
            return DragDropEffectsEx.None;
        }

        public void OnDragCompleted(IEnumerable<IEntryModel> entries, DragDropEffectsEx effect)
        {
        }

        public bool QueryCanDrop(IEntryModel dest)
        {
            return false;
        }

        public QueryDropEffects QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            return QueryDropEffects.None;
        }

        public DragDropEffectsEx OnDropCompleted(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            return DragDropEffectsEx.None;
        }



        public DragDropEffectsEx OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel dest, DragDropEffectsEx allowedEffects)
        {
            return DragDropEffectsEx.None;
        }


        public void OnDragCompleted(IEnumerable<IEntryModel> entries, IDataObject da, DragDropEffectsEx effect)
        {
            
        }
    }
}
