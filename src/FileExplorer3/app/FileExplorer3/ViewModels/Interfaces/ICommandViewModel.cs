using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileExplorer.Models;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public interface ICommandViewModel : IComparable<ICommandViewModel>, IComparable 
    {
        ICommandModel CommandModel { get; }
        ICommand Command { get; }
        ToolbarItemType CommandType { get; }
        ObservableCollection<ICommandViewModel> SubCommands { get; }

        object CommandParameter { get; }
    }
}
