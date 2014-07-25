using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    public interface IExplorerInitializer
    {
        IEventAggregator Events { get; set; }
        IWindowManager WindowManager { get; set; }
        IEntryModel[] RootModels { get; set; }

        IExplorerInitializer Clone();

        List<IViewModelInitializer<IExplorerViewModel>> Initializers { get; }

        Task InitializeModelCreatedAsync(IExplorerViewModel evm);
        Task InitializeViewAttachedAsync(IExplorerViewModel evm);
    }

    public class ExplorerInitializer : IExplorerInitializer
    {
        #region Constructors

        public ExplorerInitializer(IWindowManager wm, IEventAggregator events, IEntryModel[] rootModels,
            params IViewModelInitializer<IExplorerViewModel>[] initializers)
        {
            WindowManager = wm ?? new WindowManager();
            Events = events ?? new EventAggregator();
            RootModels = rootModels;
            Initializers = new List<IViewModelInitializer<IExplorerViewModel>>(initializers);
        }

        public ExplorerInitializer(IEntryModel[] rootModels,
          params IViewModelInitializer<IExplorerViewModel>[] initializers)
            : this(null, null, rootModels, initializers)
        {

        }

        protected ExplorerInitializer(IExplorerInitializer initializer)
            : this(initializer.WindowManager, initializer.Events, initializer.RootModels,
            initializer.Initializers.ToArray())
        {

        }

        #endregion

        #region Methods

        public IExplorerInitializer Clone()
        {
            return new ExplorerInitializer(WindowManager, Events, RootModels, Initializers.ToArray());
        }

        public Task InitializeModelCreatedAsync(IExplorerViewModel evm)
        {
            return Task.Delay(0);
        }

        public async Task InitializeViewAttachedAsync(IExplorerViewModel evm)
        {
            await Initializers.InitalizeAsync(evm);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; set; }
        public IWindowManager WindowManager { get; set; }
        public IEntryModel[] RootModels { get; set; }
        public IParameters Parameters { get; set; }
        public List<IViewModelInitializer<IExplorerViewModel>> Initializers { get; set; }

        #endregion



        
    }
}
