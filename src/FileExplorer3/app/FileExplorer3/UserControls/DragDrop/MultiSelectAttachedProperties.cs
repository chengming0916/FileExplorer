using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Utils;

namespace FileExplorer.Defines
{
 

    public static partial class AttachedProperties
    {
        private static DependencyProperty SelectionAdornerProperty =
            DependencyProperty.RegisterAttached("SelectionAdorner", typeof(SelectionAdorner), typeof(AttachedProperties));

        public static SelectionAdorner GetSelectionAdorner(DependencyObject target)
        {
            return (SelectionAdorner)target.GetValue(SelectionAdornerProperty);
        }

        public static void SetSelectionAdorner(DependencyObject target, SelectionAdorner value)
        {
            target.SetValue(SelectionAdornerProperty, value);
        }

        private static DependencyProperty LastScrollContentPresenterProperty =
           DependencyProperty.RegisterAttached("LastScrollContentPresenter", typeof(ScrollContentPresenter), typeof(AttachedProperties));

        public static ScrollContentPresenter GetLastScrollContentPresenter(DependencyObject target)
        {
            return (ScrollContentPresenter)target.GetValue(LastScrollContentPresenterProperty);
        }

        public static void SetLastScrollContentPresenter(DependencyObject target, ScrollContentPresenter value)
        {
            target.SetValue(LastScrollContentPresenterProperty, value);
        }

    }
}
