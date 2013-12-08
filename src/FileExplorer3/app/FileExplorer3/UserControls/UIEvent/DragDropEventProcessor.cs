using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileExplorer.BaseControls;
using FileExplorer.BaseControls.DragnDrop;

namespace FileExplorer.BaseControls
{
    public class DragDropEventProcessor : UIEventProcessorBase
    {
        public DragDropEventProcessor()
        {

        }

        public void SetEnableDrag(bool enable)
        {
            if (enable)
            {
                //Register Drag
                OnPreviewMouseDown = new RecordStartSelectedItem();
                OnMouseDrag = new BeginDrag();
                OnMouseUp = new EndDrag();
                OnMouseMove = new ContinueDrag();
            }
            else
            {
                //Unregister Drag
                OnPreviewMouseDown = ScriptCommands.NoCommand;
                OnMouseDrag = ScriptCommands.NoCommand;
                OnMouseUp = ScriptCommands.NoCommand;
                OnMouseMove = ScriptCommands.NoCommand;
            }
        }

        public void SetEnableDrop(bool enable)
        {
            if (enable)
            {
                //Register Drag                
                OnMouseDragEnter = new QueryDragDropEffects(QueryDragDropEffectMode.Enter);
                OnMouseDragOver = new UpdateAdorner();
                OnMouseDragLeave = new QueryDragDropEffects(QueryDragDropEffectMode.Leave);
                OnMouseDrop = new BeginDrop();

            }
            else
            {
                //Unregister Drag                
                OnMouseDragEnter = ScriptCommands.NoCommand;
                OnMouseDragOver = ScriptCommands.NoCommand;
                OnMouseDragLeave = ScriptCommands.NoCommand;
                OnMouseDrop = ScriptCommands.NoCommand;
            }
        }


        public static void OnEnableDragChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var sender = (DragDropEventProcessor)s;
            sender.SetEnableDrag((bool)e.NewValue);
        }

        public static DependencyProperty EnableDragProperty =
            DependencyProperty.Register("EnableDrag", typeof(bool),
            typeof(DragDropEventProcessor), new PropertyMetadata(false, OnEnableDragChanged));

        public bool EnableDrag
        {
            get { return (bool)GetValue(EnableDragProperty); }
            set { SetValue(EnableDragProperty, value); }
        }

        public static void OnEnableDropChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var sender = (DragDropEventProcessor)s;
            sender.SetEnableDrop((bool)e.NewValue);
        }

        public static DependencyProperty EnableDropProperty =
            DependencyProperty.Register("EnableDrop", typeof(bool),
            typeof(DragDropEventProcessor), new PropertyMetadata(false, OnEnableDropChanged));

        public bool EnableDrop
        {
            get { return (bool)GetValue(EnableDropProperty); }
            set { SetValue(EnableDropProperty, value); }
        }
    }
}
