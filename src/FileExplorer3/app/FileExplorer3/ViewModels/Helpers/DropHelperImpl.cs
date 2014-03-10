using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.ViewModels.Helpers
{
    public class NoDropHelper : NotifyPropertyChanged, ISupportDrop
    {
        #region Constructor

        public static NoDropHelper Instance = new NoDropHelper();

        #endregion

        #region Methods

        public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            throw new NotImplementedException();
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public bool IsDraggingOver
        {
            set { }
        }

        public bool IsDroppable
        {
            get { return false; }
        }

        public string DropTargetLabel
        {
            get { return "None"; }
        }

        #endregion



     
    }

    public class DropHelper<T> : NotifyPropertyChanged, ISupportDrop
    {
        #region Constructor

        public DropHelper(
            Func<string> dropTargetFunc,            
            Func<IEnumerable<T>, DragDropEffects, QueryDropResult> queryDropFunc,            
            Func<IDataObject, IEnumerable<T>> dataobjectFunc,
            Func<IEnumerable<T>, IDataObject, DragDropEffects, DragDropEffects> dropFunc,
            Func<T, IDraggable> convertFunc
            )
        {
            IsDroppable = true;

            _dropTargetFunc = dropTargetFunc;
            _queryDropFunc = queryDropFunc;
            _dataObjectFunc = dataobjectFunc;
            _dropFunc = dropFunc;
            _convertFunc = convertFunc;
        }

        #endregion

        #region Methods

        public QueryDropResult QueryDrop(IDataObject da, DragDropEffects allowedEffects)
        {
            var entries = _dataObjectFunc(da);
            return _queryDropFunc(entries, allowedEffects);
        }


        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            return from e in _dataObjectFunc(da) select _convertFunc(e);
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            var entries = _dataObjectFunc(da);
            return _dropFunc(entries, da, allowedEffects);
        }

        #endregion

        #region Data

        private Func<string> _dropTargetFunc;
        private Func<IDataObject, IEnumerable<T>> _dataObjectFunc;
        private bool _isDroppable = true, _isDraggingOver = false;
        private Func<T, IDraggable> _convertFunc;
        private Func<IEnumerable<T>, DragDropEffects, QueryDropResult> _queryDropFunc;
        private Func<IEnumerable<T>, IDataObject, DragDropEffects, DragDropEffects> _dropFunc;

        #endregion

        #region Public Properties

        public bool IsDroppable
        {
            get { return _isDroppable; }
            set { _isDroppable = value; NotifyOfPropertyChanged(() => IsDroppable); }
        }

        public bool IsDraggingOver
        {
            get { return _isDraggingOver; }
            set { _isDraggingOver = value; NotifyOfPropertyChanged(() => IsDraggingOver); }
        }

        public string DropTargetLabel { get { return _dropTargetFunc(); } }
        
        #endregion






    }
}
