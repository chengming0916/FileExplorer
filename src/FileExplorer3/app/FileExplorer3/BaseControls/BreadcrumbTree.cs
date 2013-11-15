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
            return new BreadcrumbTreeItem();
        }
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
