using FileExplorer.UIEventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FileExplorer.WPF.BaseControls
{
    /// <summary>
    /// Shows a canvas with a ResizeThumb.
    /// </summary>
    public class ResizeItemAdorner : Adorner
    {
        #region Constructor


        public ResizeItemAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _resize = new ResizeDecorator() { IsHitTestVisible = true, Background = null };
            _resize.SetBinding(Control.WidthProperty, "Size.Width");
            _resize.SetBinding(Control.HeightProperty, "Size.Height");
            _resize.SetBinding(CenteredCanvas.XProperty, "Position.X");
            _resize.SetBinding(CenteredCanvas.YProperty, "Position.Y");

            _canvas = new CenteredCanvas() { Background = null };
            _canvas.Children.Add(_resize);

            this.AddLogicalChild(_canvas);
            this.AddVisualChild(_canvas);
            //IsHitTestVisible = false;
        }

        #endregion

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            _canvas.Measure(constraint);
            return constraint;
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            _canvas.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return finalSize;
        }

        public void SetTargetItem(IResizable selected)
        {
            _resize.DataContext = selected;
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ResizeItemAdorner)._resize.DataContext = e.NewValue;
        }

        #endregion

        #region Data

        private ResizeDecorator _resize;
        private CenteredCanvas _canvas;

        #endregion

        #region Public Properties

        protected override int VisualChildrenCount { get { return 1; } }

        protected override Visual GetVisualChild(int index)
        {
            return _canvas;
        }


        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
         DependencyProperty.Register("SelectedItem", typeof(object), typeof(SelectedItemsAdorner),
         new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedItemChanged)));

  
        #endregion
    }
}
