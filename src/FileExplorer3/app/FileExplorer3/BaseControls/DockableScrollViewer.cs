using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.BaseControls
{
    /// <summary>
    /// ScrollViewer that contains Top/Right/Bottom/LeftContent so you can insert fixed content to it.
    /// </summary>
    public class DockableScrollViewer : ScrollViewer
    {

        #region Constructors

        static DockableScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockableScrollViewer),
                new FrameworkPropertyMetadata(typeof(DockableScrollViewer)));
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public static readonly DependencyProperty TopContentProperty =
            DependencyProperty.Register("TopContent", typeof(object), typeof(DockableScrollViewer));
        public object TopContent
        {
            get { return (object)GetValue(TopContentProperty); }
            set { SetValue(TopContentProperty, value); }
        }

        public static readonly DependencyProperty RightContentProperty =
           DependencyProperty.Register("RightContent", typeof(object), typeof(DockableScrollViewer));
        public object RightContent
        {
            get { return (object)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public static readonly DependencyProperty BottomContentProperty =
           DependencyProperty.Register("BottomContent", typeof(object), typeof(DockableScrollViewer));
        public object BottomContent
        {
            get { return (object)GetValue(BottomContentProperty); }
            set { SetValue(BottomContentProperty, value); }
        }

        public static readonly DependencyProperty LeftContentProperty =
           DependencyProperty.Register("LeftContent", typeof(object), typeof(DockableScrollViewer));
        public object LeftContent
        {
            get { return (object)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }


        public static readonly DependencyProperty OuterTopContentProperty =
           DependencyProperty.Register("OuterTopContent", typeof(object), typeof(DockableScrollViewer));
        public object OuterTopContent
        {
            get { return (object)GetValue(OuterTopContentProperty); }
            set { SetValue(OuterTopContentProperty, value); }
        }

        public static readonly DependencyProperty OuterRightContentProperty =
           DependencyProperty.Register("OuterRightContent", typeof(object), typeof(DockableScrollViewer));
        public object OuterRightContent
        {
            get { return (object)GetValue(OuterRightContentProperty); }
            set { SetValue(OuterRightContentProperty, value); }
        }

        public static readonly DependencyProperty OuterBottomContentProperty =
           DependencyProperty.Register("OuterBottomContent", typeof(object), typeof(DockableScrollViewer));
        public object OuterBottomContent
        {
            get { return (object)GetValue(OuterBottomContentProperty); }
            set { SetValue(OuterBottomContentProperty, value); }
        }

        public static readonly DependencyProperty OuterLeftContentProperty =
           DependencyProperty.Register("OuterLeftContent", typeof(object), typeof(DockableScrollViewer));
        public object OuterLeftContent
        {
            get { return (object)GetValue(OuterLeftContentProperty); }
            set { SetValue(OuterLeftContentProperty, value); }
        }


        #endregion
    }
}
