using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.UserControls.InputProcesor
{
    public interface IUIInputProcessor
    {
        void Update(IUIInput input);
    }

    public class ClickCountInputProcessor : IUIInputProcessor
    {
        #region Constructors

        public ClickCountInputProcessor()
        {

        }

        #endregion

        #region Methods

        public void Update(IUIInput input)
        {

            if (input.EventArgs is MouseButtonEventArgs)
                input.ClickCount = (input.EventArgs as MouseButtonEventArgs).ClickCount;
            else
                if (input.InputType == UIInputType.Touch && input.InputState == UIInputState.Pressed)
                {
                    if (DateTime.UtcNow.Subtract(_lastClickTime).TotalSeconds < 0.8 && 
                        input.IsWithin(_startInput, 10, 10))
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
        private DateTime _lastClickTime  = DateTime.MinValue;

        #endregion

        #region Public Properties

       
        #endregion
    }

    public class DragInputProcessor : IUIInputProcessor
    {
        #region Constructors

        public enum DragState { Normal, Touched, Pressed, TouchDragging, Dragging, Released }

        public DragInputProcessor()
        {

        }

        #endregion

        #region Methods

        public void Update(IUIInput input)
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

        private void UpdateInputPosition(IUIInput input)
        {
          
            if (_dragState == DragState.Touched && input.EventArgs is TouchEventArgs)
            {
                if (DateTime.UtcNow.Subtract(_touchTime).TotalSeconds >= 0.5)
                {
                    var rect = (input.EventArgs as TouchEventArgs).GetTouchPoint(null).Size;
                    if ((input as TouchInput).IsDragThresholdReached(_startTouchInput as TouchInput))
                    {
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
            if (_dragState == DragState.Pressed && input.IsDragThresholdReached(_startInput))
            {
                _dragState = DragState.Dragging;
                _isDragging = true;
                DragStartedFunc(input);
            }
        }

        private void UpdatInputReleased(IUIInput input)
        {
            if (input.IsSameSource(_startInput))
            {
                if (_isDragging && _dragState == DragState.Dragging)
                    DragStoppedFunc(input);
            }
            _isDragging = false;
            _dragState = DragState.Released;
        }

        private void UpdateInputPressed(IUIInput input)
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
                                case DragState.Released:
                                    _dragState = DragState.Pressed;
                                    break;
                            }

                            break;
                    }


                }
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
