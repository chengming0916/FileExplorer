using Caliburn.Micro;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using FileExplorer.WPF.Views;
using FileExplorer.WPF.Views.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.WPF.ViewModels;
using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.Models;

namespace FileExplorer.WPF.UserControls
{

    public class Explorer : ContentControl
    {
        public enum ExplorerMode { Unknown, Normal, ToolWindow }
        #region Constructors

        static Explorer()
        {
            new FileExplorerBootStrapper();//.Initialize();
        }

        public Explorer()
        {
            _wm = new WindowManager();
            _events = new EventAggregator();
            _evm = new ExplorerViewModel(new ExplorerInitializer(_wm, _events, new IEntryModel[] { }));
        }

        #endregion

        #region Methods

        public static void OnPropertiesChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Explorer exp = sender as Explorer;

            if (args.Property == RootDirectoriesProperty)
            {
                IEntryModel[] newRootDirs = args.NewValue as IEntryModel[];
                if (newRootDirs != null && newRootDirs.Length > 0)
                    exp._evm.Commands.Execute(
                        new IScriptCommand[] 
                    { 
                        FileExplorer.Script.Explorer.ChangeRoot(ChangeType.Changed, newRootDirs),
                        //FileExplorer.ViewModels.Explorer.GoTo(newRootDirs.First())
                    }
                        );
            }
            else if (args.Property == ModeProperty)
            {
                switch ((ExplorerMode)args.NewValue)
                {
                    case ExplorerMode.Normal: exp.Content = new ExplorerView(); break;
                    case ExplorerMode.ToolWindow: exp.Content = new ToolWindow(); break;
                    default: exp.Content = null; break;
                }

                if (exp.Content != null)
                    Caliburn.Micro.Bind.SetModel(exp.Content as DependencyObject, exp._evm);
            }
        }

        #endregion

        #region Data

        IWindowManager _wm;
        IEventAggregator _events;
        IExplorerViewModel _evm;

        #endregion

        #region Public Properties

        public IExplorerViewModel ViewModel { get { return _evm; } }

        public static DependencyProperty ModeProperty = DependencyProperty.Register("Mode",
            typeof(ExplorerMode), typeof(Explorer), new PropertyMetadata(ExplorerMode.Unknown, OnPropertiesChanged));

        public ExplorerMode Mode
        {
            get { return (ExplorerMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static DependencyProperty RootDirectoriesProperty = DependencyProperty.Register("RootDirectories",
            typeof(IEntryModel[]), typeof(Explorer), new PropertyMetadata(null, OnPropertiesChanged));

        public IEntryModel[] RootDirectories
        {
            get { return (IEntryModel[])GetValue(RootDirectoriesProperty); }
            set { SetValue(RootDirectoriesProperty, value); }
        }


     
        #endregion

    }
}
