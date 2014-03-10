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
        IEventAggregator Events { get; }
        IWindowManager WindowManager { get; }
        IEntryModel[] RootModels { get; }

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

        #endregion

        #region Methods


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
