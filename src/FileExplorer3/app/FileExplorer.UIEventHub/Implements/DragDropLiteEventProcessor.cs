using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.UIEventHub;
using FileExplorer.WPF.BaseControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.WPF.BaseControls
{


    public class DragDropLiteEventProcessor : UIEventProcessorBase
    {
        public DragDropLiteEventProcessor()
        {
            _processEvents.AddRange(
                new[] {
                    FrameworkElement.KeyDownEvent,
                    FrameworkElement.PreviewMouseDownEvent,
                    UIEventHub.MouseDragEvent,
                    FrameworkElement.PreviewMouseUpEvent,
                    FrameworkElement.MouseMoveEvent,
                    FrameworkElement.MouseLeaveEvent,

                    FrameworkElement.TouchLeaveEvent,
                    UIEventHub.TouchDragEvent,
                    FrameworkElement.PreviewTouchDownEvent,
                    FrameworkElement.TouchMoveEvent,
                    FrameworkElement.TouchUpEvent, //Not Preview or it would trigger parent's PreviewTouchUp first.
                    FrameworkElement.GiveFeedbackEvent
                }
             );

            Print.PrintConsoleAction = c => Console.WriteLine(c);
        }

        #region Methods

        #endregion

        #region Public Properties

        public static DependencyProperty EnableTouchProperty =
            DependencyProperty.Register("EnableTouch", typeof(bool), typeof(DragDropLiteEventProcessor),
            new PropertyMetadata(true));

        public bool EnableTouch
        {
            get { return (bool)GetValue(EnableTouchProperty); }
            set { SetValue(EnableTouchProperty, value); }
        }

        public static DependencyProperty EnableDragProperty =
           DragDropEventProcessor.EnableDragProperty.AddOwner(typeof(DragDropLiteEventProcessor));

        public bool EnableDrag
        {
            get { return (bool)GetValue(EnableDragProperty); }
            set { SetValue(EnableDragProperty, value); }
        }

        public static DependencyProperty EnableDropProperty =
            DragDropEventProcessor.EnableDropProperty.AddOwner(typeof(DragDropLiteEventProcessor));

        public bool EnableDrop
        {
            get { return (bool)GetValue(EnableDropProperty); }
            set { SetValue(EnableDropProperty, value); }
        }

        #endregion
    }
}
