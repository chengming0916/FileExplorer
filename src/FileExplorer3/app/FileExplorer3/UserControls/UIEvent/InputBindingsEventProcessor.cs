using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.UserControls.InputProcesor;

namespace FileExplorer.BaseControls
{



    public class InputBindingsEventProcessor : UIEventProcessorBase
    {

        public class QueryInputBindings : ScriptCommandBase
        {
            InputBindingsEventProcessor _processor;
            public QueryInputBindings(InputBindingsEventProcessor processor)
                : base("QueryInputBindings")
            {
                _processor = processor;
                
            }            
            public override IScriptCommand Execute(ParameterDic pm)
            {
                IUIInput input = pm.AsUIParameterDic().Input;
                object sender = input.Sender;
                InputEventArgs eventArgs = input.EventArgs as InputEventArgs;
                foreach (InputBinding ib in _processor.InputBindings)
                {
                    bool match = ib.Gesture.Matches(sender, eventArgs);
                    if (!match && ib is MouseBinding && 
                        input.InputType == Defines.UIInputType.Touch &&
                        (ib as MouseBinding).MouseAction == MouseAction.LeftDoubleClick)
                        match = input.ClickCount == 2;

                    if (match && ib.Command != null)
                        if (ib.Command.CanExecute(ib.CommandParameter))
                        {
                            ib.Command.Execute(ib.CommandParameter);
                            return ResultCommand.OK;
                        }
                }
                return ResultCommand.NoError;
            }
        }

        public InputBindingsEventProcessor()
        {            
            _processEvents.AddRange(
                new [] {
                    FrameworkElement.KeyDownEvent, 
                    FrameworkElement.PreviewMouseDownEvent,
                    FrameworkElement.PreviewTouchDownEvent
                });
        }

        public override IScriptCommand OnEvent(RoutedEvent eventId)
        {
            return new QueryInputBindings(this);
        }

        public static DependencyProperty InputBindingsProperty =
            DependencyProperty.Register("InputBindings", typeof(InputBindingCollection), typeof(InputBindingsEventProcessor), 
            new PropertyMetadata(new InputBindingCollection()));

        public InputBindingCollection InputBindings
        {
            get { return (InputBindingCollection)GetValue(InputBindingsProperty); }
            set { SetValue(InputBindingsProperty, value); }
        }

    }
}
