using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.WPF.BaseControls.Menu;

namespace FileExplorer.WPF.BaseControls
{
    public class ContextMenuEventProcessor : UIEventProcessorBase
    {
        public ContextMenuEventProcessor()
        {
            _processEvents.AddRange(
                new[] {
                 FrameworkElement.MouseRightButtonUpEvent
                }
             );
        }

        protected override FileExplorer.Script.IScriptCommand onEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "MouseRightButtonUp": return new ShowContextMenu(ContextMenu);
            }

            return base.onEvent(eventId);

        }

        public static DependencyProperty ContextMenuProperty =
         DependencyProperty.Register("ContextMenu", typeof(ContextMenu),
         typeof(DragDropEventProcessor), new PropertyMetadata(null));

        public ContextMenu ContextMenu
        {
            get { return (ContextMenu)GetValue(ContextMenuProperty); }
            set { SetValue(ContextMenuProperty, value); }
        }
    }
}
