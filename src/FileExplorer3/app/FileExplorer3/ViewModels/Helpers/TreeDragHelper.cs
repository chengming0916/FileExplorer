using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.ViewModels.Helpers
{
    public class TreeDragHelper<VM, T> : ISupportDrag
       where VM : IDraggable
    {

        #region Constructor

        public TreeDragHelper(IEntriesHelper<VM> entries, ITreeSelector<VM, T> selection,

            Func<IEnumerable<T>, DragDropEffects> queryDragFunc,
            Func<IEnumerable<T>, IDataObject> dataObjectFunc,
            Action<IEnumerable<T>, DragDropEffects> dragCompletedAction,
            Func<VM, T> convertBackFunc)
        {
            _entries = entries;
            _selection = selection;
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
            if (_selection.RootSelector.SelectedViewModel != null)
                yield return _selection.RootSelector.SelectedViewModel;
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            var entryModels = draggables.Cast<VM>().Select(d => _convertBackFunc(d));
            return _queryDragFunc(entryModels);

        }

        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            var entryModels = draggables.Cast<VM>().Select(d => _convertBackFunc(d));
            return _dataObjectFunc(entryModels);
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        {
            if (_dragCompletedAction == null)
                throw new ArgumentException();
            var entryModels = draggables.Cast<VM>().Select(d => _convertBackFunc(d));
            _dragCompletedAction(entryModels, effect);
        }

        #endregion

        #region Data

        private ITreeSelector<VM, T> _selection;
        private IEntriesHelper<VM> _entries;
        private Func<VM, T> _convertBackFunc;
        private Func<IEnumerable<T>, DragDropEffects> _queryDragFunc;
        private Func<IEnumerable<T>, IDataObject> _dataObjectFunc;
        private Action<IEnumerable<T>, DragDropEffects> _dragCompletedAction;

        #endregion

        #region Public Properties

        #endregion

    }
}
