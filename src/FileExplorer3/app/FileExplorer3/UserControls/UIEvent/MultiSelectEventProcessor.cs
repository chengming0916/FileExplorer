using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
{
    public class MultiSelectEventProcessor : UIEventProcessorBase
    {
        public MultiSelectEventProcessor(ICommand unselectAllCommand = null)
        {
            OnMouseDrag = new BeginSelect(unselectAllCommand);
            OnMouseMove = new ContinueSelect();
            OnMouseUp = new EndSelect();
        }
    }
}
