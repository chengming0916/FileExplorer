using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.BaseControls;

namespace FileExplorer.Defines
{
    public static partial class AttachedProperties
    {
        public static readonly DependencyProperty DragAdornerProperty =
          DependencyProperty.RegisterAttached("DragAdorner", typeof(DragAdorner), typeof(AttachedProperties), new UIPropertyMetadata(null));

        public static DragAdorner GetDragAdorner(DependencyObject obj)
        {
            return (DragAdorner)obj.GetValue(DragAdornerProperty);
        }

        public static void SetDragAdorner(DependencyObject obj, DragAdorner value)
        {
            obj.SetValue(DragAdornerProperty, value);
        }



        public static readonly Point InvalidPoint = new Point(double.NaN, double.NaN);

        private static DependencyProperty StartPositionProperty =
          DependencyProperty.RegisterAttached("StartPosition", typeof(Point), typeof(AttachedProperties), 
          new PropertyMetadata(InvalidPoint));

        public static Point GetStartPosition(DependencyObject obj)
        {
            return (Point)obj.GetValue(StartPositionProperty);
        }

        public static void SetStartPosition(DependencyObject obj, Point value)
        {
            obj.SetValue(StartPositionProperty, value);
        }

        public static Point GetStartPlusStartScrollbarPosition(DependencyObject obj)
        {
            var startPositon = (Point)obj.GetValue(StartPositionProperty);
            var startScrollPositon = (Point)obj.GetValue(StartScrollbarPositionProperty);
            if (startScrollPositon == InvalidPoint)
                return startPositon;
            else return new Point(startPositon.X + startScrollPositon.X,
                startPositon.Y + startScrollPositon.Y);
        }

        private static DependencyProperty StartScrollbarPositionProperty =
         DependencyProperty.RegisterAttached("StartScrollbarPosition", typeof(Point), typeof(AttachedProperties),
         new PropertyMetadata(InvalidPoint));

        public static Point GetStartScrollbarPosition(DependencyObject obj)
        {
            return (Point)obj.GetValue(StartScrollbarPositionProperty);
        }

        public static void SetStartScrollbarPosition(DependencyObject obj, Point value)
        {
            obj.SetValue(StartScrollbarPositionProperty, value);
        }


        public static DependencyProperty IsDraggingProperty =
          DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(AttachedProperties));


        public static bool GetIsDragging(DependencyObject target)
        {
            return (bool)target.GetValue(IsDraggingProperty);
        }

        public static void SetIsDragging(DependencyObject target, bool value)
        {
            target.SetValue(IsDraggingProperty, value);
        }

        #region EnableDrag / Drop   

        public static DependencyProperty EnableDragProperty =
           DependencyProperty.RegisterAttached("EnableDrag", typeof(bool), typeof(AttachedProperties));

        public static bool GetEnableDrag(DependencyObject target)
        {
            return (bool)target.GetValue(EnableDragProperty);
        }

        public static void SetEnableDrag(DependencyObject target, bool value)
        {
            target.SetValue(EnableDragProperty, value);
        }


        public static DependencyProperty EnableDropProperty =
            DependencyProperty.RegisterAttached("EnableDrop", typeof(bool), typeof(AttachedProperties));


        public static bool GetEnableDrop(DependencyObject target)
        {
            return (bool)target.GetValue(EnableDropProperty);
        }

        public static void SetEnableDrop(DependencyObject target, bool value)
        {
            target.SetValue(EnableDropProperty, value);
        }

        #endregion

        public static readonly DependencyProperty DragItemTemplateProperty =
            DependencyProperty.RegisterAttached("DragItemTemplate", typeof(DataTemplate), typeof(AttachedProperties),
            new UIPropertyMetadata(null));


        public static DataTemplate GetDragItemTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(DragItemTemplateProperty);
        }

        public static void SetDragItemTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(DragItemTemplateProperty, value);
        }




    
        public static bool IsValidPosition(this Point point)
        {
            return point.X != double.NaN && point.Y != double.NaN;
        }

     




  


       

    }
}
