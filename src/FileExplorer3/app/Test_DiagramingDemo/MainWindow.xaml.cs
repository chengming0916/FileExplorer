﻿using FileExplorer;
using FileExplorer.UIEventHub;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
using MetroLog;
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

namespace DiagramingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            DataContext = new CanvasViewModel();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var ra = new ResizeItemAdorner(cc) { };
            cc.Content = ra;
            ra.SetValue(ResizeItemAdorner.SelectedItemProperty, (DataContext as CanvasViewModel).Items[0] as IResizable);
            ra.SetTargetItem((DataContext as CanvasViewModel).Items[0] as IResizable);
        }
    }
}
