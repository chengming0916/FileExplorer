using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls.DragnDrop
{
    public class DragDropEventProcessor : UIEventProcessorBase
    {
        public DragDropEventProcessor()
        {
            OnPreviewMouseDown = new RecordStartSelectedItem();
            OnMouseDrag = new BeginDrag();
            OnMouseUp = new EndDrag();
            //OnMouseDragOver = new QueryDragOver();
            OnMouseDragOver = new UpdateAdorner();            
            OnMouseDragEnter = new QueryDragDropEffects();
            OnMouseDragLeave = new HideAdorner();
            OnMouseDrop = new BeginDrop();
        }
    }
}
