using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
{
    public class BreadcrumbExpander : DropDownList
    {
        #region Constructor

        static BreadcrumbExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbExpander),
                new FrameworkPropertyMetadata(typeof(BreadcrumbExpander)));
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
