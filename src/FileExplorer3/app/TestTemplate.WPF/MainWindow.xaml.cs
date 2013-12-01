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
using FileExplorer;
using Cofe.Core.Script;

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
            setupBreadcrumb();
            setupBreadcrumbTree();
            setupDragAndDrop();
        }        

        private void setupDragAndDrop()
        {
            lvDnd1.AllowDrop = true;
            lvDnd2.AllowDrop = true;
            ScriptRunner runner = new ScriptRunner();

            var adapter1 = new UIEventAdapter(runner, lvDnd1, true, MultiSelectEventProcessor.Instance);
            //var adapter2 = new UIEventAdapter(runner, lvDnd2, true, DebugUIEventProcessor.Instance);

            //lvDnd1.Loaded += (o, e) =>
            //    {
            //        var pd = new UIParameterDic() { Sender = lvDnd1 };
            //        pd["IsSelecting"] = true;
            //        pd["StartPosition"] = new Point(30, 30);
            //        pd["EndPosition"] = new Point(100, 100);

            //        MultiSelectScriptCommands.AttachAdorner.Execute(pd);

            //        if (pd["SelectionAdorner"] == null)
            //            throw new Exception();

            //        lvDnd1.Dispatcher.BeginInvoke(new System.Action(() =>
            //            {
            //                MultiSelectScriptCommands.UpdateAdorner.Execute(pd);
            //            }));
                    
            //        //MultiSelectScriptCommands.DetachAdorner.Execute(pd);
            //    };
        }

        private void setupBreadcrumbTree()
        {
            var tvModel = new TreeViewModel();
            btreeTab.DataContext = tvModel;
            bexp.AddValueChanged(ComboBox.SelectedValueProperty, (o, e) =>
                {
                    string path = bexp.SelectedValue as string;
                    if (path != null)
                        tvModel.Selection.AsRoot().SelectAsync(path);
                });
            selectBTreeItem.Click += (RoutedEventHandler)((o, e) =>
                {
                    tvModel.Selection.AsRoot().SelectAsync(selectBTreeCombo.Text);
                });
            //var items = TreeViewModel.GenerateFakeTreeViewModels().RootItems;
            //items[0].Subitems[1].IsExpanded = true;
            //items[0].Subitems[1].Subitems[2].IsExpanded = true;
            //items[0].Subitems[1].Subitems[2].Subitems[3].IsExpanded = true;
            //items[0].Subitems[1].Subitems[2].Subitems[3].Subitems[4].SelectionHelper.IsSelected = true;
            //btree.ItemsSource = items;

        }


        private void setupBreadcrumb()
        {
            FakeViewModel fvm = new FakeViewModel("Root");
            for (int i = 1; i < 10; i++)
                fvm.SubDirectories.Add(new FakeViewModel("Sub" + i.ToString(), "Sub" + i.ToString() + "1", "Sub" + i.ToString() + "2"));
            breadcrumbCore.ItemsSource = fvm.SubDirectories;
            breadcrumbCore.RootItemsSource = fvm.SubDirectories;

            //SuggestBoxes            
            suggestBoxDummy.SuggestSources = new List<ISuggestSource>(new[] { new DummySuggestSource() });
            suggestBoxAuto.RootItem = fvm;

            suggestBoxAuto2.HierarchyHelper = suggestBoxAuto.HierarchyHelper =
                new PathHierarchyHelper("Parent", "Value", "SubDirectories");

            //suggestBoxAuto2
            suggestBoxAuto2.RootItem = FakeViewModel.GenerateFakeViewModels(TimeSpan.FromSeconds(0.5));
            suggestBoxAuto2.SuggestSources = new List<ISuggestSource>(new[] { new AutoSuggestSource() }); //This is default value, suggest based on HierarchyLister.List()


            //breadcrumb
            breadcrumb1.RootItem = FakeViewModel.GenerateFakeViewModels(TimeSpan.FromSeconds(0));
            breadcrumb2.RootItem = FakeViewModel.GenerateFakeViewModels(TimeSpan.FromSeconds(0));

            bool UseGenericHierarchyHelper = true;

            if (UseGenericHierarchyHelper)
            {
                //Generic version is faster than Nongeneric PathHierarchyHelper.
                //This replaced the ParentPath, ValuePath and SubEntriesPath in markup.
                IHierarchyHelper hierarchyHelper = new PathHierarchyHelper<FakeViewModel>("Parent", "Value", "SubDirectories");
                //suggestBoxAuto.HierarchyHelper = hierarchyHelper;
                //suggestBoxAuto2.HierarchyHelper = hierarchyHelper;
                breadcrumb1.HierarchyHelper = hierarchyHelper;
                breadcrumb2.HierarchyHelper = hierarchyHelper;
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //sbar.Items.Add(new StatusbarItemEx() { Content = "Add", Type = FileExplorer.Defines.DisplayType.Text, Header = "New" });
        }
    }
}
