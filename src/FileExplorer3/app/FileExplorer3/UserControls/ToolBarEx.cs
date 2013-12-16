using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UserControls
{
    public class ToolbarEx : ItemsControl
    {
        #region Constructor

        static ToolbarEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarEx), new FrameworkPropertyMetadata(typeof(ToolbarEx)));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolbarItemEx();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

    }

    public enum ToolbarItemType { Button, Range, Menu }

    public class ToolbarItemEx : ContentControl
    {
                #region Constructor
        
        #endregion

        #region Methods
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties



        public ToolbarItemType ContentType
        {
            get { return (ToolbarItemType)GetValue(ContentTypeProperty); }
            set { SetValue(ContentTypeProperty, value); }
        }
        
        public static readonly DependencyProperty ContentTypeProperty =
            DependencyProperty.Register("ContentType", typeof(ToolbarItemType), 
            typeof(ToolbarEx), new PropertyMetadata(ToolbarItemType.Button));        
        
        #endregion
    }

}
