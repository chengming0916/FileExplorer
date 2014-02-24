using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.ViewModels;
using FileExplorer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF.MDI;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MdiWindow.xaml
    /// </summary>
    public partial class MdiWindow : Window
    {
        public MdiWindow()
        {
            InitializeComponent();
        }

        public IEventAggregator _events = new EventAggregator();
        public IWindowManager _windowManager = new WindowManager();
        public IProfile _profileEx = null;

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            if (_profileEx == null)
                _profileEx = new FileSystemInfoExProfile(_events, _windowManager);
            var root = _profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName).Result;
            var viewModel = AppViewModel.initExplorerModel(new ExplorerViewModel(_events, _windowManager, root),
                true, true, new[] { root }, _windowManager);
            var view = new ExplorerView();
            Caliburn.Micro.Bind.SetModel(view, viewModel); //Set the ViewModel using this command.
            Container.Children.Add(new MdiChild
			{
				Title = "Explorer",
                Content = view,
				Width = 500,
				Height = 334,
				Position = new Point(0, 0)
			});
        }
    }
}
