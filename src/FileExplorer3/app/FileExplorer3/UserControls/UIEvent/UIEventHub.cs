using System;
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
using Cofe.Core.Script;
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
                                    case "MouseMove":
                                    case "MouseDrag":
                                    case "PreviewMouseUp":
                                        break;
                                    default:
                                        RoutedEventHandler handler = (RoutedEventHandler)(
                                            (o, re) => { execute(_eventProcessors, e, o, re); });
                                        _registeredHandler.Add(e, handler);
                                        Control.AddHandler(e, handler);
                                        break;
                                }

                            }

                            //These has to be handled for detecting drag.
                            Control.PreviewMouseDown += Control_PreviewMouseDown;
                            Control.MouseMove += Control_MouseMove;
                            Control.PreviewMouseUp += Control_PreviewMouseUp;
                        }
                        else
                        {

                            foreach (var evt in _registeredHandler.Keys)
                                Control.RemoveHandler(evt, _registeredHandler[evt]);
                            _registeredHandler.Clear();

                            Control.PreviewMouseDown -= Control_PreviewMouseDown;
                            Control.MouseMove -= Control_MouseMove;
                            Control.PreviewMouseUp -= Control_PreviewMouseUp;
                        }
                    }
                }));
        }


        private bool execute(IList<UIEventProcessorBase> processors, RoutedEvent eventId, object sender, RoutedEventArgs e)
        {
            //if (eventName != "OnMouseMove")
            //    Debug.WriteLine(eventName);
            UIParameterDic pd = new UIParameterDic() { EventArgs = e, EventName = eventId.Name, Sender = sender };
            Queue<IScriptCommand> commands = new Queue<IScriptCommand>(
                processors
                .Where(p => p.ProcessEvents.Contains(eventId))
                .Select(p => p.OnEvent(eventId))
                .Where(c => c.CanExecute(pd)));
            _scriptRunner.Run(commands, pd);
            return pd.IsHandled;
        }




        MouseButtonEventArgs _mouseDownEvent = null;
        void Control_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var scp = ControlUtils.GetScrollContentPresenter(sender as Control);
                if (scp == null ||
                    //ListViewEx contains ContentBelowHeader, allow placing other controls in it, this is to avoid that)
                    (!scp.Equals(UITools.FindAncestor<ScrollContentPresenter>(e.OriginalSource as DependencyObject)) &&
                    //This is for handling user click in empty area of a panel.
                     !(e.OriginalSource is ScrollViewer))
                    )
                    return;

                bool isOverGridViewHeader = UITools.FindAncestor<GridViewColumnHeader>(e.OriginalSource as DependencyObject) != null;
                bool isOverScrollBar = UITools.FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject) != null;
                if (isOverGridViewHeader || isOverScrollBar)
                    return;

                _mouseDownEvent = e;
                
            }
            //if (!control.IsFocused)
            //    return;
            execute(_eventProcessors, FrameworkElement.PreviewMouseDownEvent, sender, e);

            if (e.ClickCount == 1)
            {
                Control control = sender as Control;
                AttachedProperties.SetIsMouseDragging(control, false);
                AttachedProperties.SetStartPosition(control, e.GetPosition(control));
                AttachedProperties.SetStartInput(control, e.ChangedButton);                
                AttachedProperties.SetStartScrollbarPosition(control, ControlUtils.GetScrollbarPosition(control));
            }

        }

        void Control_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;

            Point position = e.GetPosition(control);
            Point startPosition = AttachedProperties.GetStartPosition(control);
            if (startPosition.IsValidPosition())
            {
                execute(_eventProcessors, FrameworkElement.MouseMoveEvent, sender, e);

                if (!AttachedProperties.GetIsMouseDragging(control))
                    if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                        if (Math.Abs(position.X - startPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(position.Y - startPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                        {
                            Control_MouseDrag(sender, e);
                        }
            }
        }

        public static RoutedEvent MouseDragEvent = EventManager.RegisterRoutedEvent(
                "MouseDrag", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIEventHub));

        public void Control_MouseDrag(object sender, MouseEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            AttachedProperties.SetIsMouseDragging(control, true);
            if (execute(_eventProcessors, UIEventHub.MouseDragEvent, sender, e))
            //DragDropEventProcessor set e.IsHandled to true
            {
                //DragDrop does not raise MouseUp, so have to raise manually.
                Control_PreviewMouseUp(sender,
                    _mouseDownEvent ?? new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
            }
        }

        void Control_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;

            execute(_eventProcessors, FrameworkElement.PreviewMouseUpEvent, sender, e);

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
