﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Markup;
using System.Windows.Threading;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

namespace FileExplorer.BaseControls
{

    public interface IUIEventHub
    {
        UIElement Control { get; }
        IList<UIEventProcessorBase> EventProcessors { get; }
        bool IsEnabled { get; set; }
    }

    public static partial class ExtensionMethods
    {
        public static IUIEventHub RegisterEventProcessors(this UIElement control, params UIEventProcessorBase[] processors)
        {
            return new UIEventHub(new ScriptRunner(), control, true, processors);
        }
    }

    public class UIEventHub : IUIEventHub
    {
        #region Constructor

        public UIEventHub(IScriptRunner runner, UIElement control, bool startIsEnabled = true, params UIEventProcessorBase[] eventProcessors)
        {
            Control = control;
            _eventProcessors = new List<UIEventProcessorBase>(eventProcessors);
            _scriptRunner = runner;
            IsEnabled = startIsEnabled;
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
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (value != _isEnabled)
                    {
                        var listenEvents = _eventProcessors.SelectMany(ep => ep.ProcessEvents).Distinct().ToArray();

                        _isEnabled = value;

                        if (_isEnabled)
                        {
                            foreach (var e in listenEvents)
                            {
                                switch (e.Name)
                                {
                                    case "PreviewMouseDown":
                                    case "PreviewTouchDown":
                                    case "MouseMove":
                                    case "MouseDrag":
                                    case "PreviewMouseUp":
                                    case "PreviewTouchUp":
                                        break;
                                    default:
                                        RoutedEventHandler handler = (RoutedEventHandler)(
                                            (o, re) => { executeAsync(_eventProcessors, e, o, re); });
                                        _registeredHandler.Add(e, handler);
                                        Control.AddHandler(e, handler);
                                        break;
                                }

                            }

                            //These has to be handled for detecting drag.
                            Control.PreviewMouseDown += Control_PreviewMouseDown;
                            Control.MouseMove += Control_MouseMove;
                            Control.PreviewMouseUp += Control_PreviewMouseUp;

                            Control.PreviewTouchDown += Control_PreviewTouchDown;
                            Control.TouchMove += Control_TouchMove;
                            Control.PreviewTouchUp += Control_PreviewTouchUp;
                        }
                        else
                        {

                            foreach (var evt in _registeredHandler.Keys)
                                Control.RemoveHandler(evt, _registeredHandler[evt]);
                            _registeredHandler.Clear();

                            Control.PreviewMouseDown -= Control_PreviewMouseDown;
                            Control.MouseMove -= Control_MouseMove;
                            Control.PreviewMouseUp -= Control_PreviewMouseUp;

                            Control.PreviewTouchDown -= Control_PreviewTouchDown;
                            Control.TouchMove -= Control_TouchMove;
                            Control.PreviewTouchUp -= Control_PreviewTouchUp;
                        }
                    }
                }));
        }

        private async Task<bool> executeAsync(IList<UIEventProcessorBase> processors, RoutedEvent eventId,
            object sender, RoutedEventArgs e, Action<ParameterDic> thenFunc = null)
        {

            if (eventId != Mouse.MouseMoveEvent)
                Debug.WriteLine(eventId);

            ParameterDic pd = ParameterDicConverters.ConvertUIParameter.Convert(null, eventId.Name, sender, e);                                
            Queue<IScriptCommand> commands = new Queue<IScriptCommand>(
                processors
                .Where(p => p.ProcessEvents.Contains(eventId))
                .Select(p => p.OnEvent(eventId))
                .Where(c => c.CanExecute(pd)));
            await _scriptRunner.RunAsync(commands, pd);
            if (thenFunc != null)
                thenFunc(pd);
            return pd.IsHandled;
        }


        private bool isValidPosition(object sender, InputEventArgs e)
        {
            var scp = ControlUtils.GetScrollContentPresenter(sender as Control);
            if (scp == null ||
                //ListViewEx contains ContentBelowHeader, allow placing other controls in it, this is to avoid that)
                (!scp.Equals(UITools.FindAncestor<ScrollContentPresenter>(e.OriginalSource as DependencyObject)) &&
                //This is for handling user click in empty area of a panel.
                 !(e.OriginalSource is ScrollViewer))
                )
                return false;

            bool isOverGridViewHeader = UITools.FindAncestor<GridViewColumnHeader>(e.OriginalSource as DependencyObject) != null;
            bool isOverScrollBar = UITools.FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject) != null;
            if (isOverGridViewHeader || isOverScrollBar)
                return false;

            return true;
        }
        //private void handleInputDown(object sender, InputEventArgs e, bool )
        //{
        //    if (!isValidPosition(sender, e))
        //        return;

        //    executeAsync(_eventProcessors, FrameworkElement.PreviewMouseDownEvent, sender, e, pd =>
        //    {
        //        if (e.ClickCount == 1)
        //        {
        //            Control control = sender as Control;
        //            AttachedProperties.SetIsMouseDragging(control, false);
        //            AttachedProperties.SetStartPosition(control, e.GetPosition(control));
        //            AttachedProperties.SetStartInput(control, e.ChangedButton);
        //            AttachedProperties.SetStartScrollbarPosition(control, ControlUtils.GetScrollbarPosition(control));
        //        }
        //    });
        //}


        MouseButtonEventArgs _mouseDownEvent = null;
        void Control_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isValidPosition(sender, e))
                return;

            if (e.ClickCount == 1)
            {
                _mouseDownEvent = e;
                
            }
            //if (!control.IsFocused)
            //    return;
            executeAsync(_eventProcessors, FrameworkElement.PreviewMouseDownEvent, sender, e, pd =>
                {
                    if (e.ClickCount == 1)
                    {
                        Control control = sender as Control;
                        AttachedProperties.SetIsMouseDragging(control, false);
                        AttachedProperties.SetStartPosition(control, e.GetPosition(control));
                        AttachedProperties.SetStartInput(control, e.ChangedButton);
                        AttachedProperties.SetStartScrollbarPosition(control, ControlUtils.GetScrollbarPosition(control));
                    }
                });
        }

        void Control_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;

            Point position = e.GetPosition(control);
            Point startPosition = AttachedProperties.GetStartPosition(control);
            if (startPosition.IsValidPosition())
            {
                executeAsync(_eventProcessors, FrameworkElement.MouseMoveEvent, sender, e);

                if (!AttachedProperties.GetIsMouseDragging(control))
                    if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                        if (Math.Abs(position.X - startPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(position.Y - startPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                        {
                            Control_MouseDrag(sender, e);
                        }
            }
        }


        void Control_PreviewTouchDown(object sender, TouchEventArgs e)
        {
          
        }

        void Control_TouchMove(object sender, TouchEventArgs e)
        {

        }

        void Control_PreviewTouchUp(object sender, TouchEventArgs e)
        {

        }


        public static RoutedEvent MouseDragEvent = EventManager.RegisterRoutedEvent(
                "MouseDrag", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIEventHub));

        public void Control_MouseDrag(object sender, InputEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            AttachedProperties.SetIsMouseDragging(control, true);
            if (AsyncUtils.RunSync(() => executeAsync(_eventProcessors, UIEventHub.MouseDragEvent, sender, e)))
            //DragDropEventProcessor set e.IsHandled to true
            {
                //DragDrop does not raise MouseUp, so have to raise manually.
                Control_PreviewMouseUp(sender, _mouseDownEvent);
            }
        }

        void Control_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isValidPosition(sender, e))
                return;

            FrameworkElement control = sender as FrameworkElement;

            executeAsync(_eventProcessors, FrameworkElement.PreviewMouseUpEvent, sender, e);

            AttachedProperties.SetIsMouseDragging(control, false);
            AttachedProperties.SetStartPosition(control, AttachedProperties.InvalidPoint);
            AttachedProperties.SetStartScrollbarPosition(control, AttachedProperties.InvalidPoint);
        }

        #endregion

        #region Data

        private IList<UIEventProcessorBase> _eventProcessors;
        private Dictionary<RoutedEvent, RoutedEventHandler> _registeredHandler = new Dictionary<RoutedEvent, RoutedEventHandler>();
        private IScriptRunner _scriptRunner;
        private bool _isEnabled = false;

        #endregion

        #region Public Properties

        public bool IsEnabled { get { return _isEnabled; } set { setIsEnabled(value); } }
        public UIElement Control { get; private set; }
        public IList<UIEventProcessorBase> EventProcessors
        {
            get { return _eventProcessors; }
            set { _eventProcessors = value; }
        }



        #endregion

    }


}
