using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using QuickZip.IO.COFE.UserControls.MVVM;
//using QuickZip.IO.COFE;
//using QuickZip.IO.COFE.UserControls.MVVM.ViewModel;
using QuickZip.UserControls.MVVM;
using QuickZip.UserControls.MVVM.ViewModel;
using QuickZip.UserControls;

namespace Explorer2TestProj
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {            
            InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (o, e) => { this.Close(); }));
            var profile = new ExProfile();
            _evm = null;            
            explr.DataContext = _evm = new ExExplorerViewModel(profile);            
            //evm.ChangeCurrentEntry(d);
        }

        private ExExplorerViewModel _evm = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RoutedEventHandler onLoaded = null;
            onLoaded = (o, e) =>
            {
                explr.Loaded -= onLoaded;

                FileList2 filelist = explr.Template.FindName("PART_FileList", explr) as FileList2;

                _evm.RegisterDragAndDrop(filelist);

                DirectoryTree2 dirTree = explr.Template.FindName("PART_DirectoryTree", explr) as DirectoryTree2;
                _evm.RegisterDragAndDrop(dirTree);


                DataTemplate itemTemplate = explr.FindResource("EntryViewModel_DragDataTemplate") as DataTemplate;
                if (itemTemplate != null)
                    _evm.RegisterDragTemplate(dirTree, itemTemplate);


            };

            explr.Loaded += onLoaded;
        }
    }
}
