using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.WPF
{
    public interface IPanelLayoutHelper
    {
        ChildInfo this[int idx] { get; }
        void ResetLayout();
        Size Extent { get; }

        Size Measure(Size availableSize);
        Size Arrange(Size finalSize);
    }
}
