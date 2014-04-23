using Caliburn.Micro;
using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{

    public interface ITabbedExplorerViewModel : IConductor, IConductActiveItem, IParent<IScreen>
    {
        void OpenTab(IEntryModel model = null);
        void CloseTab(IExplorerViewModel evm);

        int GetTabIndex(IExplorerViewModel evm);
        void InsertTab(IExplorerViewModel evm, int idx);
    }
}
