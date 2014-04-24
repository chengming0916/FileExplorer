using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.ViewModels.Helpers
{


    public class SingleDropHelper<T> : NotifyPropertyChanged, ISupportDrop
        where T : class, IDraggable
    {

        #region Constructors

        public SingleDropHelper(T tvm,
            Func<bool> isDraggingFunc, Func<string> dropTargetFunc,
            Func<IEnumerable<IDraggable>, T, DragDropEffects> dropHandler)
        {
            _tvm = tvm;
            _dropTargetFunc = dropTargetFunc;
            _isDraggingFunc = isDraggingFunc;
            _dropHandler = dropHandler;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private T _tvm;
        private bool _isDraggingOver = false;
        private Func<IEnumerable<IDraggable>, T, DragDropEffects> _dropHandler;
        private Func<bool> _isDraggingFunc;
        private Func<string> _dropTargetFunc;

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
            get { return _dropTargetFunc(); }
        }

        private T getTabViewModel(IDataObject da)
        {
            return da.GetDataPresent(typeof(T)) ? (T)da.GetData(typeof(T)) : default(T);
        }

        public virtual QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
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
        }

        #endregion
    }


    public class TabDropHelper<T> : SingleDropHelper<T>
     where T : class, IDraggable
    {

        #region Constructors

        public TabDropHelper(T tvm, ITabControlViewModel<T> tcvm)
            : base(tvm, () => tvm.IsDragging, () => tvm.DisplayName,
            (sourceDraggable, disTvm) =>
            {
                var sourceVM = sourceDraggable.FirstOrDefault() as T;
                if (sourceVM != null)
                {
                    int thisIdx = tcvm.GetTabIndex(disTvm);
                    int srcIdx = tcvm.GetTabIndex(sourceVM);
                    if (srcIdx != -1 && thisIdx != -1)
                    {
                        int destIdx = thisIdx > srcIdx ? thisIdx - 1 : thisIdx;
                        tcvm.MoveTab(srcIdx, destIdx);
                        return DragDropEffects.Move;
                    }
                }
                return DragDropEffects.None;
            })
        {
            _tvm = tvm;
            _tcvm = tcvm;
        }

        #endregion

        #region Methods

        public override QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            var baseResult = base.QueryDrop(da, allowedEffects);
            if (baseResult == QueryDropResult.None) //If not T (ViewModel for tab)
                _tcvm.SelectedItem = _tvm;          //Select current tab.
            return baseResult;
        }

        #endregion

        #region Data

        private ITabControlViewModel<T> _tcvm;
        private T _tvm;


        #endregion

        #region Public Properties

        #endregion
    }

}
