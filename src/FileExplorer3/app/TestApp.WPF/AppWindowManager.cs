using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using System.Windows.Data;
using FileExplorer.ViewModels;

namespace TestApp
{
    public class AppWindowManager : WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = base.EnsureWindow(model, view, isDialog);

            if (model is FileExplorer.ViewModels.ProgressDialogViewModel)
            {
                window.SizeToContent = SizeToContent.WidthAndHeight;
            }
            else
                if (model is FileExplorer.ViewModels.MessageDialogViewModel)
                {
                    window.SizeToContent = SizeToContent.Height;
                    window.Width = 300;
                }
                else if (model is FileExplorer.ViewModels.AddDirectoryViewModel)
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
                        window.SetBinding(Window.IconProperty, new Binding("CurrentDirectory.Icon") { Mode = BindingMode.OneWay });
                    }
                    else if (model is FileExplorer.ViewModels.TabbedExplorerViewModel)
                    {
                        window.Width = 800; window.Height = 500;
                        window.WindowState = WindowState.Maximized;
                        window.SetBinding(Window.TitleProperty, new Binding("ActiveItem.DisplayName") { Mode = BindingMode.OneWay });
                        window.SetBinding(Window.IconProperty, new Binding("ActiveItem.CurrentDirectory.Icon") { Mode = BindingMode.OneWay });
                    }

                    else if (model is FileExplorer.ViewModels.LoginViewModel)
                    {
                        window.Width = 300; window.Height = 350;
                    }
                    else
                    {
                        window.Width = 500; window.Height = 500;
                    }

                    
                }

            return window;
        }
    }


    //public class TabbedAppWindowManager : TabbedWindowManager
    //{
    //    public TabbedAppWindowManager(ITabbedExplorerViewModel tabExplorer)
    //        : base(tabExplorer)
    //    {

    //    }

    //    protected override Window EnsureWindow(object model, object view, bool isDialog)
    //    {
    //        Window window = base.EnsureWindow(model, view, isDialog);

    //        if (model is FileExplorer.ViewModels.ProgressDialogViewModel)
    //        {
    //            window.SizeToContent = SizeToContent.WidthAndHeight;
    //        }
    //        else
    //            if (model is FileExplorer.ViewModels.MessageDialogViewModel)
    //            {
    //                window.SizeToContent = SizeToContent.Height;
    //                window.Width = 300;
    //            }
    //            else if (model is FileExplorer.ViewModels.AddDirectoryViewModel)
    //            {
    //                window.SizeToContent = SizeToContent.Height;
    //                window.Width = 300;
    //            }
    //            else
    //            {
    //                window.SizeToContent = SizeToContent.Manual;

    //                if (model is FileExplorer.ViewModels.ExplorerViewModel || 
    //                    model is FileExplorer.ViewModels.TabbedExplorerViewModel)
    //                {
    //                    window.Width = 800; window.Height = 500;
    //                }
    //                else if (model is FileExplorer.ViewModels.LoginViewModel)
    //                {
    //                    window.Width = 300; window.Height = 350;
    //                }
    //                else
    //                {
    //                    window.Width = 500; window.Height = 500;
    //                }

    //                window.SetBinding(Window.IconProperty, new Binding("CurrentDirectory.Icon") { Mode = BindingMode.OneWay });
    //            }

    //        return window;
    //    }
    //}
}
