﻿using FileExplorer;
using FileExplorer.Script;
using FileExplorer.UIEventHub;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Test_ShellDragDemo
{
    public class FileListViewModel : NotifyPropertyChanged, ISupportShellDrag, ISupportShellDrop, IContainer<ISelectable>
    {
        private ObservableCollection<FileViewModel> _items = new ObservableCollection<FileViewModel>();
        private bool _isDraggingOver;

        public ObservableCollection<FileViewModel> Items { get { return _items; } }
        public dynamic Commands { get; private set; }

        public FileListViewModel(string label)
        {
            DropTargetLabel = label;
            Commands = new DynamicRelayCommandDictionary()
            {
                ParameterDicConverter = ParameterDicConverters.FromParameterDic(
                new ParameterDic()
                {
                    { "FileListVM", this }
                })
            };

            Commands.UnselectAll = new SimpleScriptCommand("UnselectAll",
               pd =>
               {
                   pd.GetValue<FileListViewModel>("{FileListVM}").UnselectAll();
                   return ResultCommand.NoError;
               });

        }


        public void UnselectAll()
        {
            foreach (var item in Items)
                item.IsSelected = false;
        }


        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            var da = new DataObject();
            var fList = new StringCollection();
            foreach (var d in draggables.Cast<FileViewModel>())
            {
                if (!File.Exists(d.FileName))
                    using (var sw = File.CreateText(d.FileName))
                        sw.WriteLine(d.DisplayName);
                fList.Add(d.FileName);
            }
            da.SetFileDropList(fList);
            return da;
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, System.Windows.DragDropEffects effect)
        {
            OnDragCompleted(draggables, effect);
        }

        public bool HasDraggables
        {
            get { return Items.Any(f => f.IsSelected); }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return Items.Where(f => f.IsSelected);
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return DragDropEffects.Copy;
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, System.Windows.DragDropEffects effect)
        {
            if (effect == DragDropEffects.Move)
                foreach (var f in draggables.Cast<FileViewModel>())
                    if (Items.Contains(f))
                        Items.Remove(f);
        }

        IEnumerable<ISelectable> IContainer<ISelectable>.GetChildItems()
        {
            return Items;
        }

        #region ISupportShellDrop

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject dataObj)
        {
            DataObject da = dataObj as DataObject;
            if (da.ContainsFileDropList())
            {
                foreach (var file in da.GetFileDropList())
                {
                    FileViewModel fvm = new FileViewModel(file);                    
                        yield return fvm;
                }
            }
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            foreach (var fvm in draggables.Cast<FileViewModel>())
                this.Items.Add(fvm);
            if (allowedEffects.HasFlag(DragDropEffects.Move))
                return DragDropEffects.Move;
            if (allowedEffects.HasFlag(DragDropEffects.Copy))
                return DragDropEffects.Copy;
            return DragDropEffects.Link;
        }

        public bool IsDraggingOver
        {
            get { return _isDraggingOver; }
            set { _isDraggingOver = value; NotifyOfPropertyChanged(() => IsDraggingOver); }
        }

        public bool IsDroppable
        {
            get { return true; ; }
        }

        public string DropTargetLabel
        {
            get;
            set;
        }

        public QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            if (draggables.Any(d => Items.Contains(d)))
                return QueryDropEffects.None;

            return QueryDropEffects.CreateNew(DragDropEffects.Link | DragDropEffects.Move, DragDropEffects.Move);
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            if (draggables.Any(d => Items.Contains(d)))
                return DragDropEffects.None;

            return Drop(draggables, null, allowedEffects);
        }
        #endregion
    }
}
