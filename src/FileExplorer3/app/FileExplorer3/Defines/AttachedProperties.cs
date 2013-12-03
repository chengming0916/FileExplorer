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
