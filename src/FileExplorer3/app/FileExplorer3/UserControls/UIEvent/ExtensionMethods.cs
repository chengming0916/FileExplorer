using FileExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileExplorer
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
        public static ISelectable ToISelectable(this ListBoxItem lbm)
        {
            if (lbm.DataContext is ISelectable)
                return lbm.DataContext as ISelectable;
            else return new LBSelectable(lbm);
        }
    }
}
