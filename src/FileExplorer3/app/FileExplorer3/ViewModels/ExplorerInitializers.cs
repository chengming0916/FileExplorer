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
        { return new StartupDirectoryInitializers(startupDir); }
    }


    public class StartupDirectoryInitializers : IViewModelInitializer<IExplorerViewModel>
    {
        private IEntryModel _startupDir;

        public StartupDirectoryInitializers(IEntryModel startupDir)
        {
            _startupDir = startupDir;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            if (_startupDir != null)
                await explorerModel.GoAsync(_startupDir);
        }
    }
}
