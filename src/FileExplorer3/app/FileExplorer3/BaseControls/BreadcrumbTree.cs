using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.BaseControls
{
    public class BreadcrumbTree : TreeView
    {
        #region Constructor

        static BreadcrumbTree()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbTree),
            //    new FrameworkPropertyMetadata(typeof(BreadcrumbTree)));
        }

        #endregion

        #region Methods 

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbTreeItem() { IsChildSelected = true };
        }

        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        public static readonly DependencyProperty OverflowedItemContainerStyleProperty =
                   DependencyProperty.Register("OverflowedItemContainerStyle", typeof(Style), typeof(BreadcrumbTree));

        public Style OverflowedItemContainerStyle
        {
            get { return (Style)GetValue(OverflowedItemContainerStyleProperty); }
            set { SetValue(OverflowedItemContainerStyleProperty, value); }
        }
        
        #endregion
    }
}
