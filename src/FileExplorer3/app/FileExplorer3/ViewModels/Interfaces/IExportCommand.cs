using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Indicate the view model contains a number of ICommands.
    /// </summary>
    public interface ICommandContainer
    {
        IEnumerable<ICommand> ExportedCommands { get; }
    }

    
}
