using Caliburn.Micro;
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
        void OpenTab(IExplorerViewModel evm = null);
        void CloseTab(IExplorerViewModel evm);
    }
}
