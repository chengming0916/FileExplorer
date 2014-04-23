using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestTemplate.WPF
{

    public class TabControlViewModel : NotifyPropertyChanged, ISupportDrag
    {
        public TabControlViewModel(int numberOfTabs = 20)
        {
            Items = new ObservableCollection<TabViewModel>();
            for (int i = 0; i < numberOfTabs; i++)
                Items.Add(new TabViewModel(this, String.Format("Tab {0}", i)));
        }

        public ObservableCollection<TabViewModel> Items { get; set; }
        private int _selectedIdx = 0;
        public int SelectedIndex
        {
            get { return _selectedIdx; }
            set { _selectedIdx = value; NotifyOfPropertyChanged(() => SelectedIndex); }
        }

        public bool HasDraggables
        {
            get { return true; ; }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return new List<IDraggable>() { Items[_selectedIdx] };
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return DragDropEffects.Move;
        }

        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            return new DataObject(typeof(TabViewModel), draggables.FirstOrDefault() as TabViewModel);
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        {
            Debug.WriteLine("DragCompleted");
        }

        public void MoveTab(TabViewModel tvm, int idx)
        {
            if (idx != -1 && tvm != null && Items.Contains(tvm))
            {
                var draggingItemIdx = Items.IndexOf(tvm);
                //Adjust if include the dragging item in the index
                var targetItemIdx = idx > draggingItemIdx ? idx - 1 : idx;
                Items.Move(Items.IndexOf(tvm), targetItemIdx);
            }
        }
    }

    public class TabViewModel : NotifyPropertyChanged, IDraggable, ISupportDrop
    {
        public TabViewModel(TabControlViewModel tcvm, string header)
        {
            _tcvm = tcvm;
            Header = header;
        }

        public string Header { get; set; }

        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                _isDragging = value;
                NotifyOfPropertyChanged(() => IsDragging);
                NotifyOfPropertyChanged(() => HeaderOpacity);
            }
        }



        private bool _isDraggingOver = false;
        private TabControlViewModel _tcvm;
        private bool _isDragging = false;

        public bool IsDraggingOver
        {
            get { return _isDraggingOver; }
            set
            {
                _isDraggingOver = value;
                NotifyOfPropertyChanged(() => IsDraggingOver);
                NotifyOfPropertyChanged(() => ShowPlaceHolder);
            }
        }


        public float HeaderOpacity { get { return _isDragging ? 0.5f : 1f; } }
        public bool ShowPlaceHolder
        {
            get { return !_isDragging && _isDraggingOver; }
        }

        public bool IsDroppable
        {
            get { return true; }
        }

        public string DropTargetLabel
        {
            get { return Header; }
        }

        private TabViewModel getTabViewModel(IDataObject da)
        {
            return da.GetDataPresent(typeof(TabViewModel)) ? da.GetData(typeof(TabViewModel)) as TabViewModel : null;
        }

        public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            if (getTabViewModel(da) != null)
                return QueryDropResult.CreateNew(DragDropEffects.Move);
            return QueryDropResult.None;
        }

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            if (getTabViewModel(da) != null)
                return new List<IDraggable>() { getTabViewModel(da) };
            return new List<IDraggable>();
        }

        public override string ToString()
        {
            return Header;
        }
        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            if (getTabViewModel(da) != null)
            {
                _tcvm.MoveTab(draggables.FirstOrDefault() as TabViewModel, _tcvm.Items.IndexOf(this));
                Debug.WriteLine("Drop");
                return DragDropEffects.Move;
            }
            else return DragDropEffects.None;
        }
    }
}
