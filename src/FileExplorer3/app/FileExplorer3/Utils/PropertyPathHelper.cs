using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace FileExplorer.Utils
{
    //Thomas Levesque - http://stackoverflow.com/questions/3577802/wpf-getting-a-property-value-from-a-binding-path
    public static class PropertyPathHelper
    {
        public static object GetValueFromPropertyInfo(object obj, string propertyPath)
        {
            Type type = obj.GetType();
            var pInfo = type.GetProperty(propertyPath);
            if (pInfo != null)
                return pInfo.GetValue(obj);
            return null;
        }

        public static object GetValue(object obj, string propertyPath)
        {
            Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            if (dispatcher == null)
                return GetValueFromPropertyInfo(obj, propertyPath);

            Binding binding = new Binding(propertyPath);
            binding.Mode = BindingMode.OneTime;
            binding.Source = obj;
            BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
            return _dummy.GetValue(Dummy.ValueProperty);


        }

        public static object GetValue(object obj, BindingBase binding)
        {
            return GetValue(obj, (binding as Binding).Path.Path);
        }



        private static readonly Dummy _dummy = new Dummy();

        private class Dummy : DependencyObject
        {
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(Dummy), new UIPropertyMetadata(null));
        }
    }
}
