﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.BaseControls
{
    public class OverflowableStackPanel : StackPanel
    {
        #region Constructor

        #endregion

        #region Methods

        private double getWH(Size size, Orientation orientation)
        {
            return orientation == Orientation.Horizontal ? size.Width : size.Height;
        }
    
        protected override Size MeasureOverride(Size constraint)
        {
            var items = InternalChildren.Cast<UIElement>();

            overflowableWH = 0;
            nonoverflowableWH = 0;
            int overflowCount = 0;

            foreach (var item in items)
            {
                item.Measure(constraint);
                if (GetCanOverflow(item))
                    overflowableWH += getWH(item.DesiredSize, Orientation);
                else nonoverflowableWH += getWH(item.DesiredSize, Orientation);
            }

            foreach (var ele in items.Reverse())
            {
                if (GetCanOverflow(ele))
                    if (overflowableWH + nonoverflowableWH > getWH(constraint, Orientation))
                    {
                        overflowCount += 1;
                        SetIsOverflow(ele, true);
                        overflowableWH -= getWH(ele.DesiredSize, Orientation);
                    }
                    else SetIsOverflow(ele, false);
            }

            SetValue(OverflowItemCountProperty, overflowCount);

            return Orientation == Orientation.Horizontal ?
                    new Size(overflowableWH + nonoverflowableWH, constraint.Height) :
                    new Size(constraint.Width, overflowableWH + nonoverflowableWH);

        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var items = InternalChildren.Cast<UIElement>();
            if (Orientation == Orientation.Horizontal)
            {
                double curX = 0;
                foreach (var item in items)
                {
                    if (!GetCanOverflow(item) || !GetIsOverflow(item)) //Not overflowable or not set overflow
                    {
                        item.Arrange(new Rect(curX, 0, item.DesiredSize.Width, arrangeSize.Height));
                        curX += item.DesiredSize.Width;
                    }
                    else item.Arrange(new Rect(0, 0, 0, 0));
                    
                }
                return arrangeSize;
            }
            else
                return base.ArrangeOverride(arrangeSize);

            
        }

        #endregion

        #region Data

        double overflowableWH = 0;
        double nonoverflowableWH = 0;

        #endregion

        #region Public Properties

        public static DependencyProperty OverflowItemCountProperty = DependencyProperty.Register("OverflowItemCount", typeof(int),
            typeof(OverflowableStackPanel), new PropertyMetadata(0));

        public int OverflowItemCount
        {
            get { return (int)GetValue(OverflowItemCountProperty); }
            set { SetValue(OverflowItemCountProperty, value); }
        }

        public static DependencyProperty CanOverflowProperty = DependencyProperty.RegisterAttached("CanOverflow", typeof(bool),
            typeof(OverflowableStackPanel), new UIPropertyMetadata(false));

        public static bool GetCanOverflow(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanOverflowProperty);
        }

        public static void SetCanOverflow(DependencyObject obj, bool value)
        {
            obj.SetValue(CanOverflowProperty, value);
        }

        public static DependencyProperty IsOverflowProperty = DependencyProperty.RegisterAttached("IsOverflow", typeof(bool),
           typeof(OverflowableStackPanel), new UIPropertyMetadata(false));

        public static bool GetIsOverflow(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsOverflowProperty);
        }

        public static void SetIsOverflow(DependencyObject obj, bool value)
        {
            obj.SetValue(IsOverflowProperty, value);
        }

        #endregion
    }
}
