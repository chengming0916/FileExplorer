using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.UserControls.InputProcesor
{
    public interface IUIInputProcessor
    {
        IEnumerable<RoutedEvent> ProcessEvents { get; }
        bool ProcessAllEvents { get; }
        void Update(ref IUIInput input);
    }

    public class InputProcessorBase : IUIInputProcessor
    {
        protected List<RoutedEvent> _processEvents;
        public InputProcessorBase()
        {
            ProcessAllEvents = false;
            _processEvents = new List<RoutedEvent>();
        }

        public virtual void Update(ref IUIInput input)
        {
        }

        public bool ProcessAllEvents { get; protected set; }

        public IEnumerable<RoutedEvent> ProcessEvents { get { return _processEvents; } }

    }

    public class FlickInputProcessor : InputProcessorBase
    {



        #region Constructors

        public FlickInputProcessor()
        {
            _processEvents.AddRange(new[] { 
                UIElement.PreviewTouchDownEvent,
                UIElement.PreviewTouchUpEvent
            }
            );
        }

        #endregion

        #region Methods

        public override void Update(ref IUIInput input)
        {
            switch (input.EventArgs.RoutedEvent.Name)
            {
                case "PreviewTouchDown":
                    _touchTime = DateTime.UtcNow;
                    _touchDownPosition = input.Position;
                    break;

                case "PreviewTouchUp":
                    if (DateTime.UtcNow.Subtract(_touchTime).TotalMilliseconds < Defines.Defaults.MaximumFlickTime)
                    {
                        if (Math.Abs(input.Position.X - _touchDownPosition.X) > Defines.Defaults.MinimumFlickThreshold)
                        {
                            if (input.Position.X > _touchDownPosition.X)
                                input.TouchGesture = UITouchGesture.FlickRight;
                            else input.TouchGesture = UITouchGesture.FlickLeft;
                        }
                        else if (Math.Abs(input.Position.Y - _touchDownPosition.Y) > Defines.Defaults.MinimumFlickThreshold)
                        {
                            if (input.Position.Y > _touchDownPosition.Y)
                                input.TouchGesture = UITouchGesture.FlickDown;
                            else input.TouchGesture = UITouchGesture.FlickUp;
                        }
                    }
                    break;
            }

        }

        #endregion

        #region Data

        private Point _touchDownPosition = AttachedProperties.InvalidPoint;
        private DateTime _touchTime = DateTime.MinValue;

        #endregion

        #region Public Properties

        #endregion

    }

    public class IsTouchDownInputProcessor : InputProcessorBase
    {

        #region Constructors

        public IsTouchDownInputProcessor()
        {
            ProcessAllEvents = true;
        }

        #endregion

        #region Methods

        public override void Update(ref IUIInput input)
        {
            if (input.InputType == UIInputType.Touch && input.InputState != UIInputState.NotApplied)
                _touchState = input.InputState;

            input.Touch = _touchState;
        }

        #endregion

        #region Data

        private UIInputState _touchState = UIInputState.NotApplied;

        #endregion

        #region Public Properties


        #endregion

    }

    public class DebugInputProcessor : InputProcessorBase
    {
        public DebugInputProcessor()
        {
            ProcessAllEvents = true;
        }

        public override void Update(ref IUIInput input)
        {
            var touchEventArgs = input.EventArgs as TouchEventArgs;
            if (touchEventArgs != null)
            {
                var touchPts = touchEventArgs.GetIntermediateTouchPoints(input.Sender as IInputElement);
                var touchInput = String.Join("", touchPts.Select(tp => tp.Action.ToString()[0]));
                Console.WriteLine(touchInput);
            }

        }
    }

    public class DropInputProcessor : InputProcessorBase
    {
        public DropInputProcessor()
        {
            ProcessAllEvents = false;
            _processEvents.AddRange(new[] { 
                UIElement.DragEnterEvent,
                UIElement.DragLeaveEvent,

                UIElement.DragOverEvent,
                UIElement.DropEvent
            }
            );
        }

        public override void Update(ref IUIInput input)
        {
            input = new DragInput(input);
        }
    }

    public class ClickCountInputProcessor : InputProcessorBase
    {
        #region Constructors

        public ClickCountInputProcessor()
        {

            ProcessAllEvents = false;
            _processEvents.AddRange(new[] { 
                UIElement.StylusSystemGestureEvent,
                UIElement.PreviewTouchDownEvent,
                UIElement.PreviewMouseLeftButtonDownEvent,
                UIElement.PreviewTouchDownEvent
            });
        }

        #endregion

        #region Methods

        public override void Update(ref IUIInput input)
        {



            if (input.EventArgs is MouseButtonEventArgs)
                input.ClickCount = (input.EventArgs as MouseButtonEventArgs).ClickCount;
            else
                if (input.InputType == UIInputType.Touch && input.InputState == UIInputState.Pressed)
                {

                    //touchPts.First().Action == TouchAction.
                    if (DateTime.UtcNow.Subtract(_lastClickTime).TotalMilliseconds <
                        FileExplorer.Defines.Defaults.MaximumClickInterval &&
                        input.IsWithin(_startInput, FileExplorer.Defines.Defaults.MinimumDragDistance,
                        FileExplorer.Defines.Defaults.MinimumDragDistance))
                    {
                        _clickCount += 1;
                        input.ClickCount = _clickCount;

                    }
                    else
                    {
                        _startInput = input;
                        _clickCount = 1;
                    }
                    _lastClickTime = DateTime.UtcNow;
                }
            //else _clickCount = 0;
        }

        #endregion

        #region Data

        private int _clickCount = 1;
        private IUIInput _startInput = InvalidInput.Instance;
        private DateTime _lastClickTime = DateTime.MinValue;

        #endregion

        #region Public Properties



        #endregion
    }

    public class DragInputProcessor : InputProcessorBase
    {
        #region Constructors

        public enum DragState { Normal, Touched, Pressed, TouchDragging, Dragging, Released }

        public DragInputProcessor()
        {
            ProcessAllEvents = false;
            _processEvents.AddRange(new[] { 
                UIElement.PreviewMouseDownEvent,
                UIElement.PreviewTouchDownEvent,

                UIElement.MouseMoveEvent,
                UIElement.TouchMoveEvent,

                UIElement.PreviewMouseUpEvent,
                UIElement.PreviewTouchUpEvent
            }
            );
        }

        #endregion

        #region Methods

        public override void Update(ref IUIInput input)
        {

            switch (input.InputState)
            {
                case Defines.UIInputState.Pressed:
                    UpdateInputPressed(input);
                    break;
                case Defines.UIInputState.Released:
                    UpdatInputReleased(input);
                    break;
                default:
                    UpdateInputPosition(input);
                    break;
            }
            input.IsDragging = IsDragging;
        }

        public void UpdateInputPosition(IUIInput input)
        {
            if (_dragState == DragState.Touched && input.EventArgs is TouchEventArgs)
            {
                if (DateTime.UtcNow.Subtract(_touchTime).TotalSeconds >= 0.5)
                {
                    var rect = (input.EventArgs as TouchEventArgs).GetTouchPoint(null).Size;
                    if ((input as TouchInput).IsDragThresholdReached(_startTouchInput as TouchInput))
                    {
                        StartInput = _startTouchInput;
                        _dragState = DragState.Pressed;
                        //_touchTime = DateTime.MinValue;
                        //_isDragging = true;
                        //DragStartedFunc(input);
                    }
                    else
                    {
                        _touchTime = DateTime.MinValue;
                        _dragState = DragState.Normal;
                    }
                }

            }

            //Console.WriteLine(String.Format("UpdateInputPosition - {0}", _dragState));
            if (_dragState == DragState.Pressed && input.IsDragThresholdReached(_startInput))
            {
                _dragState = DragState.Dragging;
                _isDragging = true;
                DragStartedFunc(input);
            }
        }

        public void UpdatInputReleased(IUIInput input)
        {
            if (input.IsSameSource(_startInput))
            {
                if (_isDragging && _dragState == DragState.Dragging)
                    DragStoppedFunc(input);
            }
            _isDragging = false;
            _dragState = DragState.Released;
            //Console.WriteLine(String.Format("UpdatInputReleased - {0}", _dragState));
        }

        public void UpdateInputPressed(IUIInput input)
        {

            if (!_isDragging && input.IsValidPositionForLisView(true))
                if (input.ClickCount <= 1) //Touch/Stylus input 's ClickCount = 0
                {
                    StartInput = input;
                    _isDragging = false;
                    switch (input.InputType)
                    {
                        case UIInputType.Touch:
                            _startTouchInput = input;
                            _dragState = DragState.Touched;
                            _touchTime = DateTime.UtcNow;
                            //input.EventArgs.Handled = true;
                            break;
                        default:
                            switch (_dragState)
                            {
                                case DragState.Touched:
                                    break;
                                case DragState.Normal:
                                case DragState.Released:
                                    _dragState = DragState.Pressed;
                                    break;
                            }

                            break;
                    }


                }
            //Console.WriteLine(String.Format("UpdateInputPressed - {0}", _dragState));
        }

        #endregion

        #region Data

        private DateTime _touchTime = DateTime.MinValue;
        private DragState _dragState = DragState.Normal;
        private bool _isDragging = false;
        private IUIInput _startInput = InvalidInput.Instance;
        private IUIInput _startTouchInput = InvalidInput.Instance;
        //private Func<Point, Point> _positionAdjustFunc = pt => pt;

        #endregion

        #region Public Properties

        //public Func<Point, Point> PositionAdjustFunc { get { return _positionAdjustFunc; } set { _positionAdjustFunc = value; } }

        public IUIInput StartInput
        {
            get { return _startInput; }
            private set { _startInput = value; }
        }
        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                _isDragging = value;
                if (!value && _dragState == DragState.Dragging)
                    StartInput = InvalidInput.Instance;
                _dragState = DragState.Normal;
            }
        }
        public Action<IUIInput> DragStartedFunc = (currentInput) => { };
        public Action<IUIInput> DragStoppedFunc = (currentInput) => { };


        #endregion
    }
}
