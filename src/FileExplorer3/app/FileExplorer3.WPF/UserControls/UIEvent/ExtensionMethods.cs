using FileExplorer.Defines;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.WPF
{


    public static partial class ExtensionMethods
    {
        private class LBSelectable : ISelectable
        {
            private ListBoxItem _lbItem;
            public LBSelectable(ListBoxItem lbItem)
            {
                _lbItem = lbItem;
            }
            public bool IsSelected
            {
                get
                {
                    return (bool)_lbItem.GetValue(ListBoxItem.IsSelectedProperty);
                }
                set
                {
                    _lbItem.SetValue(ListBoxItem.IsSelectedProperty, value);
                }
            }
        }
        public static ISelectable ToISelectable(object viewModel, ListBoxItem lbm)
        {
            if (viewModel is ISelectable)
                return viewModel as ISelectable;
            else
                if (lbm.DataContext is ISelectable)
                    return lbm.DataContext as ISelectable;
                else return new LBSelectable(lbm);
        }

        public static DragDropEffects ToDragDropEffects(this DragDropResult ddr)
        {
            return (DragDropEffects)(int)ddr;
        }
    }
}
