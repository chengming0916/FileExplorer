using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF
{
    public class VerticalStackPanelLayout : PanelLayoutBase
    {
        #region fields


        #endregion

        #region constructors


        public VerticalStackPanelLayout(IOCPanel panel, IItemGeneratorHelper generator)
            : base(panel, generator)
        {                  
        }

        #endregion

        #region events

        #endregion

        #region properties
        

        #endregion

        #region methods

        protected override bool getContinueMeasure(int currentIdx, Size availableSize)
        {
            return getPanelHeight(currentIdx) < availableSize.Height + _panel.Scroll.Offset.Y;
        }

        private ChildInfo measure(int idx, Size availableSize)
        {
            if (this[idx] == ChildInfo.Empty)
            {
                ChildInfo ci = new ChildInfo()
                {
                    DesiredSize = _generator.Measure(idx, availableSize)
                };

                _childInfoDic[idx] = ci;
            }
            return this[idx];
        }

        //private int findStartIdx(Size availableSize, Point scrollbarOffset, out int startIdx)
        //{
        //    topPos = 0;
        //    int itemCount = getItemCount();

        //    for (int idx = 0; idx < itemCount; idx++)
        //    {               
        //        ChildInfo ci = measure(idx, availableSize);
        //        if (topPos < scrollbarOffset.Y)                
        //            return idx;       
        //         topPos += measure(idx, availableSize).DesiredSize.Value.Height;
        //    }

        //    return itemCount -1;
        //}         
        Size? _extent;
        public override Size Measure(Size availableSize)
        {
            double topPos = 0;
            int itemCount = getItemCount();            

            startIdx = itemCount -1;
            for (int idx = 0; idx < itemCount; idx++)
            {               
                ChildInfo ci = measure(idx, availableSize);
                if (topPos >= _panel.Scroll.Offset.Y )                
                {
                    startIdx = idx;                    
                    break;
                }
                 topPos += ci.DesiredSize.Value.Height;
            }

            for (int idx = startIdx; idx < itemCount; idx++)
            {
                ChildInfo ci = measure(idx, availableSize);
                if (topPos > availableSize.Height + _panel.Scroll.Offset.Y)
                {
                    endIdx = idx;                   
                    break;
                }
                topPos += ci.DesiredSize.Value.Height;

            }

            startIdx -= 10;
            endIdx += 10;
            if (startIdx < 0)
                startIdx = 0;
            if (endIdx > itemCount - 1)
                endIdx = itemCount - 1;

            _panel.Generator.CleanUp(startIdx, endIdx);
            Extent = estimatePanelExtent(availableSize);
            return availableSize;
        }       

        protected override Size estimatePanelExtent(Size availableSize)
        {            
            int itemCount = getItemCount();
            return new Size(availableSize.Width, getPanelHeight(itemCount));
        }

        protected override void estimateVisibleItems(Size availableSize, Point offset, out int startIdx, out int endIdx)
        {
            startIdx = 0;
            double viewPortTop = availableSize.Height - offset.Y;
            double height = 0;
            int itemCount = endIdx = getItemCount();
            for (int idx = 0; idx < itemCount; idx++)
            {
                ChildInfo ci = this[idx];
                if (height < offset.Y)
                    startIdx = idx;
                height += ci == ChildInfo.Empty || !ci.DesiredSize.HasValue ? 0 : ci.DesiredSize.Value.Height;
                if (height > offset.Y + availableSize.Height)
                {
                    endIdx = idx;
                    break;
                }
            }

        }

        private double getPanelHeight(int lastItemIdx)
        {
            double defaultHeight = this[0].DesiredSize.Value.Height;
            double height = 0;

            for (int idx = 0; idx < lastItemIdx; idx++)
            {                
                ChildInfo ci = this[idx];
                height += ci == ChildInfo.Empty || !ci.DesiredSize.HasValue ? defaultHeight : ci.DesiredSize.Value.Height;
            }

            return height;
        }

        protected override Rect arrangeItem(Size finalSize, int currentIdx, out bool continueArrange)
        {
            Size desiredSize = new Size(finalSize.Width, this[currentIdx].DesiredSize.Value.Height);
            continueArrange = true;
            Rect retVal;

            if (currentIdx == 0)
                retVal = new Rect(new Point(0, 0), desiredSize);
            else
            {
                Rect previousRect = this[currentIdx-1].ArrangedRect.Value;
                retVal = new Rect(previousRect.BottomLeft, desiredSize);
                continueArrange = retVal.Height < finalSize.Height;
            }

            return retVal;
        }

        #endregion

    }
}
