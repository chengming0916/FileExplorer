using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.BaseControls;
using FileExplorer.UserControls;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Defines
{
    public static partial class AttachedProperties
    {
        #region DragAdorner

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


        #endregion

        #region StartDraggingItem

        public static DependencyProperty StartDraggingItemProperty =
          DependencyProperty.RegisterAttached("StartDraggingItem", typeof(Control), typeof(AttachedProperties));

        public static Control GetStartDraggingItem(DependencyObject target)
        {
            return (Control)target.GetValue(StartDraggingItemProperty);
        }

        public static void SetStartDraggingItem(DependencyObject target, Control value)
        {
            target.SetValue(StartDraggingItemProperty, value);
        }
        #endregion

        #region DragItemTemplate
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
        #endregion

        #region IsDragging

        public static DependencyProperty IsDraggingProperty =
       DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));


        public static bool GetIsDragging(DependencyObject target)
        {
            return (bool)target.GetValue(IsDraggingProperty);
        }

        public static void SetIsDragging(DependencyObject target, bool value)
        {
            target.SetValue(IsDraggingProperty, value);
        }

        #endregion

        #region IsDraggingOver

        public static DependencyProperty IsDraggingOverProperty =
       DependencyProperty.RegisterAttached("IsDraggingOver", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));


        public static bool GetIsDraggingOver(DependencyObject target)
        {
            return (bool)target.GetValue(IsDraggingOverProperty);
        }

        public static void SetIsDraggingOver(DependencyObject target, bool value)
        {
            target.SetValue(IsDraggingOverProperty, value);
        }

        #endregion

        #region DraggingOverItem

        public static DependencyProperty DraggingOverItemProperty =
       DependencyProperty.RegisterAttached("DraggingOverItem", typeof(FrameworkElement), typeof(AttachedProperties),
       new PropertyMetadata(null, OnLastDraggingOverPropertyChange));

        public static void OnLastDraggingOverPropertyChange
            (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                SetIsDraggingOver(e.OldValue as DependencyObject, false);
            if (e.NewValue != null)
                SetIsDraggingOver(e.NewValue as DependencyObject, true);
        }

        public static object GetDraggingOverItem(DependencyObject target)
        {
            return (FrameworkElement)target.GetValue(DraggingOverItemProperty);
        }

        public static void SetDraggingOverItem(DependencyObject target, FrameworkElement value)
        {
            target.SetValue(DraggingOverItemProperty, value);
        }

        #endregion

        #region PreviousDraggables

        public static DependencyProperty SelectedDraggablesProperty =
       DependencyProperty.RegisterAttached("SelectedDraggables", typeof(IEnumerable<IDraggable>),
       typeof(AttachedProperties), new PropertyMetadata(null));


        public static IEnumerable<IDraggable> GetSelectedDraggables(DependencyObject target)
        {
            return (IEnumerable<IDraggable>)target.GetValue(SelectedDraggablesProperty);
        }

        public static void SetSelectedDraggables(DependencyObject target, IEnumerable<IDraggable> value)
        {
            target.SetValue(SelectedDraggablesProperty, value);
        }
        #endregion

    }
}
