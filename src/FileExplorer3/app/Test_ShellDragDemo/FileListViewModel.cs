using FileExplorer;
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
        private bool _isDraggingOver, _isDraggingFrom;

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


        public IDataObject GetDataObject(IDraggable[] draggables)
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

        public void OnDragCompleted(IDraggable[] draggables, IDataObject da, System.Windows.DragDropEffects effect)
        {
            OnDragCompleted(draggables, effect);
        }

        public bool HasDraggables
        {
            get { return Items.Any(f => f.IsSelected); }
        }

        public IDraggable[] GetDraggables()
        {
            return Items.Where(f => f.IsSelected).ToArray();
        }

        public DragDropEffects QueryDrag(IDraggable[] draggables)
        {
            return DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move;
        }

        public void OnDragCompleted(IDraggable[] draggables, System.Windows.DragDropEffects effect)
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

        public IDraggable[] QueryDropDraggables(IDataObject dataObj)
        {
            List<IDraggable> retVal = new List<IDraggable>();
            DataObject da = dataObj as DataObject;
            if (da.ContainsFileDropList())
            {
                foreach (var file in da.GetFileDropList())                
                    retVal.Add(new FileViewModel(file));                                                            
            }
            return retVal.ToArray();
        }

        public DragDropEffects Drop(IDraggable[] draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            foreach (var fvm in Items)
                fvm.IsSelected = false;
            foreach (var fvm in draggables.Cast<FileViewModel>())
                this.Items.Add(fvm);
            if (allowedEffects.HasFlag(DragDropEffects.Move))
                return DragDropEffects.Move;
            if (allowedEffects.HasFlag(DragDropEffects.Copy))
                return DragDropEffects.Copy;
            return DragDropEffects.Link;
        }

        public bool IsDraggingFrom
        {
            get { return _isDraggingFrom; }
            set { _isDraggingFrom = value; NotifyOfPropertyChanged(() => IsDraggingFrom); }
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

        public QueryDropEffects QueryDrop(IDraggable[] draggables, DragDropEffects allowedEffects)
        {
            if (draggables.Any(d => Items.Contains(d)))
                return QueryDropEffects.None;

            return QueryDropEffects.CreateNew(allowedEffects & (DragDropEffects.Link | DragDropEffects.Move), 
                allowedEffects & DragDropEffects.Move);
        }

        public DragDropEffects Drop(IDraggable[] draggables, DragDropEffects allowedEffects)
        {
            if (draggables.Any(d => Items.Contains(d)))
                return DragDropEffects.None;

            return Drop(draggables, null, allowedEffects);
        }
        #endregion
    }
}
