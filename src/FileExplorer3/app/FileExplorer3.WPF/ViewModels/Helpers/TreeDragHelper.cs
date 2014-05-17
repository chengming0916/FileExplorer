using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF.ViewModels.Helpers
{
    public class TreeDragHelper<T> : ISupportDrag       
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

        #endregion

        #region Public Properties

        #endregion

    }
}
