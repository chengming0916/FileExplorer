using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Defines
{
    public enum HierarchicalResult { Parent, Current, Child, Unrelated };
    
    public enum TransferMode { Copy, Move, Rename, Link }

    //ToAdd a template, you have to update DisplayTemplateSelector.cs
    public enum DisplayType { Auto, Text, Number, Kb, Percent, Filename, Boolean }

    public enum ViewMode : int
    {
        vmTile = 13,
        vmGrid = 14,
        vmList = 15,
        vmSmallIcon = 16,
        vmIcon = 48,
        vmLargeIcon = 80,
        vmExtraLargeIcon = 120,
        vmViewer = 256
    }

    public enum SelectionModeEx : int
    {
        Single = 1,
        Multiple,
        Extended,
        SelectionHelper
    }

}
