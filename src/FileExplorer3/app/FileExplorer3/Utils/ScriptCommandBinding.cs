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
    public enum ScriptBindingScope { Application, Explorer, Local }

    public interface IScriptCommandBinding : INotifyPropertyChanged
    {
        IScriptCommand ScriptCommand { get; set; }
        ICommand Command { get; set; }
        RoutedUICommand UICommandKey { get; }
        CommandBinding CommandBinding { get; }
        ScriptBindingScope Scope { get; }
    }

   

    public class ScriptCommandBinding : NotifyPropertyChanged, IScriptCommandBinding
    {
        #region Constructor

        public static IScriptCommandBinding FromScriptCommand<T>(RoutedUICommand uiCommandKey,
         T targetObject, Func<T, IScriptCommand> scriptCommandFunc, IParameterDicConverter parameterDicConverter = null, ScriptBindingScope scope = ScriptBindingScope.Application)
        {
            return new ScriptCommandBinding<T>(uiCommandKey, targetObject, scriptCommandFunc, parameterDicConverter) { Scope = scope };
        }

        public ScriptCommandBinding(RoutedUICommand uICommandKey, ICommand command, IParameterDicConverter parameterDicConverter = null)
        {
            Scope = ScriptBindingScope.Application;
            Command = command;
            UICommandKey = uICommandKey == null ? ApplicationCommands.NotACommand : uICommandKey;            
            ParameterDicConverter = parameterDicConverter == null ? ParameterDicConverters.ConvertParameterOnly : parameterDicConverter;            
        }

        public ScriptCommandBinding(RoutedUICommand uICommandKey, IScriptCommand scriptCommand,
            IParameterDicConverter parameterDicConverter = null)
        {
            Scope = ScriptBindingScope.Application;
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

        protected ScriptCommandBinding(RoutedUICommand uiCommandKey, IParameterDicConverter parameterDicConverter = null)
        {
            Scope = ScriptBindingScope.Application;
            UICommandKey = uiCommandKey;
            ParameterDicConverter = parameterDicConverter == null ? ParameterDicConverters.ConvertParameterOnly : parameterDicConverter;
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

        protected void setScriptCommand(IScriptCommand value)
        {
            _scriptCommand = value;
            _command = new SimpleCommand()
            {
                CanExecuteDelegate = (p) => ScriptCommand.CanExecute(ParameterDicConverter.Convert(p)),
                ExecuteDelegate = (p) => ScriptRunnerSources.Default.GetScriptRunner().Run(
                    new Queue<IScriptCommand>(new[] { ScriptCommand }), ParameterDicConverter.Convert(p))
            };
        }

        protected void setCommand(ICommand value)
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

        public virtual IScriptCommand ScriptCommand { get { return _scriptCommand; } set { setScriptCommand(value); } }
        public ICommand Command { get { return _command; } set { setCommand(value); } }        
        private IParameterDicConverter ParameterDicConverter { get; set; }
        public RoutedUICommand UICommandKey { get; private set; }
        public CommandBinding CommandBinding { get { return getCommandBiniding(); } }
        public ScriptBindingScope Scope { get; set; }

        #endregion


    }

    public class ScriptCommandBinding<T> : ScriptCommandBinding
    {
        #region Constructor

        public ScriptCommandBinding(RoutedUICommand uICommandKey,
            T targetObject, Func<T, IScriptCommand> scriptCommandFunc,
            IParameterDicConverter parameterDicConverter = null)
            : base(uICommandKey, parameterDicConverter)
        {
            _targetObject = targetObject;
            _scriptCommandFunc = scriptCommandFunc;
            
            setScriptCommand(null); //Assign Command to call ScriptCommand

        }

     

        #endregion

        #region Methods

        #endregion

        #region Data

        public Func<T, IScriptCommand> _scriptCommandFunc;
        public T _targetObject;

        #endregion

        #region Public Properties

        public override IScriptCommand ScriptCommand { get { return _scriptCommandFunc(_targetObject); } }

        #endregion
    }
}
