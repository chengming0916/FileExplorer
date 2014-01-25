﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace TestApp.WPF
{
    public class AppWindowManager : WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = base.EnsureWindow(model, view, isDialog);

            if (model is FileExplorer.ViewModels.MessageDialogViewModel)
            {
                window.SizeToContent = SizeToContent.Height;
                window.Width = 300;
            }
            else
            {
                window.SizeToContent = SizeToContent.Manual;

                if (model is FileExplorer.ViewModels.ExplorerViewModel)
                {
                    window.Width = 800; window.Height = 500;
                }
                else if (model is FileExplorer.ViewModels.LoginViewModel)
                {
                    window.Width = 800; window.Height = 500;
                }
                else
                {
                    window.Width = 500; window.Height = 500;
                }
            }

            return window;
        }
    }
}