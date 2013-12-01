using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
{
    public class MultiSelectEventProcessor : UIEventProcessorBase
    {
        public static MultiSelectEventProcessor Instance = new MultiSelectEventProcessor();

        public MultiSelectEventProcessor()
        {
            OnMouseDrag = MultiSelectScriptCommands.StartDrag;
            OnMouseMove = MultiSelectScriptCommands.UpdateAdornerPosition;
            OnMouseUp = MultiSelectScriptCommands.EndDrag;
        }
    }
}
