using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Defines
{
    public enum TransferMode { Copy, Move, Rename, Link }

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
