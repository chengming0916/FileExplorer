﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections;

namespace FileExplorer.BaseControls
{
    public class DragAdorner : Adorner
    {

        public DragAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _canvas = new Canvas() { Background = null };

            //FrameworkElementFactory overlapPanelfactory = new FrameworkElementFactory(typeof(OverlappingPanel));
            //overlapPanelfactory.SetValue(OverlappingPanel.OverlapXProperty, 5);
            //overlapPanelfactory.SetValue(OverlappingPanel.OverlapYProperty, 5);
            _items = new ItemsControl()
                {
                    MaxHeight = 250,
                    MaxWidth = 1000,
                    Visibility = System.Windows.Visibility.Collapsed,
                    Opacity = 0.8,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center, 
            
                    //ItemsPanel = new ItemsPanelTemplate(overlapPanelfactory)
                };

            Canvas.SetTop(_items, -50);
            Canvas.SetLeft(_items, -50);

            this.ContextMenu = new ContextMenu() { PlacementTarget = _items };
            SetSupportedDragDropEffects(DragDropEffects.All);


            this.ContextMenu.AddHandler(MenuItem.ClickEvent, (RoutedEventHandler)((o, e) =>
            {
                MenuItem mi =  e.Source as MenuItem;
                if (mi.Tag is DragDropEffects)
                {
                    this.SetValue(DragDropEffectProperty, mi.Tag);
                    this.ContextMenu.StaysOpen = false;
                }
            }));

            _canvas.Children.Add(_items);

            this.AddLogicalChild(_canvas);
            this.AddVisualChild(_canvas);
        }


        #region Methods

        public void SetSupportedDragDropEffects(DragDropEffects effects, DragDropEffects defaultEffect = DragDropEffects.Copy)
        {
            ContextMenu.Items.Clear();
            foreach (var e in Enum.GetValues(typeof(DragDropEffects)))
            {
                DragDropEffects curEffect = (DragDropEffects)e;
                if (curEffect != DragDropEffects.None && effects.HasFlag(curEffect))
                {
                    var header = new TextBlock() { Text = curEffect.ToString() };
                    if (curEffect.Equals(defaultEffect))
                    {
                        header.FontWeight = FontWeights.Bold;
                        ContextMenu.Items.Insert(0, new MenuItem() { Tag = e, Header = header });
                    }
                    else ContextMenu.Items.Add(new MenuItem() { Tag = e, Header = header });
                }
            }
        }

        protected override int VisualChildrenCount { get { return 1; } }

        protected override Visual GetVisualChild(int index)
        {
            return _canvas;
        }

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

        public static void OnDraggingItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DragAdorner adorner = o as DragAdorner;
            if (adorner != null)
            {
                adorner._items.ItemsSource = e.NewValue as IEnumerable;
            }
        }

        public static void OnIsDraggingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DragAdorner adorner = o as DragAdorner;
            if (adorner != null)
            {
                //Debug.WriteLine(e.NewValue);
                if ((bool)e.NewValue)
                    adorner._items.Visibility = Visibility.Visible;
                else adorner._items.Visibility = Visibility.Collapsed;
            }
        }

        public static void OnIsContextMenuVisibleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //Debug.Write(e.NewValue);
            // DragAdorner adorner = o as DragAdorner;
            // if (adorner != null)
            // {
            //     Debug.WriteLine(adorner._canvas.ContextMenu.Visibility);
            // }
            // //DragAdorner adorner = o as DragAdorner;
            // //if (adorner != null)
            // //{
                 
                 
                 
            // //    adorner._canvas.ContextMenu.Visibility = Visibility.Visible;
            // //}
        }

        public static void OnPointerPositionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DragAdorner adorner = o as DragAdorner;
            if (adorner != null)
            {
                Point newPos = (Point)e.NewValue;
                Canvas.SetLeft(adorner._items, newPos.X + 10);
                Canvas.SetTop(adorner._items, newPos.Y + 10);
            }
        }

        public static void OnDraggingItemTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DragAdorner adorner = o as DragAdorner;
            if (adorner != null)
            {
                adorner._items.ItemTemplate = e.NewValue as DataTemplate;
            }
        }

        

        #endregion

        #region Data

        private Canvas _canvas;
        private ItemsControl _items;        

        #endregion

        #region Public Properties

        public DragDropEffects DragDropEffect
        {
            get { return (DragDropEffects)GetValue(DragDropEffectProperty); }
            set { SetValue(DragDropEffectProperty, value); }
        }
        
        public static readonly DependencyProperty DragDropEffectProperty =
            DependencyProperty.Register("DragDropEffect", typeof(DragDropEffects), typeof(DragAdorner),
            new UIPropertyMetadata(DragDropEffects.None));




        public IEnumerable DraggingItems
        {
            get { return (IEnumerable)GetValue(DraggingItemsProperty); }
            set { SetValue(DraggingItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DraggingItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraggingItemsProperty =
            DependencyProperty.Register("DraggingItems", typeof(IEnumerable), typeof(DragAdorner),
            new UIPropertyMetadata(new System.Collections.ArrayList(), new PropertyChangedCallback(OnDraggingItemsChanged)));




        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDragging.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(DragAdorner), new UIPropertyMetadata(false,
             new PropertyChangedCallback(OnIsDraggingChanged)));




        public bool IsContextMenuVisible
        {
            get { return (bool)GetValue(IsContextMenuVisibleProperty); }
            set { SetValue(IsContextMenuVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsContextMenuVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsContextMenuVisibleProperty =
            DependencyProperty.Register("IsContextMenuVisible", typeof(bool), typeof(DragAdorner), new UIPropertyMetadata(false,
                 new PropertyChangedCallback(OnIsContextMenuVisibleChanged)));



        public Point PointerPosition
        {
            get { return (Point)GetValue(PointerPositionProperty); }
            set { SetValue(PointerPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointerPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointerPositionProperty =
            DependencyProperty.Register("PointerPosition", typeof(Point), typeof(DragAdorner), 
            new UIPropertyMetadata(new Point(-50, -50), new PropertyChangedCallback(OnPointerPositionChanged)));




        public DataTemplate DraggingItemTemplate
        {
            get { return (DataTemplate)GetValue(DraggingItemTemplateProperty); }
            set { SetValue(DraggingItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DraggingItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraggingItemTemplateProperty =
            DependencyProperty.Register("DraggingItemTemplate", typeof(DataTemplate), typeof(DragAdorner), 
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnDraggingItemTemplateChanged)));

        


        #endregion



    }
}
