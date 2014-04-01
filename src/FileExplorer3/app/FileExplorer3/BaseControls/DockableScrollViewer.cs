using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.BaseControls
{

    public class DockableContent : ContentControl
    {
        public DockableContent()
        {
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            //Visibility = false;
        }
        public static readonly DependencyProperty IsResizebleProperty =
         DependencyProperty.Register("IsResizable", typeof(bool), typeof(DockableContent), new PropertyMetadata(false));
        public bool IsResizable
        {
            get { return (bool)GetValue(IsResizebleProperty); }
            set { SetValue(IsResizebleProperty, value); }
        }
    }

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
            DependencyProperty.Register("TopContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent TopContent
        {
            get { return (DockableContent)GetValue(TopContentProperty); }
            set { SetValue(TopContentProperty, value); }
        }

        public static readonly DependencyProperty RightContentProperty =
           DependencyProperty.Register("RightContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent RightContent
        {
            get { return (DockableContent)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public static readonly DependencyProperty BottomContentProperty =
           DependencyProperty.Register("BottomContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent BottomContent
        {
            get { return (DockableContent)GetValue(BottomContentProperty); }
            set { SetValue(BottomContentProperty, value); }
        }

        public static readonly DependencyProperty LeftContentProperty =
           DependencyProperty.Register("LeftContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent LeftContent
        {
            get { return (DockableContent)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }


        public static readonly DependencyProperty OuterTopContentProperty =
           DependencyProperty.Register("OuterTopContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent OuterTopContent
        {
            get { return (DockableContent)GetValue(OuterTopContentProperty); }
            set { SetValue(OuterTopContentProperty, value); }
        }

        public static readonly DependencyProperty OuterRightContentProperty =
           DependencyProperty.Register("OuterRightContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent OuterRightContent
        {
            get { return (DockableContent)GetValue(OuterRightContentProperty); }
            set { SetValue(OuterRightContentProperty, value); }
        }

        public static readonly DependencyProperty OuterBottomContentProperty =
           DependencyProperty.Register("OuterBottomContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent OuterBottomContent
        {
            get { return (DockableContent)GetValue(OuterBottomContentProperty); }
            set { SetValue(OuterBottomContentProperty, value); }
        }

        public static readonly DependencyProperty OuterLeftContentProperty =
           DependencyProperty.Register("OuterLeftContent", typeof(DockableContent), typeof(DockableScrollViewer));
        public DockableContent OuterLeftContent
        {
            get { return (DockableContent)GetValue(OuterLeftContentProperty); }
            set { SetValue(OuterLeftContentProperty, value); }
        }


        #endregion
    }
}
