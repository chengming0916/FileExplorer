using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{

    public interface ITabbedExplorerViewModel
    {
        void AddTab(ITabItemViewModel tab);
        void RemoveTab(ITabItemViewModel tab);

        ObservableCollection<ITabItemViewModel> Tabs { get; }
        ICommandManager Commands { get; }
    }

    public interface ITabItemViewModel
    {

    }
}
