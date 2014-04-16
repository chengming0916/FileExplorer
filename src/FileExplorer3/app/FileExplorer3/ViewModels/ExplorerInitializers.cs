using FileExplorer.Defines;
using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public static class ExplorerInitializers
    {
        public static IViewModelInitializer<IExplorerViewModel> StartupDirectory(IEntryModel startupDir)
        { return DoAsync(async evm => await evm.GoAsync(startupDir)); }

        public static IViewModelInitializer<IExplorerViewModel> ViewMode(string viewMode, int itemSize)
        { return Do(evm => { evm.FileList.ViewMode = viewMode; evm.FileList.ItemSize = itemSize; }); }

       
        public static IViewModelInitializer<IExplorerViewModel> Do(Action<IExplorerViewModel> action)
        {
            return new DoViewModelInitializer(action);
        }

        public static IViewModelInitializer<IExplorerViewModel> DoAsync(Func<IExplorerViewModel, Task> action)
        {
            return new DoAsyncViewModelInitializer(action);
        }
    
    }

    public class DoViewModelInitializer : IViewModelInitializer<IExplorerViewModel>
    {
        private Action<IExplorerViewModel> _action;

        public DoViewModelInitializer(Action<IExplorerViewModel> action)
        {
            _action = action;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            _action(explorerModel);
        }
    }

    public class DoAsyncViewModelInitializer : IViewModelInitializer<IExplorerViewModel>
    {
        private Func<IExplorerViewModel, Task> _action;

        public DoAsyncViewModelInitializer(Func<IExplorerViewModel, Task> action)
        {
            _action = action;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            await _action(explorerModel);
        }
    }

   
}
