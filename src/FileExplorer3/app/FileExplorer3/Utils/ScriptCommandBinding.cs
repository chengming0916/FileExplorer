using System;
using System.Collections.Generic;
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
    public interface IScriptCommandBinding
    {
        IScriptCommand ScriptCommand { get; }
        ICommand Command { get; }
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
            ScriptRunnerSource = ScriptRunnerSources.Null;
            ParameterDicConverter = parameterDicConverter == null ? ParameterDicConverters.ConvertParameterOnly : parameterDicConverter;
            ScriptCommand = new ICommandScriptCommand(command, ParameterDicConverter);
        }

        public ScriptCommandBinding(RoutedUICommand uICommandKey, IScriptCommand scriptCommand,
            IScriptRunnerSource scriptRunnerSource = null, IParameterDicConverter parameterDicConverter = null)
        {
            ScriptCommand = scriptCommand;
            UICommandKey = uICommandKey == null ? ApplicationCommands.NotACommand : uICommandKey;
            ScriptRunnerSource = scriptRunnerSource == null ? ScriptRunnerSources.Null : scriptRunnerSource;
            ParameterDicConverter = parameterDicConverter == null ? ParameterDicConverters.ConvertParameterOnly : parameterDicConverter;
            Command = new SimpleCommand()
            {
                CanExecuteDelegate = (p) => scriptCommand.CanExecute(ParameterDicConverter.Convert(p)),
                ExecuteDelegate = (p) => ScriptRunnerSource.GetScriptRunner().Run(
                    new Queue<IScriptCommand>(new[] { scriptCommand }), ParameterDicConverter.Convert(p))
            };
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
      
        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IScriptCommand ScriptCommand { get; private set; }
        public ICommand Command { get; private set; }
        private IScriptRunnerSource ScriptRunnerSource { get; set; }
        private IParameterDicConverter ParameterDicConverter { get; set; }
        public RoutedUICommand UICommandKey { get; private set; }
        public CommandBinding CommandBinding { get { return getCommandBiniding(); } }

        #endregion


    }
}
