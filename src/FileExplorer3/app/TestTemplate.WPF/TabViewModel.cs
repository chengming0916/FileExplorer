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

    public class TabControlViewModel : NotifyPropertyChanged, ISupportDragHelper
    {
        public TabControlViewModel(int numberOfTabs = 20)
        {
            Items = new ObservableCollection<TabViewModel>();
            for (int i = 0; i < numberOfTabs; i++)
                Items.Add(new TabViewModel(this, String.Format("Tab {0}", i)));
            DragHelper = new TabControlDragHelper<TabViewModel>(() => Items[SelectedIndex]);
        }

        public ObservableCollection<TabViewModel> Items { get; set; }
        private int _selectedIdx = 0;
        public int SelectedIndex
        {
            get { return _selectedIdx; }
            set { _selectedIdx = value; NotifyOfPropertyChanged(() => SelectedIndex); }
        }

        public ISupportDrag DragHelper
        {
            get;
            set;
        }

        //public bool HasDraggables
        //{
        //    get { return true; ; }
        //}

        //public IEnumerable<IDraggable> GetDraggables()
        //{
        //    return new List<IDraggable>() { Items[_selectedIdx] };
        //}

        //public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        //{
        //    return DragDropEffects.Move;
        //}

        //public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        //{
        //    return new DataObject(typeof(TabViewModel), draggables.FirstOrDefault() as TabViewModel);
        //}

        //public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        //{
        //    Debug.WriteLine("DragCompleted");
        //}


        //public void MoveTab(TabViewModel tvm, int idx)
        //{
        //    if (idx != -1 && tvm != null && Items.Contains(tvm))
        //    {
        //        var draggingItemIdx = Items.IndexOf(tvm);
        //        //Adjust if include the dragging item in the index
        //        var targetItemIdx = idx > draggingItemIdx ? idx - 1 : idx;
        //        Items.Move(Items.IndexOf(tvm), targetItemIdx);
        //    }
        //}

   
    }

    public class TabControlDragHelper<T> : NotifyPropertyChanged, ISupportDrag
        where T : TabViewModel
    {

        #region Constructors

        public TabControlDragHelper(Func<T> selectedTabFunc)
        {
            _selectedTabFunc = selectedTabFunc;
        }

        #endregion

        #region Methods

        //public void MoveTab(T tvm, int idx)
        //{
        //    if (idx != -1 && tvm != null && _items.Contains(tvm))
        //    {
        //        var draggingItemIdx = _items.IndexOf(tvm);
        //        //Adjust if include the dragging item in the index
        //        var targetItemIdx = idx > draggingItemIdx ? idx - 1 : idx;
        //        _items.Move(_items.IndexOf(tvm), targetItemIdx);
        //    }
        //}

        public DragDropEffects NotifyDrop(IEnumerable<IDraggable> draggables, T overTabModel)
        {
            //T firstTab = draggables.FirstOrDefault() as T;
            // if (firstTab != null)
            //_tabControlHelper.MoveTab(firstTab, _tabControlHelper.Items.IndexOf(_tvm));
            //return DragDropEffects.Move;
            return _dropHandler(draggables, overTabModel);
        }


        public IEnumerable<IDraggable> GetDraggables()
        {
            T selectedTab = _selectedTabFunc();
            if (selectedTab != null)
                return new List<IDraggable>() { selectedTab };
            else return new List<IDraggable>();
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return DragDropEffects.Move;
        }

        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            return new DataObject(typeof(T), draggables.FirstOrDefault() as T);
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        {
        }


        #endregion

        #region Data

        private Func<IEnumerable<IDraggable>, T, DragDropEffects> _dropHandler;
        private Func<T> _selectedTabFunc;

        #endregion

        #region Public Properties


        public bool HasDraggables
        {
            get { return _selectedTabFunc() != null; }
        }

        #endregion






    }

    public class TabDropHelper<T> : NotifyPropertyChanged, ISupportDrop
        where T : TabViewModel
    {

        #region Constructors

        public TabDropHelper(T tvm, 
            Func<bool> isDraggingFunc,
            Func<IEnumerable<IDraggable>, T, DragDropEffects> dropHandler)
        {
            _tvm = tvm;
            _isDraggingFunc = isDraggingFunc;
            _dropHandler = dropHandler;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private T _tvm;
        //private TabControlDragHelper<T> _tabControlHelper;
        private bool _isDraggingOver = false;
        private Func<IEnumerable<IDraggable>, T, DragDropEffects> _dropHandler;
        private Func<bool> _isDraggingFunc;

        #endregion

        #region Public Properties

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


        public bool ShowPlaceHolder
        {
            get { return !_isDraggingFunc() && _isDraggingOver; }
        }

        public bool IsDroppable
        {
            get { return true; }
        }

        public string DropTargetLabel
        {
            get { return _tvm.Header; }
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

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            return _dropHandler(draggables, this._tvm);
            //if (getTabViewModel(da) != null)
            //{
            //    T firstTab = draggables.FirstOrDefault() as T;
            //    _tabControlHelper.MoveTab(firstTab,_tabControlHelper.Items.IndexOf(_tvm));
            //    return DragDropEffects.Move;
            //}
            //else return DragDropEffects.None;
        }

        #endregion
    }

    public class TabViewModel : NotifyPropertyChanged, IDraggable, ISupportDropHelper
    {
        public TabViewModel(TabControlViewModel tcvm, string header)
        {
            _tcvm = tcvm;
            Header = header;
            DropHelper = new TabDropHelper<TabViewModel>(this, () => IsDragging,
                (draggables, tvm) =>
                {
                    var sourceVM = draggables.FirstOrDefault() as TabViewModel;
                    if (sourceVM != null)
                    {
                        int thisIdx = _tcvm.Items.IndexOf(this);
                        int srcIdx = _tcvm.Items.IndexOf(sourceVM);
                        if (srcIdx != -1 && thisIdx != -1)
                        {
                            int destIdx = thisIdx > srcIdx ? thisIdx - 1 : thisIdx;
                            _tcvm.Items.Move(srcIdx, destIdx);
                            return DragDropEffects.Move;
                        }
                    }
                    return DragDropEffects.None;
                });
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



        //private bool _isDraggingOver = false;
        private TabControlViewModel _tcvm;
        private bool _isDragging = false;

        //public bool IsDraggingOver
        //{
        //    get { return _isDraggingOver; }
        //    set
        //    {
        //        _isDraggingOver = value;
        //        NotifyOfPropertyChanged(() => IsDraggingOver);
        //        NotifyOfPropertyChanged(() => ShowPlaceHolder);
        //    }
        //}
        public ISupportDrop DropHelper
        {
            get;
            set;
        }

        public float HeaderOpacity { get { return _isDragging ? 0.5f : 1f; } }
        //public bool ShowPlaceHolder
        //{
        //    get { return !_isDragging && _isDraggingOver; }
        //}

        //public bool IsDroppable
        //{
        //    get { return true; }
        //}

        //public string DropTargetLabel
        //{
        //    get { return Header; }
        //}

        //private TabViewModel getTabViewModel(IDataObject da)
        //{
        //    return da.GetDataPresent(typeof(TabViewModel)) ? da.GetData(typeof(TabViewModel)) as TabViewModel : null;
        //}

        //public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        //{
        //    if (getTabViewModel(da) != null)
        //        return QueryDropResult.CreateNew(DragDropEffects.Move);
        //    return QueryDropResult.None;
        //}

        //public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        //{
        //    if (getTabViewModel(da) != null)
        //        return new List<IDraggable>() { getTabViewModel(da) };
        //    return new List<IDraggable>();
        //}

        public override string ToString()
        {
            return Header;
        }
        //public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        //{
        //    if (getTabViewModel(da) != null)
        //    {
        //        _tcvm.MoveTab(draggables.FirstOrDefault() as TabViewModel, _tcvm.Items.IndexOf(this));
        //        Debug.WriteLine("Drop");
        //        return DragDropEffects.Move;
        //    }
        //    else return DragDropEffects.None;
        //}

        
    }
}
