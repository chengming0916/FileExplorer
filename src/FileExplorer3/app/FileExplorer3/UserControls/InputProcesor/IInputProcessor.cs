using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UserControls.InputProcesor
{
    public interface IInputProcessor
    {
        void Update(IUIInput input);
    }

    public class DragInputProcessor : IInputProcessor
    {
        #region Constructors

        public enum DragState {  Normal, Pressed, Dragging, Released }

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
        }

        private void UpdateInputPosition(IUIInput input)
        {
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
                _isDragging = false;
                _dragState = DragState.Released;
                //_startInput = InvalidInput.Instance;
            }
        }

        private void UpdateInputPressed(IUIInput input)
        {
            if (!_isDragging && input.IsValidPositionForLisView(true))
                if (input.ClickCount <= 1) //Touch/Stylus input 's ClickCount = 0
                {
                    StartInput = input;
                    _isDragging = false;
                    _dragState = DragState.Pressed;
                }
        }

        #endregion

        #region Data

        private DragState _dragState = DragState.Normal;
        private bool _isDragging = false;
        private IUIInput _startInput = InvalidInput.Instance;
        //private Func<Point, Point> _positionAdjustFunc = pt => pt;

        #endregion

        #region Public Properties

        //public Func<Point, Point> PositionAdjustFunc { get { return _positionAdjustFunc; } set { _positionAdjustFunc = value; } }

        public IUIInput StartInput { get { return _startInput; } 
            private set { _startInput = value; } }
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
