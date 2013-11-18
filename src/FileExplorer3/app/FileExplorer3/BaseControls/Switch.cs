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
    /// Display ContentOn or ContentOff depends on whether IsSwitchOn is true.
    /// </summary>
    public class Switch : HeaderedContentControl
    {
        #region Constructor

        static Switch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Switch),
                new FrameworkPropertyMetadata(typeof(Switch)));
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public bool IsSwitchOn
        {
            get { return (bool)GetValue(IsSwitchOnProperty); }
            set { SetValue(IsSwitchOnProperty, value); }
        }

        public static readonly DependencyProperty IsSwitchOnProperty =
            DependencyProperty.Register("IsSwitchOn", typeof(bool),
            typeof(Switch), new UIPropertyMetadata(true));

        public object ContentOn
        {
            get { return (object)GetValue(ContentOnProperty); }
            set { SetValue(ContentOnProperty, value); }
        }

        public static readonly DependencyProperty ContentOnProperty =
            DependencyProperty.Register("ContentOn", typeof(object),
            typeof(Switch), new UIPropertyMetadata(null));

        public object ContentOff
        {
            get { return (object)GetValue(ContentOffProperty); }
            set { SetValue(ContentOffProperty, value); }
        }

        public static readonly DependencyProperty ContentOffProperty =
            DependencyProperty.Register("ContentOff", typeof(object),
            typeof(Switch), new UIPropertyMetadata(null));

        #endregion
    }
}
