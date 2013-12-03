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


    }
}
