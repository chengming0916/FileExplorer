﻿using Caliburn.Micro;
using FileExplorer.Defines;
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
        string ConfigurationPath { get; set; }

        IExplorerInitializer Clone();

        List<IViewModelInitializer<IExplorerViewModel>> Initializers { get; }
    }

    public class ExplorerInitializer : IExplorerInitializer
    {
        #region Constructors

        public ExplorerInitializer(IWindowManager wm, IEventAggregator events, IEntryModel[] rootModels,
            string configurationPath = null,
            params IViewModelInitializer<IExplorerViewModel>[] initializers)
        {
            WindowManager = wm;
            Events = events;
            RootModels = rootModels;
            Initializers = new List<IViewModelInitializer<IExplorerViewModel>>(initializers);
            ConfigurationPath = configurationPath;
        }

        protected ExplorerInitializer(IExplorerInitializer initializer)
            : this(initializer.WindowManager, initializer.Events, initializer.RootModels, 
            initializer.ConfigurationPath,
            initializer.Initializers.ToArray())
        {

        }

        #endregion

        #region Methods

        public IExplorerInitializer Clone()
        {
            return new ExplorerInitializer(WindowManager, Events, RootModels, ConfigurationPath, Initializers.ToArray());
        }


        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IEventAggregator Events { get; set; }
        public IWindowManager WindowManager { get; set; }
        public IEntryModel[] RootModels { get; set; }
        public IConfiguration Parameters { get; set; }
        public List<IViewModelInitializer<IExplorerViewModel>> Initializers { get; set; }
        public string ConfigurationPath { get; set; }

        #endregion



      
    }
}
