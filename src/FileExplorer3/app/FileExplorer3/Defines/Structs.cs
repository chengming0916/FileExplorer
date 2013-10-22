using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Defines
{
    public class ViewChangedEvent
    {
        string ViewMode { get; set; }
        string OldViewMode { get; set; }

        public ViewChangedEvent(string viewMode, string oldViewMode)
        {
            ViewMode = viewMode;
            OldViewMode = oldViewMode;
        }
    }
}
