using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.WPF
{
    public abstract class PanelLayoutBase : IPanelLayoutHelper
    {
        #region fields
        protected IOCPanel _panel;
        protected ConcurrentDictionary<int, ChildInfo> _childInfoDic;
        protected IItemGeneratorHelper _generator;

        #endregion

        #region constructors

        protected PanelLayoutBase(IOCPanel panel, IItemGeneratorHelper generator)
        {
            _panel = panel;
            _generator = generator;
            _childInfoDic = new ConcurrentDictionary<int, ChildInfo>();
            Extent = Size.Empty;
        }

        #endregion

        #region events

        #endregion

        #region properties

        public Size Extent { get; set; }

        public ChildInfo this[int idx]
        {
            get { return _childInfoDic.ContainsKey(idx) ? _childInfoDic[idx] : ChildInfo.Empty; }
        }



        #endregion

        #region methods

        public void ResetLayout()
        {
            _childInfoDic.Clear();
        }

        protected virtual bool getContinueMeasure(int currentIdx, Size availableSize)
        {
            return true;
        }

        //protected abstract Size getPanelSize(int lastItemIdx);
        protected abstract Size estimatePanelExtent(Size availableSize);
        protected abstract void estimateVisibleItems(Size availableSize, Point offset, out int startIdx, out int endIdx);

        protected abstract Rect arrangeItem(Size finalSize, int currentIdx, out bool continueArrange);

        protected int startIdx, endIdx;
        public virtual Size Measure(Size availableSize)
        {
            bool continueMeasure = true;

            //int startIdx, endIdx;
            estimateVisibleItems(availableSize, _panel.Scroll.Offset, out startIdx, out endIdx);
            //Debug.WriteLine(String.Format("{0}-{1}", startIdx, endIdx));
            _panel.Generator.CleanUp(0, endIdx);

            int itemCount = getItemCount();
            for (int idx = 0; idx <= endIdx; idx++)
            {
                if (this[idx] == ChildInfo.Empty)
                {
                    ChildInfo ci = new ChildInfo()
                    {
                        DesiredSize = _generator.Measure(idx, availableSize),                        
                    };

                    _childInfoDic[idx] = ci;
                }

                continueMeasure = getContinueMeasure(idx, availableSize);

                if (!continueMeasure)
                {
                    endIdx = idx;
                    break;
                }
            }

            Extent = estimatePanelExtent(availableSize);
            return availableSize;
        }

        protected int getItemCount()
        {
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this._panel);
            int itemCount = itemsControl.Items.Count;
            return itemCount;
        }

        public Size Arrange(Size finalSize)
        {
            bool continueArrange = true;

            int itemCount = getItemCount();
            for (int idx = 0; idx <= endIdx; idx++)
            {
                ChildInfo ci = this[idx];
                if (ci != ChildInfo.Empty)
                {
                    if (!ci.ArrangedRect.HasValue) //Not arranged in ChildInfo.
                        ci.ArrangedRect = arrangeItem(finalSize, idx, out continueArrange);

                    if (idx >= startIdx)
                    {
                        _generator.Arrange(idx, ci.ArrangedRect.Value);                        
                    }
                }

                if (!continueArrange)
                    break;
            }

            return finalSize;
        }

        #endregion






    }
}
