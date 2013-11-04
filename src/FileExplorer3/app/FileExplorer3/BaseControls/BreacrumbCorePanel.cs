///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// LYCJ (c) 2010 - http://www.quickzip.org/components                                                            //
// Release under MIT license.                                                                                   //
//                                                                                                               //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
{
    public class BreadcrumbCorePanel : Panel
    {
        private static DependencyProperty IsArrangedProperty = DependencyProperty.RegisterAttached("IsArranged", typeof(bool),
            typeof(BreadcrumbCorePanel), new PropertyMetadata(false));
        private static DependencyProperty ArrangedRectProperty = DependencyProperty.RegisterAttached("ArrangedRect", typeof(Rect),
            typeof(BreadcrumbCorePanel), new PropertyMetadata(new Rect(0, 0, 0, 0)));

        public static readonly RoutedEvent RemoveShadowItemEvent = EventManager.RegisterRoutedEvent("RemoveShadowItem",
           RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbCorePanel));

      

        public event RoutedEventHandler RemoveShadowItem
        {
            add { AddHandler(RemoveShadowItemEvent, value); }
            remove { RemoveHandler(RemoveShadowItemEvent, value); }
        }

        private Size lastfinalSize;

        //private bool IsPopupPanel()
        //{
        //    var lbParent = findParentControl();
        //    return lbParent is BreadcrumbOverflowPanel;
        //}


        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);
            if (Children.Count > 0)
            {
                //if (IsPopupPanel())
                //{
                //    //Measure as vertical stack.
                //    double maxWidth = 0;
                //    double desiredHeight = 0;
                //    for (int i = 0; i < Math.Min(GetLastNonVisibleIndex()+1, Children.Count); i++) //First to last
                //    {
                //        UIElement item = Children[i] as UIElement;                        
                //        if (item != null)
                //        {
                //            item.Measure(availableSize);
                //            maxWidth = Math.Max(maxWidth, item.DesiredSize.Width);
                //            desiredHeight += item.DesiredSize.Height;
                //        }
                //    }
                //    return new Size(maxWidth, desiredHeight);
                //}
                //else
                {
                    //Measure as horizontal stack, right to left.
                    double availableWidth = availableSize.Width;
                    for (int i = Children.Count - 1; i >= 0; i--) //Allocate from last to first
                    {
                        var current = Children[i];
                        current.Measure(new Size(availableWidth, availableSize.Height));
                        availableWidth -= current.DesiredSize.Width;
                    }
                    return availableSize;
                }
            }
            return new Size(0, 0);

        }

        #region AttachedProperties

        //private ItemsControl findParentControl()
        //{
        //    return UITools.FindAncestor<ItemsControl>(this);
        //}

        private BreadcrumbCore findBreadcrumbCore()
        {
            var bcore = UITools.FindAncestor<BreadcrumbCore>(this);
            //if (bcore == null)
            //    bcore = UITools.FindAncestor<BreadcrumbCore>(
            //        UITools.FindLogicalAncestor<DropDown>(UITools.FindAncestor<ItemsPresenter>(this)));
            //if (bcore == null)
            //    bcore = UITools.FindAncestor<BreadcrumbCore>(
            //        UITools.FindLogicalAncestor<DropDown>(findParentControl()));
            return bcore;
        }

        private int GetLastNonVisibleIndex()
        {
            var bcore = findBreadcrumbCore();
            return bcore == null ? -1 : bcore.LastNonVisible;
        }


        public void SetLastNonVisibleIndex(int value)
        {
            var bcore = findBreadcrumbCore();
            if (bcore != null)
                bcore.SetValue(BreadcrumbCore.LastNonVisibleIndexProperty, value);
        }



        private bool GetIsArranged(UIElement element)
        {
            return (bool)element.GetValue(IsArrangedProperty);
        }

        public void SetIsArranged(UIElement element, bool value)
        {
            element.SetValue(IsArrangedProperty, value);
        }

        private Rect GetArrangedRect(UIElement element)
        {
            return (Rect)element.GetValue(ArrangedRectProperty);
        }

        public void SetArrangedRect(UIElement element, Rect value)
        {
            element.SetValue(ArrangedRectProperty, value);
            if (element is BreadcrumbItem)
                (element as BreadcrumbItem).IsItemVisible = !value.Equals(new Rect(0, 0, 0, 0));
        }
        #endregion

        private Rect[] arrangeChildren(Size availableSize)
        {
            Rect[] retVal = new Rect[Children.Count];

            //if (IsPopupPanel())
            //{
            //    double curY = 0;

            //    for (int i = 0; i < Math.Min(GetLastNonVisibleIndex()+1, Children.Count); i++) //First to last
            //    {

            //        UIElement item = Children[i] as UIElement;
            //        if (item != null)
            //        {
            //            retVal[i] = new Rect(0, curY, availableSize.Width, item.DesiredSize.Height);
            //            curY += item.DesiredSize.Height;
            //        }
            //        else retVal[i] = new Rect(0, 0, 0, 0);
            //    }
            //}
            //else
            {
                double curX = availableSize.Width;
                for (int i = Children.Count - 1; i >= 0; i--) //Allocate from last to first
                {
                    var current = Children[i];
                    if (curX > 0)
                    {
                        double startPos = curX - current.DesiredSize.Width;

                        if (curX > current.DesiredSize.Width)
                        {
                            retVal[i] = new Rect(startPos,
                                (availableSize.Height - current.DesiredSize.Height) / 2,
                                current.DesiredSize.Width, current.DesiredSize.Height);
                            curX -= current.DesiredSize.Width;
                        }
                        else //Not enough space to allocate current, recalculate the retVal
                        {
                            retVal[i] = new Rect(0, 0, 0, 0);
                            for (int j = i; j < Children.Count; j++)
                                retVal[j].Offset(-curX, 0);
                            curX = 0;
                            SetLastNonVisibleIndex(i);
                        }
                    }
                    else retVal[i] = new Rect(0, 0, 0, 0);
                }
            }
            return retVal;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            lastfinalSize = finalSize;
            var rects = arrangeChildren(finalSize);
            //if (IsPopupPanel())
            //{
            //    for (int i = 0; i < this.Children.Count; i++)
            //    {
            //        Children[i].Arrange(rects[i]);
            //    }
            //}
            //else
                for (int i = 0; i < this.Children.Count; i++)
                {
                    SetArrangedRect(Children[i], rects[i]);
                    Children[i].Arrange(rects[i]);
                    SetIsArranged(Children[i], true);
                }
            return finalSize;

        }
    }
}
