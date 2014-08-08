﻿using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public class ScriptRelayCommand : RelayCommand
    {
        public ScriptRelayCommand(IScriptCommand command, IParameterDicConverter converter, IScriptRunner scriptRunner)
            : base(
            pm => 
                {
                    scriptRunner.RunAsync(converter.Convert(pm), command);
                }, 
            pm => command.CanExecute(converter.Convert(pm)))
        {

        }
    }
}
