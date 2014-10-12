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
    #region DropHelper
    public abstract class DropHelper : NotifyPropertyChanged, ISupportDrop
    {
        #region Constructor

        public DropHelper()
        {
            IsDroppable = true;
            DropTargetLabel = "{MethodLabel} {ItemLabel} to {ISupportDrop.DisplayName}";
        }

        #endregion

        #region Methods

        public abstract QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects);
        public abstract DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects);

        public virtual void OnDragOver()
        {

        }

        public virtual void OnDragLeave()
        {

        }

        #endregion

        #region Data

        private bool _isDraggingOver = false;

        #endregion

        #region Public Properties

        public virtual bool IsDraggingOver
        {
            get
            {
                return _isDraggingOver;
            }
            set
            {
                if (_isDraggingOver != value)
                {
                    _isDraggingOver = value;
                    if (value)
                        OnDragOver();
                    else OnDragLeave();
                    NotifyOfPropertyChanged(() => IsDraggingOver);
                }
            }
        }


        public virtual bool IsDroppable
        {
            get;
            set;
        }

        public virtual string DropTargetLabel
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        #endregion

       
    }

    #endregion

    #region NoDropHelper

    public class NoDropHelper : DropHelper
    {
        public static ISupportDrop Instance = new NoDropHelper();

        public NoDropHelper()
            : base()
        {
            IsDroppable = false;
        }

        public override QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return QueryDropEffects.None;
        }

        public override DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return DragDropEffects.None;
        }
    }

    #endregion

    #region DropHelper<T>
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="M">Model of entry to be dropped.</typeparam>
    public abstract class DropHelper<M> : NotifyPropertyChanged, ISupportDrop
        where M : class
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converter">Convert from IDraggable to M, you can use LambdaConverter or other IValueCinverter</param>
        public DropHelper(IValueConverter converter)
        {
            _converter = converter;
            IsDroppable = true;
            DropTargetLabel = "{MethodLabel} {ItemLabel} to {ISupportDrop.DisplayName}";
        }

        #endregion

        #region Methods

        public abstract QueryDropEffects QueryDrop(IEnumerable<M> models, DragDropEffects allowedEffects);
        public abstract DragDropEffects Drop(IEnumerable<M> models, DragDropEffects allowedEffects);

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

        public QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return QueryDrop(Convert(draggables, true), allowedEffects);
        }

        public DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return Drop(Convert(draggables, true), allowedEffects);
        }

        public virtual void OnDragOver()
        {

        }

        public virtual void OnDragLeave()
        {

        }

        #endregion

        #region Data

        protected IValueConverter _converter;
        private bool _isDraggingOver = false;

        #endregion

        #region Public Properties

        public virtual bool IsDraggingOver
        {
            get
            {
                return _isDraggingOver;
            }
            set
            {
                if (_isDraggingOver != value)
                {
                    _isDraggingOver = value;
                    if (value)
                        OnDragOver();
                    else OnDragLeave();
                    NotifyOfPropertyChanged(() => IsDraggingOver);
                }
            }
        }


        public virtual bool IsDroppable
        {
            get;
            set;
        }

        public virtual string DropTargetLabel
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }


        #endregion
    }

    #endregion

    #region LambdaDropHelper

    public class LambdaDropHelper<M> : DropHelper<M>
        where M : class
    {
        private Func<IEnumerable<M>, DragDropEffects, QueryDropEffects> _queryDropFunc;
        private Func<IEnumerable<M>, DragDropEffects, DragDropEffects> _dropFunc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converter">Convert from IDraggable to M, you can use LambdaConverter or other IValueCinverter.</param>
        /// <param name="queryDropFunc">Called when QueryDrop, with models and allowEffects as parameter, return supported effect.</param>
        /// <param name="dropFunc">Call when Drop, with models and allowedEffects as parameter, return actual effect.</param>
        public LambdaDropHelper(IValueConverter converter, 
            Func<IEnumerable<M>, DragDropEffects, QueryDropEffects> queryDropFunc,
            Func<IEnumerable<M>, DragDropEffects, DragDropEffects> dropFunc)
            : base(converter)
        {
            _queryDropFunc = queryDropFunc;
            _dropFunc = dropFunc;
        }

        public override QueryDropEffects QueryDrop(IEnumerable<M> models, DragDropEffects allowedEffects)
        {
            return _queryDropFunc(models, allowedEffects);
        }

        public override DragDropEffects Drop(IEnumerable<M> models, DragDropEffects allowedEffects)
        {
            return _dropFunc(models, allowedEffects);
        }
    }

    #endregion
}
