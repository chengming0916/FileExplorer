using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FileExplorer.UIEventHub
{

    #region ShellDragHelper
    public abstract class ShellDragHelper : DragHelper, ISupportShellDrag
    {

        #region Constructor

        #endregion

        #region Methods

        public abstract IDataObject GetDataObject(IEnumerable<IDraggable> draggables);

        public abstract void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect);

        public override void OnDragCompleted(IEnumerable<IDraggable> draggables, DragDropEffects effect)
        {
            OnDragCompleted(draggables, null, effect);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion



    }
    #endregion

    #region ShellDragHelper<T>
    public abstract class ShellDragHelper<M> : DragHelper<M>, ISupportShellDrag
        where M : class
    {

        #region Constructor


        /// <summary>
        /// 
        /// </summary>
        /// <param name="converter">Convert from IDraggable to M</param>
        public ShellDragHelper(IValueConverter converter)
            : base(converter)
        {
        }


        #endregion

        #region Methods

        public abstract IDataObject GetDataObject(IEnumerable<M> models);
        public abstract void OnDragCompleted(IEnumerable<M> models, IDataObject da, DragDropEffects effect);


        IDataObject ISupportShellDrag.GetDataObject(IEnumerable<IDraggable> draggables)
        {
            return GetDataObject(Convert(draggables));
        }

        void ISupportShellDrag.OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects effect)
        {
            OnDragCompleted(Convert(draggables), da, effect);
        }

        public override void OnDragCompleted(IEnumerable<M> models, DragDropEffects effect)
        {
            OnDragCompleted(models, null, effect);
        }
      
        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }
    #endregion

    #region LambdaShellDragHelper

     public class LambdaShellDragHelper<M> : ShellDragHelper<M>
        where M : class
     {
         private Func<IEnumerable<M>> _getModelsFunc;
         private Func<IEnumerable<M>, DragDropEffects> _queryDragFunc;
         private Action<IEnumerable<M>, IDataObject, DragDropEffects> _onDragCompletedAction;         
         private IValueConverter _dataObjectConverter;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="converter"></param>
         /// <param name="dataObjectConverter">IValueConverter convert IEnumerable[M] to IDataObject</param>
         /// <param name="getModelsFunc"></param>
         /// <param name="queryDragFunc"></param>
         /// <param name="onDragCompletedAction"></param>
         public LambdaShellDragHelper(IValueConverter converter, IValueConverter dataObjectConverter,
            Func<IEnumerable<M>> getModelsFunc,            
            Func<IEnumerable<M>, DragDropEffects> queryDragFunc,
            Action<IEnumerable<M>, IDataObject, DragDropEffects> onDragCompletedAction)
            : base(converter)
        {
            _getModelsFunc = getModelsFunc;
            _dataObjectConverter = dataObjectConverter;
            _queryDragFunc = queryDragFunc;
            _onDragCompletedAction = onDragCompletedAction;
        }

         public override IEnumerable<M> GetModels()
         {
             return _getModelsFunc();
         }

         public override IDataObject GetDataObject(IEnumerable<M> models)
         {
             return _dataObjectConverter.Convert(models, typeof(IDataObject), null, Thread.CurrentThread.CurrentUICulture) as IDataObject;
         }

         public override DragDropEffects QueryDrag(IEnumerable<M> models)
         {
             return _queryDragFunc(models);
         }

         
         public override void OnDragCompleted(IEnumerable<M> models, IDataObject da, DragDropEffects effect)
         {
             _onDragCompletedAction(models, da, effect);
         }         
     }

    #endregion
}
