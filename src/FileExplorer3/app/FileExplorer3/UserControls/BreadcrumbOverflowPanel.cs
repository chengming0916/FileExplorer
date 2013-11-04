using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UserControls
{
    public class BreadcrumbOverflowPanel : ItemsControl
    {
        #region Constructor

        static BreadcrumbOverflowPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbOverflowPanel),
             new FrameworkPropertyMetadata(typeof(BreadcrumbOverflowPanel)));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbItem() { HeaderTemplate = this.HeaderTemplate, ShowToggle = false };
        }

        #endregion

        #region Data



        #endregion

        #region Public Properties

        public static readonly DependencyProperty HeaderTemplateProperty =
            HeaderedItemsControl.HeaderTemplateProperty.AddOwner(typeof(BreadcrumbOverflowPanel));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #endregion
    }
}
