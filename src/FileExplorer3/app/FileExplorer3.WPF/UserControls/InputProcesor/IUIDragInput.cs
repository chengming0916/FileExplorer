using FileExplorer.Defines;
using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileExplorer.UserControls.InputProcesor
{

    public interface IUIDragInput : IUIInput
    {
        IDataObject Data { get; }
        DragDropEffects AllowedEffects { get; }
        DragDropEffects Effects { get;  set; }

    }

    public class DragInput : IUIDragInput
    {
        #region Constructors

        private void init(IDataObject dataObject,
             DragDropEffects allowedEffects, Action<DragDropEffects> effectSetFunc)
        {
            Data = dataObject;
            AllowedEffects = allowedEffects;
            _effectSetFunc = effectSetFunc;
        }

        public DragInput(IUIInput sourceInput, IDataObject dataObject,
             DragDropEffects allowedEffects, Action<DragDropEffects> effectSetFunc)
        {
            _sourceInput = sourceInput;
            init(dataObject, allowedEffects, effectSetFunc);
        }

        public DragInput(IUIInput sourceInput)
        {
            _sourceInput = sourceInput;
            var eventArgs = sourceInput.EventArgs as DragEventArgs;
            _relPositionFunc = relTo => eventArgs.GetPosition(relTo);
            _position = _relPositionFunc(sourceInput.Sender as IInputElement);
            init(eventArgs.Data, eventArgs.AllowedEffects, (eff) => { eventArgs.Effects = eff; });
        }

        #endregion

        #region Methods

        public Point PositionRelativeTo(IInputElement inputElement)
        {
            return _relPositionFunc != null ?
                _relPositionFunc(inputElement) :
                _sourceInput.PositionRelativeTo(inputElement);
        }

        #endregion

        #region Data

        IUIInput _sourceInput;
        private Action<DragDropEffects> _effectSetFunc;
        private DragDropEffects _effect = DragDropEffects.None;
        private Func<IInputElement, Point> _relPositionFunc;
        private Point? _position;
        

        #endregion

        #region Public Properties

        public IDataObject Data { get; private set; }
        public DragDropEffects AllowedEffects { get; private set; }
        public DragDropEffects Effects { get { return _effect; } set { _effect = value; _effectSetFunc(value); } }

        public RoutedEventArgs EventArgs { get { return _sourceInput.EventArgs; } }
        public object Sender { get { return _sourceInput.Sender; } set { _sourceInput.Sender = value; } }
        public int ClickCount { get { return _sourceInput.ClickCount; } set { _sourceInput.ClickCount = value; } }
        public Point Position { get { return _position.HasValue ? _position.Value : _sourceInput.Position; } }
        public Point ScrollBarPosition { get { return _sourceInput.ScrollBarPosition; } }
        public UIInputType InputType { get { return _sourceInput.InputType; } }
        public UIInputState InputState { get { return _sourceInput.InputState; } }
        public bool IsDragging { get { return _sourceInput.IsDragging; } set { _sourceInput.IsDragging = value; } }
        public UIInputState Touch { get { return _sourceInput.Touch; } set { _sourceInput.Touch = value; } }
        public UITouchGesture TouchGesture { get { return _sourceInput.TouchGesture; } set { _sourceInput.TouchGesture = value; } }


        #endregion




    }


}
