using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileExplorer.Models;
using FileExplorer.UserControls;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface ICommandViewModel : IComparable<ICommandViewModel>, IComparable 
    {
        ICommandModel CommandModel { get; }
        IScriptCommandBinding CommandBinding { get; }
        ToolbarItemType CommandType { get; }
        IEntriesHelper<ICommandViewModel> SubCommands { get; }

        object CommandParameter { get; }
    }
}
