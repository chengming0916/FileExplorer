//using FileExplorer.UIEventHub;
//using FileExplorer.WPF.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace FileExplorer3.WPF.ViewModels.Helpers
//{
//    public class TabDropHelper<M> : DropHelper<M>
//    {
//        #region Constructors

//        public TabDropHelper(T tvm, ITabControlViewModel<T> tcvm)
//            : base(new LambdaDragHelper)
//            //: base(tvm, () => tvm.IsDragging, () => tvm.DisplayName,
//            //(sourceDraggable, disTvm) =>
//            //{
//            //    var sourceVM = sourceDraggable.FirstOrDefault() as T;
//            //    if (sourceVM != null)
//            //    {
//            //        int thisIdx = tcvm.GetTabIndex(disTvm);
//            //        int srcIdx = tcvm.GetTabIndex(sourceVM);
//            //        if (srcIdx != -1 && thisIdx != -1)
//            //        {
//            //            int destIdx = thisIdx > srcIdx ? thisIdx - 1 : thisIdx;
//            //            tcvm.MoveTab(srcIdx, destIdx);
//            //            return DragDropEffects.Move;
//            //        }
//            //    }
//            //    return DragDropEffects.None;
//            //})
//        {

//            _tvm = tvm;
//            _tcvm = tcvm;
//        }

//        #endregion

//        #region Methods

//        public override QueryDropEffects QueryDrop(IDataObject da, DragDropEffects allowedEffects)
//        {
//            var baseResult = base.QueryDrop(da, allowedEffects);
//            if (baseResult == QueryDropEffects.None) //If not T (ViewModel for tab)
//                _tcvm.SelectedItem = _tvm;          //Select current tab.
//            return baseResult;
//        }

//        #endregion

//        #region Data

//        private ITabControlViewModel<T> _tcvm;
//        private T _tvm;


//        #endregion

//        #region Public Properties

//        #endregion

//    }
//}
