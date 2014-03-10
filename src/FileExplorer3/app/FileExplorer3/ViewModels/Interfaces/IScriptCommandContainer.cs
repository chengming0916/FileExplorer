﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core.Script;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Owned by ViewModel (e.g. IFileListViewModel) for a number of changable IScriptCommands (e.g. Open)
    /// </summary>
    public interface IScriptCommandContainer : IExportCommandBindings
    {
        IParameterDicConverter ParameterDicConverter { get; }
    }    
}
