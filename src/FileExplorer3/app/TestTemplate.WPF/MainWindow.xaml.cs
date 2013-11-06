using System;
using System.Collections;
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
using FileExplorer.BaseControls;
using FileExplorer.UserControls;
using FileExplorer.Utils;

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

        //private void test(System.Collections.IEnumerable itemSource, HierarchicalDataTemplate dt)
        //{


        //}

        public class DummySuggestSource : ISuggestSource
        {

            public Task<IList<object>> SuggestAsync(object data, string input, IHierarchyHelper helper)
            {
                return Task.FromResult<IList<object>>(new List<object>()
                {
                     new { Header = input + "-add xyz", Value = input + "xyz" },
                     new { Header = input + "-add abc", Value = input + "abc" }
                });
            }

            public bool RunInDispatcher { get { return false; } }
         
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
            suggestBoxDummy.SuggestSource = new DummySuggestSource();

            suggestBoxAuto.DataContext = fvm;
            suggestBoxAuto.HierarchyHelper = new PathHierarchyHelper<FakeViewModel>("Parent", "Value", "SubDirectories");            

            //suggestBoxAuto2
            suggestBoxAuto2.DataContext = FakeViewModel.GenerateFakeViewModels(TimeSpan.FromSeconds(0.5));
            suggestBoxAuto2.HierarchyHelper = new PathHierarchyHelper<FakeViewModel>("Parent", "Value", "SubDirectories");
            suggestBoxAuto2.SuggestSource = new AutoSuggestSource(); //This is default value.
            



        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //sbar.Items.Add(new StatusbarItemEx() { Content = "Add", Type = FileExplorer.Defines.DisplayType.Text, Header = "New" });
        }
    }
}
