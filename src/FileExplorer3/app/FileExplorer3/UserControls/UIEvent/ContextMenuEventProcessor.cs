//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using FileExplorer.BaseControls.Menu;

//namespace FileExplorer.BaseControls
//{
//    public class ContextMenuEventProcessor : UIEventProcessorBase
//    {
//        public ContextMenuEventProcessor()
//        {
//            _processEvents.AddRange(
//                new [] {
//                 FrameworkElement.MouseUpEvent
//                }
//             );
//        }

//        public override Cofe.Core.Script.IScriptCommand OnEvent(RoutedEvent eventId)
//        {
//            switch (eventId.Name)
//            {
//                case "MouseUp": return new ShowContextMenu(ContextMenu);
//            }

//            return base.OnEvent(eventId);

//        }

//        public static DependencyProperty ContextMenuProperty =
//         DependencyProperty.Register("ContextMenu", typeof(ContextMenu),
//         typeof(DragDropEventProcessor), new PropertyMetadata(null));

//        public ContextMenu ContextMenu
//        {
//            get { return (ContextMenu)GetValue(ContextMenuProperty); }
//            set { SetValue(ContextMenuProperty, value); }
//        }
//    }
//}
