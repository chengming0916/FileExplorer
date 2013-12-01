using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

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

        public UIEventAdapter(IScriptRunner runner, UIElement control, bool startIsEnabled = true, params IUIEventProcessor[] eventProcessors)
        {
            Control = control;
            IsEnabled = startIsEnabled;
            _eventProcessors = new List<IUIEventProcessor>(eventProcessors);
            _scriptRunner = runner;
        }

        #endregion

        #region Methods

        #region Static Helpers

        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }


        #endregion
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

        private bool execute(List<IUIEventProcessor> processors, Func<IUIEventProcessor, IScriptCommand> commandFunc,
            string eventName, object sender, RoutedEventArgs e)
        {
            UIParameterDic pd = new UIParameterDic() { EventArgs = e, EventName = eventName, Sender = sender };
            Queue<IScriptCommand> commands = new Queue<IScriptCommand>(
                processors.Select(p => commandFunc(p)).Where(c => c.CanExecute(pd)));
            _scriptRunner.Run(commands, pd);
            return pd.EventArgs.Handled;
        }

        void Control_DragLeave(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                execute(_eventProcessors, p => p.OnMouseDragLeave, "OnMouseDragLeave", sender, e);
            }
        }

        void Control_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                execute(_eventProcessors, p => p.OnMouseDrop, "OnMouseDrop", sender, e);
            }
        }

        void Control_DragEnter(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                execute(_eventProcessors, p => p.OnMouseDragEnter, "OnMouseDragEnter", sender, e);
            }
        }

        void Control_DragOver(object sender, DragEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (!AttachedProperties.GetIsDragging(control))
            {
                execute(_eventProcessors, p => p.OnMouseDragOver, "OnMouseDragOver", sender, e);
            }
        }

        MouseButtonEventArgs _mouseDownEvent = null;
        void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount > 1)
                return;
            bool isOverGridViewHeader = UITools.FindAncestor<GridViewColumnHeader>(e.OriginalSource as DependencyObject) != null;
            bool isOverScrollBar = UITools.FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject) != null;
            if (isOverGridViewHeader || isOverScrollBar)
                return;

            _mouseDownEvent = e;

            Control control = sender as Control;
            execute(_eventProcessors, p => p.OnMouseDown, "OnMouseDown", sender, e);

            AttachedProperties.SetIsDragging(control, false);
            if (UITools.IsMouseOverSelectedItem(control))
            {                
                AttachedProperties.SetStartPosition(control, e.GetPosition(control));
                AttachedProperties.SetStartScrollbarPosition(control, ControlUtils.GetScrollbarPosition(control));
            }
            else
            {
                AttachedProperties.SetStartPosition(control, AttachedProperties.InvalidPoint);
                AttachedProperties.SetStartScrollbarPosition(control, AttachedProperties.InvalidPoint);
            }
        }

        void Control_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;

            Point position = e.GetPosition(null);
            Point startPosition = AttachedProperties.GetStartPosition(control);
            if (startPosition.IsValidPosition())
            {
                execute(_eventProcessors, p => p.OnMouseMove, "OnMouseMove", sender, e);

                if (!AttachedProperties.GetIsDragging(control))
                    if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                        if (Math.Abs(position.X - startPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(position.Y - startPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                        {
                            AttachedProperties.SetIsDragging(control, true);
                            Control_MouseDrag(sender, e);
                        }
            }
        }

        public void Control_MouseDrag(object sender, MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (execute(_eventProcessors, p => p.OnMouseDrag, "OnMouseDrag ", sender, e))
            {
                Control_MouseUp(sender,
                    _mouseDownEvent ?? new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
            }
        }

        void Control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            AttachedProperties.SetIsDragging(control, false);
            AttachedProperties.SetStartPosition(control, AttachedProperties.InvalidPoint);
            AttachedProperties.SetStartScrollbarPosition(control, AttachedProperties.InvalidPoint);

            execute(_eventProcessors, p => p.OnMouseUp, "OnMouseUp ", sender, e);
        }

        #endregion

        #region Data

        private List<IUIEventProcessor> _eventProcessors;
        private IScriptRunner _scriptRunner;
        private bool _isEnabled = false;

        #endregion

        #region Public Properties

        public bool IsEnabled { get { return _isEnabled; } set { setIsEnabled(value); } }
        public UIElement Control { get; private set; }
        public List<IUIEventProcessor> EventProcessors { get { return _eventProcessors; } }
        #endregion


    }





}
