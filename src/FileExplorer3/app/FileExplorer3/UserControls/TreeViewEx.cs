using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UserControls
{
    //TreeViewEx and TreeViewItemEx

    public class TreeViewEx : TreeView
    {
        #region Cosntructor

        public TreeViewEx()
        {

        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }

    public class TreeViewItemEx : TreeViewItem
    {       
        #region Cosntructor

        public TreeViewItemEx()
        {
           
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

        //override hittestvi

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public bool IsItemUpdateRequired
        {
            get { return (bool)GetValue(IsItemUpdateRequiredProperty); }
            set { SetValue(IsItemUpdateRequiredProperty, value); }
        }

        public static readonly DependencyProperty IsItemUpdateRequiredProperty =
            DependencyProperty.Register("IsItemUpdateRequired", typeof(bool),
            typeof(TreeViewItemEx), new UIPropertyMetadata(false));

        public bool IsDraggingOver
        {
            get { return (bool)GetValue(IsDraggingOverProperty); }
            set { SetValue(IsDraggingOverProperty, value); }
        }
        
        public static readonly DependencyProperty IsDraggingOverProperty =
            DependencyProperty.Register("IsDraggingOver", typeof(bool), 
            typeof(TreeViewItemEx), new UIPropertyMetadata(false));

        #endregion
    }
}
