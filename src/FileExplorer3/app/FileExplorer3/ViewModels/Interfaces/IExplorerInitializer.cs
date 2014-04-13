using Caliburn.Micro;
using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public interface IExplorerInitializer
    {
        IEventAggregator Events { get; set; }
        IWindowManager WindowManager { get; set; }
        IEntryModel[] RootModels { get; set; }

        IExplorerInitializer Clone();

        List<IViewModelInitializer<IExplorerViewModel>> Initializers { get; }
    }

    public class ExplorerInitializer : IExplorerInitializer
    {
        #region Constructors

        public ExplorerInitializer(IWindowManager wm, IEventAggregator events, IEntryModel[] rootModels,
            params IViewModelInitializer<IExplorerViewModel>[] initializers)
        {
            WindowManager = wm;
            Events = events;
            RootModels = rootModels;
            Initializers = new List<IViewModelInitializer<IExplorerViewModel>>(initializers);
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


        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; set; }
        public IWindowManager WindowManager { get; set; }
        public IEntryModel[] RootModels { get; set; }
        public List<IViewModelInitializer<IExplorerViewModel>> Initializers { get; set; }

        #endregion



    }
}
