using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{
    /// <summary>
    /// Implemented by both Tab and TabControl's ViewModel, allow re-index tab using Drag n Drop.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public class TabControlDragHelper<M> : ShellDragHelper<M>
       where M : class, IDraggable
    {
        
        #region Constructors

        public TabControlDragHelper(ITabControlViewModel<M> tcvm)            
            : base(LambdaValueConverter.ConvertUsingCast<IDraggable, M>())
        {
            _tcvm = tcvm;
            HasDraggables = true;
        }

        #endregion

        #region Methods

        public override IEnumerable<M> GetModels()
        {
            yield return _tcvm.SelectedItem;
        }

        public override IDataObject GetDataObject(IEnumerable<M> models)
        {
            return new DataObject(typeof(IEnumerable<M>), models);
        }

        public override DragDropEffects QueryDrag(IEnumerable<M> models)
        {
            return DragDropEffects.Move;
        }

        public override void OnDragCompleted(IEnumerable<M> models, IDataObject da, DragDropEffects effect)
        {            
        }

        #endregion

        #region Data

        private ITabControlViewModel<M> _tcvm;


        #endregion

        #region Public Properties

        #endregion
    }
}
