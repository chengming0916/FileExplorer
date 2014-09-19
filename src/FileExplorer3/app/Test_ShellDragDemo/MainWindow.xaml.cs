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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetroLog;

namespace Test_ShellDragDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FileListViewModel flvm1 = new FileListViewModel("FileList#1");
            DataContext = flvm1;

            for (int i = 1; i < 20; i++)
                flvm1.Items.Add(new FileViewModel("C:\\Temp\\" + "FileVM" + i + ".txt"));     
        }
    }
}
