using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.BaseControls;

namespace FileExplorer.Defines
{
    public static class AttachedProperties
    {
        public static readonly DependencyProperty DragAdornerProperty =
          DependencyProperty.RegisterAttached("DragAdorner", typeof(DragAdorner), typeof(AttachedProperties), new UIPropertyMetadata(null));

        public static readonly Point InvalidPoint = new Point(double.NaN, double.NaN);

        private static DependencyProperty StartPositionProperty =
          DependencyProperty.RegisterAttached("StartPosition", typeof(Point), typeof(AttachedProperties), 
          new PropertyMetadata(InvalidPoint));

        public static DependencyProperty IsDraggingProperty =
          DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(AttachedProperties));

        public static DependencyProperty EnableDragProperty =
           DependencyProperty.RegisterAttached("EnableDrag", typeof(bool), typeof(AttachedProperties));

        public static DependencyProperty EnableDropProperty =
            DependencyProperty.RegisterAttached("EnableDrop", typeof(bool), typeof(AttachedProperties));


        public static readonly DependencyProperty DragItemTemplateProperty =
            DependencyProperty.RegisterAttached("DragItemTemplate", typeof(DataTemplate), typeof(AttachedProperties),
            new UIPropertyMetadata(null));



        public static DragAdorner GetDragAdorner(DependencyObject obj)
        {
            return (DragAdorner)obj.GetValue(DragAdornerProperty);
        }

        public static void SetDragAdorner(DependencyObject obj, DragAdorner value)
        {
            obj.SetValue(DragAdornerProperty, value);
        }

        public static bool IsValidPosition(this Point point)
        {
            return point.X != double.NaN && point.Y != double.NaN;
        }

        public static Point GetStartPosition(DependencyObject obj)
        {
            return (Point)obj.GetValue(StartPositionProperty);
        }

        public static void SetStartPosition(DependencyObject obj, Point value)
        {
            obj.SetValue(StartPositionProperty, value);
        }

        public static bool GetIsDragging(DependencyObject target)
        {
            return (bool)target.GetValue(IsDraggingProperty);
        }

        public static void SetIsDragging(DependencyObject target, bool value)
        {
            target.SetValue(IsDraggingProperty, value);
        }

        public static bool GetEnableDrag(DependencyObject target)
        {
            return (bool)target.GetValue(EnableDragProperty);
        }

        public static void SetEnableDrag(DependencyObject target, bool value)
        {
            target.SetValue(EnableDragProperty, value);
        }

        public static bool GetEnableDrop(DependencyObject target)
        {
            return (bool)target.GetValue(EnableDropProperty);
        }

        public static void SetEnableDrop(DependencyObject target, bool value)
        {
            target.SetValue(EnableDropProperty, value);
        }

        public static DataTemplate GetDragItemTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(DragItemTemplateProperty);
        }

        public static void SetDragItemTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(DragItemTemplateProperty, value);
        }

    }
}
