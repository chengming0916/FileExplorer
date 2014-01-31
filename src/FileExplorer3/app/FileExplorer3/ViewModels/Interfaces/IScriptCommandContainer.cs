using System;
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

    public interface IDirectoryTreeScriptCommandContainer : IScriptCommandContainer
    {
        IScriptCommand Refresh { get; set; }
        IScriptCommand Delete { get; set; }
        IScriptCommand ToggleRename { get; set; }
    }


    public interface IFileListScriptCommandContainer : IScriptCommandContainer
    {
        IScriptCommand Open { get; set; }
        IScriptCommand Refresh { get; set; }
        IScriptCommand Delete { get; set; }
        IScriptCommand ToggleRename { get; set; }
    }


    public interface INavigationScriptCommandContainer : IScriptCommandContainer
    {
        IScriptCommand Back { get; set; }
        IScriptCommand Next { get; set; }
        IScriptCommand Up { get; set; }
    }

}
