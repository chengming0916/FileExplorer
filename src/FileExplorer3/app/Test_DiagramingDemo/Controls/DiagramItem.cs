using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DiagramingDemo
{
    public class DiagramItem : ContentControl
    {
        static DiagramItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramItem), new FrameworkPropertyMetadata(typeof(DiagramItem)));            
        }
    }
}
