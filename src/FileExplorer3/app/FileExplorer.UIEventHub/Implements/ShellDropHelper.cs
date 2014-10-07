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

    #region ShellDropHelper
    public abstract class ShellDropHelper : DropHelper, ISupportShellDrop
    {

        #region Constructor

        #endregion

        #region Methods

        public abstract IEnumerable<IDraggable> QueryDropDraggables(IDataObject da);
        public abstract DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects);

        public override DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        {
            return Drop(draggables, null, allowedEffects);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

    }
    #endregion

    #region ShellDropHelper<T>
    public abstract class ShellDropHelper<M> : DropHelper<M>, ISupportShellDrop
        where M : class
    {

        #region Constructor


        /// <summary>
        /// 
        /// </summary>
        /// <param name="converter">Convert from IDraggable to M</param>
        public ShellDropHelper(IValueConverter converter)
            : base(converter)
        {
        }


        #endregion

        #region Methods

        public abstract IEnumerable<M> QueryDropModels(IDataObject da);
        
        public virtual DragDropEffects Drop(IEnumerable<M> models, IDataObject da, DragDropEffects allowedEffects)
        {
            return Drop(models, allowedEffects);
        }

        public override DragDropEffects Drop(IEnumerable<M> models, DragDropEffects allowedEffects)
        {
            return Drop(models, null, allowedEffects);
        }


        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            return ConvertBack(QueryDropModels(da), true);
        }

        public virtual DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        {
            return Drop(Convert(draggables, true), da, allowedEffects);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }
    #endregion
}
