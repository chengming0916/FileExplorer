using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cinch;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Utils
{
    public interface IScriptCommandBinding : INotifyPropertyChanged
    {
        IScriptCommand ScriptCommand { get; set; }
        ICommand Command { get; set; }
        RoutedUICommand UICommandKey { get; }
        CommandBinding CommandBinding { get; }        
    }
    

    public class ScriptCommandBinding : NotifyPropertyChanged, IScriptCommandBinding
    {
        #region Constructor

        public ScriptCommandBinding(RoutedUICommand uICommandKey, ICommand command, IParameterDicConverter parameterDicConverter = null)
        {
            Command = command;
            UICommandKey = uICommandKey == null ? ApplicationCommands.NotACommand : uICommandKey;            
            ParameterDicConverter = parameterDicConverter == null ? ParameterDicConverters.ConvertParameterOnly : parameterDicConverter;            
        }

        public ScriptCommandBinding(RoutedUICommand uICommandKey, IScriptCommand scriptCommand,
            IParameterDicConverter parameterDicConverter = null)
        {
            ScriptCommand = scriptCommand;
            UICommandKey = uICommandKey == null ? ApplicationCommands.NotACommand : uICommandKey;            
            ParameterDicConverter = parameterDicConverter == null ? ParameterDicConverters.ConvertParameterOnly : parameterDicConverter;
           
        }

        public ScriptCommandBinding(RoutedUICommand uICommandKey, Func<object, bool> canExecuteFunc, Action<object> executeFunc, 
            IParameterDicConverter parameterDicConverter = null)
            : this(uICommandKey, new SimpleCommand() { CanExecuteDelegate = (p) => canExecuteFunc == null || canExecuteFunc(p), 
                ExecuteDelegate = executeFunc, UICommand = uICommandKey }, parameterDicConverter)
        {
        }

        #endregion

        #region Methods

        private CommandBinding getCommandBiniding()
        {
            return new CommandBinding(UICommandKey,
               (ExecutedRoutedEventHandler)delegate(object sender, ExecutedRoutedEventArgs e)
               {
                   Command.Execute(e.Parameter);
                   e.Handled = true;
               },
               (CanExecuteRoutedEventHandler)delegate(object sender, CanExecuteRoutedEventArgs e)
               {
                   e.CanExecute = Command.CanExecute(e.Parameter);
               });
        }

        void setScriptCommand(IScriptCommand value)
        {
            _scriptCommand = value;
            _command = new SimpleCommand()
            {
                CanExecuteDelegate = (p) => ScriptCommand.CanExecute(ParameterDicConverter.Convert(p)),
                ExecuteDelegate = (p) => ScriptRunnerSources.Default.GetScriptRunner().Run(
                    new Queue<IScriptCommand>(new[] { ScriptCommand }), ParameterDicConverter.Convert(p))
            };
        }

        void setCommand(ICommand value)
        {
            _command = value;
            _scriptCommand = new ICommandScriptCommand(Command, ParameterDicConverter);
        }
      
        #endregion

        #region Data

        private IScriptCommand _scriptCommand = null;
        private ICommand _command = null;

        #endregion

        #region Public Properties

        public IScriptCommand ScriptCommand { get { return _scriptCommand; } set { setScriptCommand(value); } }
        public ICommand Command { get { return _command; } set { setCommand(value); } }        
        private IParameterDicConverter ParameterDicConverter { get; set; }
        public RoutedUICommand UICommandKey { get; private set; }
        public CommandBinding CommandBinding { get { return getCommandBiniding(); } }

        #endregion


    }
}
