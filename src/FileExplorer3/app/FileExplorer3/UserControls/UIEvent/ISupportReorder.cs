using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.ViewModels.Helpers
{

   
    public interface ISupportReorderHelper
    {
        ISupportReorder ReorderHelper { get; }
    }

    public interface ISupportReorder
    {
        bool IsReorderable { get; }
        IDraggable GetReorderable();
        bool CanReorder(IDraggable sourceDraggable, IDraggable targetDraggable);
        void Reorder(IDraggable sourceDraggable, IDraggable targetDraggable);
    }


}
