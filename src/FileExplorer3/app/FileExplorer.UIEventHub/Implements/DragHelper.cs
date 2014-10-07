using FileExplorer.WPF.Utils;
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
    #region DragHelper
    public abstract class DragHelper : NotifyPropertyChanged, ISupportDrag
    {
        #region Constructor

        public DragHelper()
        {
            HasDraggables = true;
        }

        #endregion

        #region Methods

        public abstract IEnumerable<IDraggable> GetDraggables();
        public abstract DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables);
        public abstract void OnDragCompleted(IEnumerable<IDraggable> draggables, DragDropEffects effect);

        public virtual void OnDragStart()
        {

        }

        public virtual void OnDragEnd()
        {

        }

        #endregion

        #region Data

        private bool _isDraggingFrom = false;

        #endregion

        #region Public Properties

        public virtual bool HasDraggables
        {
            get;
            set;
        }

        public virtual bool IsDraggingFrom
        {
            get
            {
                return _isDraggingFrom;
            }
            set
            {
                if (_isDraggingFrom != value)
                {
                    _isDraggingFrom = value;
                    if (value)
                        OnDragStart();
                    else OnDragEnd();
                    NotifyOfPropertyChanged(() => IsDraggingFrom);
                }
            }
        }

        #endregion





    }

    #endregion

    #region DragHelper<T>
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="M">Model of entry to be Dragped.</typeparam>
    public abstract class DragHelper<M> : NotifyPropertyChanged, ISupportDrag
        where M : class
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converter">Convert from IDraggable to M</param>
        public DragHelper(IValueConverter converter)
        {
            _converter = converter;
            HasDraggables = true;
        }

        #endregion

        #region Methods

        #region Convert Func

        public virtual M Convert(IDraggable draggable)
        {
            return _converter.Convert(draggable, typeof(M), null, Thread.CurrentThread.CurrentUICulture) as M;
        }

        public virtual IEnumerable<M> Convert(IEnumerable<IDraggable> draggables, bool ignoreNull = true)
        {
            return draggables.Select(d => Convert(d))
                .Where(m => !ignoreNull || m != null);
        }

        public virtual IDraggable ConvertBack(M model)
        {
            return _converter.ConvertBack(model, typeof(IDraggable), null, Thread.CurrentThread.CurrentUICulture) as IDraggable;
        }

        public virtual IEnumerable<IDraggable> ConvertBack(IEnumerable<M> models, bool ignoreNull = true)
        {
            return models.Select(m => ConvertBack(m))
                .Where(d => !ignoreNull || d != null);
        }
        #endregion

        public abstract IEnumerable<M> GetModels();
        public abstract DragDropEffects QueryDrag(IEnumerable<M> models);
        public abstract void OnDragCompleted(IEnumerable<M> models, DragDropEffects effect);

        public IEnumerable<IDraggable> GetDraggables()
        {
            return ConvertBack(GetModels());
        }

        DragDropEffects ISupportDrag.QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return QueryDrag(Convert(draggables));
        }

        void ISupportDrag.OnDragCompleted(IEnumerable<IDraggable> draggables, DragDropEffects effect)
        {
            OnDragCompleted(Convert(draggables), effect);
        }

        public virtual void OnDragStart()
        {

        }

        public virtual void OnDragEnd()
        {

        }

        #endregion

        #region Data

        protected IValueConverter _converter;
        private bool _isDraggingFrom = false;

        #endregion

        #region Public Properties

        public virtual bool IsDraggingFrom
        {
            get
            {
                return _isDraggingFrom;
            }
            set
            {
                if (_isDraggingFrom != value)
                {
                    _isDraggingFrom = value;
                    if (value)
                        OnDragStart();
                    else OnDragEnd();
                    NotifyOfPropertyChanged(() => IsDraggingFrom);
                }
            }
        }


        public virtual bool HasDraggables
        {
            get;
            set;
        }

        #endregion
    }

    #endregion

    #region LambdaDragHelper

    public class LambdaDragHelper<M> : DragHelper<M>
        where M : class 
    {
        private Func<IEnumerable<M>, DragDropEffects> _queryDragFunc;
        private Action<IEnumerable<M>, DragDropEffects> _onDragCompletedAction;
        private Func<IEnumerable<M>> _getModelsFunc;

        public LambdaDragHelper(IValueConverter converter,
            Func<IEnumerable<M>> getModelsFunc,
            Func<IEnumerable<M>, DragDropEffects> queryDragFunc,
            Action<IEnumerable<M>, DragDropEffects> onDragCompletedAction)
            : base(converter)
        {
            _getModelsFunc = getModelsFunc;
            _queryDragFunc = queryDragFunc;
            _onDragCompletedAction = onDragCompletedAction;
        }

        public override IEnumerable<M> GetModels()
        {
            return _getModelsFunc();
        }

        public override DragDropEffects QueryDrag(IEnumerable<M> models)
        {
            return _queryDragFunc(models);
        }

        public override void OnDragCompleted(IEnumerable<M> models, DragDropEffects effect)
        {
            _onDragCompletedAction(models, effect);
        }
    }
    #endregion
}
