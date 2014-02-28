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
        void Update(IInput input);
    }

    public class DragInputProcessor : IInputProcessor
    {
        #region Constructors

        public DragInputProcessor()
        {

        }

        #endregion

        #region Methods

        public void Update(IInput input)
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

        private void UpdateInputPosition(IInput input)
        {
            if (!_isDragging && input.IsDragThresholdReached(_startInput))
            {
                _isDragging = true;
                DragStartedFunc(input);
            }
        }

        private void UpdatInputReleased(IInput input)
        {
            if (input.IsSameSource(_startInput))
            {
                if (_isDragging)
                    DragStoppedFunc(input);
                _isDragging = false;
                _startInput = null;
            }
        }

        private void UpdateInputPressed(IInput input)
        {
            if (!_isDragging && input.IsValidPositionForLisView(true))
                if (input.ClickCount <= 1) //Touch/Stylus input 's ClickCount = 0
                {
                    _startInput = input;
                    _isDragging = false;
                }
        }

        #endregion

        #region Data

        private bool _isDragging = false;
        private IInput _startInput = null;
        //private Func<Point, Point> _positionAdjustFunc = pt => pt;

        #endregion

        #region Public Properties

        //public Func<Point, Point> PositionAdjustFunc { get { return _positionAdjustFunc; } set { _positionAdjustFunc = value; } }
        public bool IsDragging { get { return _isDragging; } set { _isDragging = value; } }
        public Action<IInput> DragStartedFunc = (currentInput) => { };
        public Action<IInput> DragStoppedFunc = (currentInput) => { };

        #endregion
    }
}
