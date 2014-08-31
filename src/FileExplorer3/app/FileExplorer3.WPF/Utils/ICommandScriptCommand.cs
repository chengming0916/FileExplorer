using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cinch;
using FileExplorer;
using FileExplorer.Script;

namespace FileExplorer.WPF.Utils
{
    internal class ICommandScriptCommand : ScriptCommandBase
    {
        private ICommand _command;
        private IParameterDicConverter _parameterDicConverter;
        public ICommandScriptCommand(ICommand command, IParameterDicConverter parameterDicConverter)
            : base(command.ToString())
        {
            _command = command;
            _parameterDicConverter = parameterDicConverter;
        }

        public override bool CanExecute(IParameterDic pm)
        {
            return _command.CanExecute(_parameterDicConverter.ConvertBack(pm));
        }

        public override IScriptCommand Execute(IParameterDic pm)
        {
            try
            {
                _command.Execute(_parameterDicConverter.ConvertBack(pm));
            }
            catch (Exception ex) { return ResultCommand.Error(ex); }

            return ResultCommand.NoError;
        }
    }
   

}
