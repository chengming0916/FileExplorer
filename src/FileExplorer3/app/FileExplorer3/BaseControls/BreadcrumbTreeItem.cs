using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.BaseControls
{
    public class BreadcrumbTreeItem : TreeViewItem
    {
        #region Constructor

        static BreadcrumbTreeItem()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbTreeItem),
            //    new FrameworkPropertyMetadata(typeof(BreadcrumbTreeItem)));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbTreeItem();
        }
        
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
