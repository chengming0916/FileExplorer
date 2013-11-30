using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.Defines;

namespace FileExplorer.BaseControls
{
    public interface IRoutedEventHandler
    {
        bool Handle<T>(FrameworkElement control, RoutedEventArgs args, Expression<Func<T>> eventExpression);
    }

    public class DebugRoutedEventHandler : IRoutedEventHandler
    {
        public enum HandleType { printOrgSource, printSelector, prepareDataObject }

        

        public static DebugRoutedEventHandler PrintSourceDC = new DebugRoutedEventHandler(HandleType.printOrgSource);
        public static DebugRoutedEventHandler PrintSelectedDC = new DebugRoutedEventHandler(HandleType.printSelector);
        public static DebugRoutedEventHandler PrepareDrag = new DebugRoutedEventHandler(HandleType.prepareDataObject);

        public DebugRoutedEventHandler(HandleType handleType)
        {
            _handleType = handleType;
        }
        private HandleType _handleType;

        protected string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }

        public bool Handle<T>(FrameworkElement control, RoutedEventArgs args, Expression<Func<T>> eventExpression)
        {
            object value = args.OriginalSource is FrameworkElement ? (args.OriginalSource as FrameworkElement).DataContext : null;
            switch (_handleType)
            {
                case HandleType.prepareDataObject: 
                case HandleType.printSelector :
                    if (control is System.Windows.Controls.Primitives.Selector)            
                    value = (control as System.Windows.Controls.Primitives.Selector).SelectedValue;            
                    break;
            }            
            if (value == null) value = "";
            Debug.WriteLine(String.Format("{0} - {1} = {2}", control.Name, GetPropertyName(eventExpression), value));

            if (control is System.Windows.Controls.Primitives.Selector && _handleType == HandleType.prepareDataObject)
            {                
                DataObject obj = new DataObject(DataFormats.Text, value);
                //Debug.WriteLine(String.Format("{0} - {1} = {2}", control.Name, GetPropertyName(eventExpression), value));
                System.Windows.DragDrop.DoDragDrop(control, obj, DragDropEffects.All);
                return true;
            }
            else
            {                
                if (value == null) value = "";
                
            }
                
            return false;
        }
    }

    public class HideAdorner : IRoutedEventHandler
    {
        public bool Handle<T>(FrameworkElement control, RoutedEventArgs args, Expression<Func<T>> eventExpression)
        {
            DragAdorner dragAdorner = AttachedProperties.GetDragAdorner(control);
            if (dragAdorner != null)
                dragAdorner.IsDragging = false;
            return false;
        }
    }

    public class ShowAdorner : IRoutedEventHandler
    {
        public bool Handle<T>(FrameworkElement control, RoutedEventArgs args, Expression<Func<T>> eventExpression)
        {
            DragAdorner dragAdorner = AttachedProperties.GetDragAdorner(control);
            if (dragAdorner != null)
                dragAdorner.IsDragging = true;
            else if (Debugger.IsAttached)
                Debugger.Break(); //No DragAdorner.
            return false;
        }
    }




}
