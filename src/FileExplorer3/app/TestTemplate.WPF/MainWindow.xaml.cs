using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileExplorer.UserControls;

namespace TestTemplate.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //UITools.FindVisualChild<
            List<FakeViewModel> fvm = new List<FakeViewModel>();
            fvm.Add(new FakeViewModel("Root", "Root1", "Root2"));
            for (int i = 1; i < 10; i++)
                fvm.Add(new FakeViewModel("Sub" + i.ToString(), "Sub" + i.ToString() + "1", "Sub" + i.ToString() + "2"));            
            breadcrumbCore.ItemsSource = fvm;
            
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //sbar.Items.Add(new StatusbarItemEx() { Content = "Add", Type = FileExplorer.Defines.DisplayType.Text, Header = "New" });
        }
    }
}
