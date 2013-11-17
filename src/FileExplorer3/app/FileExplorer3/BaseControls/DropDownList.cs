using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.BaseControls
{
    public class DropDownList : ComboBox
    {
        #region Constructor

        static DropDownList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownList),
                new System.Windows.FrameworkPropertyMetadata(typeof(DropDownList)));
        }
        
        #endregion

        #region Methods
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        public static readonly DependencyProperty HeaderProperty = 
            HeaderedContentControl.HeaderProperty.AddOwner(typeof(DropDownList));

        public object Header { get { return GetValue(HeaderProperty); } set { SetValue(HeaderProperty, value); } }
        
        #endregion

    }
}
