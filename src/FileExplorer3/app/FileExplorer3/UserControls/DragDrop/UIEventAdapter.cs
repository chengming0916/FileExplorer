using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FileExplorer.BaseControls;
using FileExplorer.Defines;

namespace FileExplorer.BaseControls
{

    public interface IUIEventAdapter
    {
        UIElement Control { get; }
        List<IUIEventProcessor> EventProcessors { get; }
        bool IsEnabled { get; set; }
    }

    public class UIEventAdapter : IUIEventAdapter
    {
        #region Constructor

        public UIEventAdapter(UIElement control, bool startIsEnabled = true, params IUIEventProcessor[] eventProcessors)
        {
            Control = control;
            IsEnabled = startIsEnabled;
            _eventProcessors = new List<IUIEventProcessor>(eventProcessors);
        }

        #endregion

        #region Methods

        private void setIsEnabled(bool value)
        {
            if (value != _isEnabled)
            {
                _isEnabled = value;

                if (_isEnabled)
                {
                    Control.PreviewMouseDown += Control_MouseDown;
                    Control.MouseMove += Control_MouseMove;
                    Control.MouseUp += Control_MouseUp;

                    Control.DragOver += Control_DragOver;
                    Control.DragEnter += Control_DragEnter;
                    Control.DragLeave += Control_DragLeave;
                    Control.Drop += Control_Drop;
                }
                else
                {
                    Control.PreviewMouseDown -= Control_MouseDown;
                    Control.MouseMove -= Control_MouseMove;
                    Control.MouseUp -= Control_MouseUp;

                    Control.DragOver -= Control_DragOver;
                    Control.DragEnter -= Control_DragEnter;
                    Control.DragLeave -= Control_DragLeave;
                    Control.Drop -= Control_Drop;
                }
            }
        }

        void Control_DragLeave(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                foreach (var p in _eventProcessors)
                    if (p.OnMouseDragLeave != null && p.OnMouseDragLeave.Handle(control, e, () => p.OnMouseDragLeave))
                        return;
            }
        }

        void Control_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                foreach (var p in _eventProcessors)
                    if (p.OnMouseDrop != null && p.OnMouseDrop.Handle(control, e, () => p.OnMouseDrop))
                        return;
            }
        }

        void Control_DragEnter(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                foreach (var p in _eventProcessors)
                    if (p.OnMouseDragEnter != null && p.OnMouseDragEnter.Handle(control, e, () => p.OnMouseDragEnter))
                        return;
            }
        }

        void Control_DragOver(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                foreach (var p in _eventProcessors)
                    if (p.OnMouseDragOver != null && p.OnMouseDragOver.Handle(control, e, () => p.OnMouseDragOver))
                        return;
            }
        }

        MouseButtonEventArgs _mouseDownEvent = null;
        void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownEvent = e;

            FrameworkElement control = sender as FrameworkElement;
            foreach (var p in _eventProcessors)
                if (p.OnMouseDown != null && p.OnMouseDown.Handle(control, e, () => p.OnMouseDown))
                    return;

            AttachedProperties.SetIsDragging(control, false);
            if (UITools.IsMouseOverSelectedItem(control))
                AttachedProperties.SetStartPosition(control, e.GetPosition(null));
            else AttachedProperties.SetStartPosition(control, AttachedProperties.InvalidPoint);
        }

        void Control_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;

            Point position = e.GetPosition(null);
            Point startPosition = AttachedProperties.GetStartPosition(control);
            if (startPosition.IsValidPosition())
            {
                foreach (var p in _eventProcessors)
                    if (p.OnMouseMove != null && p.OnMouseMove.Handle(control, e, () => p.OnMouseMove))
                        return;
                if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                    if (Math.Abs(position.X - startPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - startPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    AttachedProperties.SetIsDragging(control, true);
                    AttachedProperties.SetStartPosition(control, AttachedProperties.InvalidPoint);
                    Control_MouseDrag(sender, e);
                }
            }
        }        

        public void Control_MouseDrag(object sender, MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            foreach (var p in _eventProcessors)
                if (p.OnMouseDrag != null && p.OnMouseDrag.Handle(control, e, () => p.OnMouseDrag))
                {                    
                    Control_MouseUp(sender, 
                        _mouseDownEvent ?? new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
                    return;
                }
        }

        void Control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            AttachedProperties.SetIsDragging(control, false);

            foreach (var p in _eventProcessors)
                if (p.OnMouseUp != null && p.OnMouseUp.Handle(control, e, () => p.OnMouseUp))
                    return;
        }

        #endregion

        #region Data

        private List<IUIEventProcessor> _eventProcessors;
        private bool _isEnabled = false;

        #endregion

        #region Public Properties

        public bool IsEnabled { get { return _isEnabled; } set { setIsEnabled(value); } }
        public UIElement Control { get; private set; }
        public List<IUIEventProcessor> EventProcessors { get { return _eventProcessors; } }
        #endregion


    }





}
