using FileExplorer.UIEventHub;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF.ViewModels.Helpers
{
    public class TreeDragHelper<T> : NotifyPropertyChanged, ISupportDrag       
    {

        #region Constructor

        public TreeDragHelper(
            Func<IEnumerable<IDraggable>> getDraggableFunc,
            Func<IEnumerable<T>, DragDropEffects> queryDragFunc,
            Func<IEnumerable<T>, IDataObject> dataObjectFunc,
            Action<IEnumerable<T>, IDataObject, DragDropEffects> dragCompletedAction,
            Func<IDraggable, T> convertBackFunc)
        {
            _getDraggableFunc = getDraggableFunc;
            _queryDragFunc = queryDragFunc;
            _dataObjectFunc = dataObjectFunc;
            _dragCompletedAction = dragCompletedAction;
            _convertBackFunc = convertBackFunc;
        }

        #endregion

        #region Methods

        public bool HasDraggables
        {
            get { return true; }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return _getDraggableFunc();
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            var entryModels = draggables.Select(d => _convertBackFunc(d));
            return _queryDragFunc(entryModels);

        }

        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            var entryModels = draggables.Select(d => _convertBackFunc(d));
            return _dataObjectFunc(entryModels);
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, DragDropEffects effect)
        {
            OnDragCompleted(draggables, null, effect);
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        {
            if (_dragCompletedAction == null)
                throw new ArgumentException();
            var entryModels = draggables.Select(d => _convertBackFunc(d));
            _dragCompletedAction(entryModels, da, effect);
        }

        #endregion

        #region Data
        
        private Func<IDraggable, T> _convertBackFunc;
        private Func<IEnumerable<T>, DragDropEffects> _queryDragFunc;
        private Func<IEnumerable<T>, IDataObject> _dataObjectFunc;
        private Action<IEnumerable<T>, IDataObject, DragDropEffects> _dragCompletedAction;
        private Func<IEnumerable<IDraggable>> _getDraggableFunc;
        private bool _isDraggingFrom = false;

        #endregion

        #region Public Properties

        public bool IsDraggingFrom
        {
            get { return _isDraggingFrom; }
            set { _isDraggingFrom = value; NotifyOfPropertyChanged(() => IsDraggingFrom); }
        }

        #endregion







        
    }
}
