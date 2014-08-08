using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF.ViewModels.Helpers
{
    public class SingleDragHelper<T> : NotifyPropertyChanged, ISupportDrag
       where T : IDraggable
    {

        #region Constructors

        public SingleDragHelper(Func<T> selectedTabFunc)
        {
            _selectedTabFunc = selectedTabFunc;
        }

        #endregion

        #region Methods

        public DragDropEffects NotifyDrop(IEnumerable<IDraggable> draggables, T overTabModel)
        {
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
            return new DataObject(typeof(T), (T)draggables.FirstOrDefault());
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

   
    public class TabControlDragHelper<T> : SingleDragHelper<T>
      where T : IDraggable
    {

        #region Constructors

        public TabControlDragHelper(ITabControlViewModel<T> tcvm)
            : base(() => tcvm.SelectedItem)
        {

        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }

}
