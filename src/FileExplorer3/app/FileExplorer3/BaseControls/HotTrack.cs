using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileExplorer.BaseControls
{
    public class HotTrack : ContentControl
    {
        #region Constructor
        static HotTrack()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HotTrack),
                new FrameworkPropertyMetadata(typeof(HotTrack)));
        }
        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            VisualStateManager.GoToState(this, "Normal", true);

        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            VisualStateManager.GoToState(this, "MouseOver", true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
    }
}
