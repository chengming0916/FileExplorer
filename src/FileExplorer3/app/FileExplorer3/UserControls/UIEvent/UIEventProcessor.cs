using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core.Script;
using FileExplorer.ViewModels;
using Cofe.Core;
using System.Windows.Input;
using FileExplorer.Defines;
using FileExplorer.UserControls.InputProcesor;

namespace FileExplorer.BaseControls
{

    /// <summary>
    /// Allow one application (e.g. dragging) to handle events from an control.
    /// </summary>
    public interface IUIEventProcessor
    {
        int Priority { get; }
        string TargetName { get; set; }
        IScriptCommand OnEvent(RoutedEvent eventId);
        IEnumerable<RoutedEvent> ProcessEvents { get; }
    }

    public abstract class UIEventProcessorBase : Freezable, IUIEventProcessor
    {
        public int Priority { get; protected set; }
        public virtual IEnumerable<RoutedEvent> ProcessEvents { get { return _processEvents; } }
        protected List<RoutedEvent> _processEvents = new List<RoutedEvent>();

        public UIEventProcessorBase()
        {

        }

        public class CheckTargetName : IfScriptCommand
        {
            public CheckTargetName(string targetName, IScriptCommand trueCommand, IScriptCommand falseCommand)
                : base(pm =>
                    {
                        IUIInput input = pm.AsUIParameterDic().Input;
                        object sender = input.Sender;
                        RoutedEventArgs eventArgs = input.EventArgs as RoutedEventArgs;

                        if (String.IsNullOrEmpty(targetName) || UITools.FindAncestor<FrameworkElement>(
                                               eventArgs.OriginalSource as DependencyObject,
                                               (ele) => ele.Name == targetName) != null)
                            return true;
                        return false;
                    }
                    , trueCommand, falseCommand)
            {
            }
        }
        public IScriptCommand OnEvent(RoutedEvent eventId)
        {
            return new CheckTargetName(TargetName, onEvent(eventId), ResultCommand.NoError);
        }

        protected virtual IScriptCommand onEvent(RoutedEvent eventId)
        {
            return ResultCommand.NoError;
        }

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        public static DependencyProperty TargetNameProperty =
            DependencyProperty.Register("TargetName", typeof(string), typeof(UIEventProcessorBase));
        public string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set { SetValue(TargetNameProperty, value); }
        }
    }

    public class SimpleUIEventProcessor : UIEventProcessorBase, IUIEventProcessor
    {
        #region Constructors

        #endregion

        #region Methods


        protected override IScriptCommand onEvent(RoutedEvent eventId)
        {
            if (_processEvents.ContainsKey(eventId))
                return _processEvents[eventId];
            return ResultCommand.NoError;
        }

        protected void registerEvent(RoutedEvent onEvent, IScriptCommand command)
        {
            _processEvents.Add(onEvent, command);
        }


        //protected void registerEvent(RoutedEvent onEvent, 
        //    Func<ParameterDic, IScriptCommand> actionFunc, 
        //    Func<ParameterDic, bool> canExecuteFunc = null,
        //    string commandKey = "SimpleUIEventProcessor")
        //{
        //    registerEvent(onEvent, new SimpleScriptCommand(commandKey, actionFunc, canExecuteFunc));
        //}



        #endregion

        #region Data

        Dictionary<RoutedEvent, IScriptCommand> _processEvents = new Dictionary<RoutedEvent, IScriptCommand>();

        #endregion

        #region Public Properties

        public override IEnumerable<RoutedEvent> ProcessEvents { get { return _processEvents.Keys; } }

        #endregion
    }

    public class DebugUIEventProcessor : UIEventProcessorBase
    {
        public static DebugUIEventProcessor Instance = new DebugUIEventProcessor();

        protected override IScriptCommand onEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "OnPreviewMouseDown": return ScriptCommands.PrintSourceDC;
                case "OnMouseDrag": return ScriptCommands.PrepareDrag;
                case "OnMouseUp": return ScriptCommands.PrintSourceDC;
                case "OnMouseDrop": return ScriptCommands.PrintSourceDC;
            }

            return base.OnEvent(eventId);
        }

        public DebugUIEventProcessor()
        {
            Priority = 0;
        }
    }

    public class TouchGestureEventProcessor : SimpleUIEventProcessor
    {
        #region Constructors

        public TouchGestureEventProcessor()
        {
            registerEvent(UIElement.PreviewTouchUpEvent,
                new SimpleScriptCommand("TouchGesture",
                    pd =>
                    {
                        if (Gesture == pd.AsUIParameterDic().Input.TouchGesture)
                            if (Command.CanExecute(CommandParameter))
                            {
                                Command.Execute(CommandParameter);
                                return ResultCommand.OK;
                            }

                        return ResultCommand.NoError;
                    }));
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public static DependencyProperty GestureProperty =
          DependencyProperty.Register("Gesture", typeof(UITouchGesture), typeof(TouchGestureEventProcessor),
          new PropertyMetadata(null));

        public UITouchGesture Gesture
        {
            get { return (UITouchGesture)GetValue(GestureProperty); }
            set { SetValue(GestureProperty, value); }
        }


        public static DependencyProperty CommandProperty =
           DependencyProperty.Register("Command", typeof(ICommand), typeof(TouchGestureEventProcessor),
           new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public static DependencyProperty CommandParameterProperty =
           DependencyProperty.Register("CommandParameter", typeof(object), typeof(TouchGestureEventProcessor),
           new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion
    }

}
