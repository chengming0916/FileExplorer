using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    public class ScriptCommandInitializer : IExplorerInitializer
    {
        public IScriptCommand[] OnViewAttached { get; set; }

        public IScriptCommand[] OnModelCreated { get; set; }

        public Caliburn.Micro.IEventAggregator Events
        {
            get;
            set;
        }

        public Caliburn.Micro.IWindowManager WindowManager
        {
            get;
            set;
        }

        public FileExplorer.Models.IEntryModel[] RootModels
        {
            get;
            set;
        }

        public IExplorerInitializer Clone()
        {
            throw new NotImplementedException();
        }

        public async Task InitializeModelCreatedAsync(IExplorerViewModel evm)
        {
            if (OnModelCreated != null)
                await evm.Commands.ExecuteAsync(OnModelCreated);
        }

        public async Task InitializeViewAttachedAsync(IExplorerViewModel evm)
        {
            if (OnViewAttached != null)
                await evm.Commands.ExecuteAsync(OnViewAttached);
        }


        public List<IViewModelInitializer<IExplorerViewModel>> Initializers
        {
            get { return new List<IViewModelInitializer<IExplorerViewModel>>(); }
        }
    }
}
