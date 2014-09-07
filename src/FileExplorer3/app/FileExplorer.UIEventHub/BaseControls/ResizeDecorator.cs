using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.WPF.BaseControls
{
    public class ResizeDecorator : Control
    {
        static ResizeDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeDecorator), new FrameworkPropertyMetadata(typeof(ResizeDecorator)));
        }
    }
}
