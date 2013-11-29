using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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


        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty PlacementTargetProperty =
            Popup.PlacementTargetProperty.AddOwner(typeof(DropDownList));

        public PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty PlacementProperty =
            Popup.PlacementProperty.AddOwner(typeof(DropDownList));

        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
            Popup.HorizontalOffsetProperty.AddOwner(typeof(DropDownList));


        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
           Popup.VerticalOffsetProperty.AddOwner(typeof(DropDownList));

        #endregion

    }
}
