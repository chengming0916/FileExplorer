using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{

    public interface ITabControlViewModel<T> 
        where T : IDraggable
    {
        int GetTabIndex(T evm);
        void MoveTab(int srcIdx, int targetIdx);

        int SelectedIndex { get; set; }
        T SelectedItem { get; set; }
    }

    public interface ITabbedExplorerViewModel : ITabControlViewModel<IExplorerViewModel>,
        IConductor, IConductActiveItem, IParent<IScreen>
    {
        IExplorerViewModel OpenTab(IEntryModel model = null);
        void CloseTab(IExplorerViewModel evm);
    }
}
