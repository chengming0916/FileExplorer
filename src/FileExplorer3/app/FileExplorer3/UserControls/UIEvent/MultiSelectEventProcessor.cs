using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FileExplorer.BaseControls.MultiSelect;

namespace FileExplorer.BaseControls
{
    public class MultiSelectEventProcessor : UIEventProcessorBase
    {
        public MultiSelectEventProcessor()
        {
            OnPreviewMouseDown = new SetHandledIfNotFocused();
            OnMouseDrag = new BeginSelect();
            OnMouseMove = new ContinueSelect();
            OnMouseUp = new EndSelect();
        }

        public static void OnCommandChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MultiSelectEventProcessor)s;
            (sender.OnMouseDrag as BeginSelect).UnselectCommand = sender.UnselectAllCommand;
            (sender.OnMouseUp as EndSelect).UnselectCommand = sender.UnselectAllCommand;
        }

        public static DependencyProperty UnselectAllCommandProperty =
            DependencyProperty.Register("UnselectAllCommand", typeof(ICommand), 
            typeof(MultiSelectEventProcessor), new PropertyMetadata(OnCommandChanged));

        public ICommand UnselectAllCommand
        {
            get { return (ICommand)GetValue(UnselectAllCommandProperty); }
            set { SetValue(UnselectAllCommandProperty, value); }
        }
    }
}
