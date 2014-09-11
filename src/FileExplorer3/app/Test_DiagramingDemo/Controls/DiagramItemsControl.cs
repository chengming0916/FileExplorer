using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DiagramingDemo
{
    public class DiagramItemsControl : ItemsControl
    {        
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DiagramItem();
        }
    }
}
