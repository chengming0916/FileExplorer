﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {            
            base.OnStartup(e);
            //PresentationTraceSources.DataBindingSource.Listeners.Add(
            //        new ConsoleTraceListener());

            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.All;
        }
    }
}
