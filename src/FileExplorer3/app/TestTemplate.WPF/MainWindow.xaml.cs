﻿using System;
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
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //sbar.Items.Add(new StatusbarItemEx() { Content = "Add", Type = FileExplorer.Defines.DisplayType.Text, Header = "New" });
        }
    }
}
