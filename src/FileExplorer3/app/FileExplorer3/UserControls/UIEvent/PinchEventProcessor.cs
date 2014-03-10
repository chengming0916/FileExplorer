using Cofe.Core.Script;
using FileExplorer.UserControls;
using FileExplorer.UserControls.InputProcesor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.BaseControls
{
    public class PinchEventProcessor : UIEventProcessorBase, ICommandSource
    {
        public enum PinchMode { XIn, XOut, YIn, YOut, XYIn, XYOut }
        #region Constructors

        public PinchEventProcessor()
        {
            _processEvents.AddRange(
                new [] 
                { 
                    UIElement.ManipulationStartingEvent,
                    UIElement.ManipulationDeltaEvent
                });
        }

        #endregion

        #region Methods

        public override IScriptCommand OnEvent(RoutedEvent eventId)
        {
            
            switch (eventId.Name)
            {
                case "ManipulationStarting":
                    return new SimpleScriptCommand("", pd => { _prevX = 0; _prevY = 0;
                    (pd.AsUIParameterDic().EventArgs as ManipulationStartingEventArgs)
                        .ManipulationContainer = pd.AsUIParameterDic().Sender as IInputElement;
                        return ResultCommand.NoError; });
                    

            }
            return new SimpleScriptCommand("",
                pd =>
                {
                    var input = pd.AsUIParameterDic().Input as ManipulationInput;
                    Console.WriteLine(input.Delta.Scale.Length);
                    bool match = false;
                    switch (Mode)
                    {
                        case PinchMode.XIn: match = input.Delta.Scale.X > 1; break;
                        case PinchMode.YIn: match = input.Delta.Scale.Y > 1; break;
                        case PinchMode.XYIn: match = input.Delta.Scale.X  + input.Delta.Scale.Y> 1; break;

                        case PinchMode.XOut: match = input.Delta.Scale.X < 1; break;
                        case PinchMode.YOut: match = input.Delta.Scale.Y < 1; break;
                        case PinchMode.XYOut: match = input.Delta.Scale.X + input.Delta.Scale.Y < 1; break;
                    }
                    if (match)
                    {
                        if ((Command as RoutedCommand).CanExecute(CommandParameter, CommandTarget))
                            (Command as RoutedCommand).Execute(CommandParameter, CommandTarget);
                        
                    }
                    return ResultCommand.NoError;
                });
        }

        #endregion

        #region Data

        int _prevX = 0, _prevY = 0;

        #endregion

        #region Public Properties

        public static DependencyProperty ModeProperty =
                 DependencyProperty.Register("Mode", typeof(PinchMode),
                 typeof(PinchEventProcessor), new PropertyMetadata(PinchMode.XYIn));

        public PinchMode Mode
        {
            get { return (PinchMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static DependencyProperty CommandProperty =
                 DependencyProperty.Register("Command", typeof(ICommand),
                 typeof(PinchEventProcessor));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static DependencyProperty CommandParameterProperty =
                DependencyProperty.Register("CommandParameter", typeof(object),
                typeof(PinchEventProcessor));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static DependencyProperty CommandTargetProperty =
                DependencyProperty.Register("CommandTarget", typeof(IInputElement),
                typeof(PinchEventProcessor));

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

      
    }
}
