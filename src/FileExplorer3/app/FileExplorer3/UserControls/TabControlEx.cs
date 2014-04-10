using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UserControls
{
    public class TabControlEx : TabControl
    {
        #region Constructors

        static TabControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControlEx),
                new FrameworkPropertyMetadata(typeof(TabControlEx)));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TabItemEx();
            //return base.GetContainerForItemOverride();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }

    public class TabItemEx: TabItem
    {
        #region Constructors

        static TabItemEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabItemEx),
                new FrameworkPropertyMetadata(typeof(TabItemEx)));
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }
}
