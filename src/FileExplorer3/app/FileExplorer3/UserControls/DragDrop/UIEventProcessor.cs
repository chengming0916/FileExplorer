using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.BaseControls
{
   
    /// <summary>
    /// Allow one application (e.g. dragging) to handle events from an control.
    /// </summary>
    public interface IUIEventProcessor
    {
        int Priority { get; }
        IRoutedEventHandler OnMouseDrag { get; }
        IRoutedEventHandler OnMouseDragOver { get; }
        IRoutedEventHandler OnMouseDragEnter { get; }
        IRoutedEventHandler OnMouseDragLeave { get; }
        IRoutedEventHandler OnMouseDrop { get; }
        IRoutedEventHandler OnMouseUp { get; }
        IRoutedEventHandler OnMouseDown { get; }
        IRoutedEventHandler OnMouseMove { get; }
    }

    public abstract class UIEventProcessorBase : IUIEventProcessor
    {
        public int Priority { get; protected set; }
        public IRoutedEventHandler OnMouseDrag { get; protected set; }
        public IRoutedEventHandler OnMouseUp { get; protected set; }
        public IRoutedEventHandler OnMouseDown { get; protected set; }
        public IRoutedEventHandler OnMouseMove { get; protected set; }

        public IRoutedEventHandler OnMouseDragEnter { get; protected set; }
        public IRoutedEventHandler OnMouseDragOver { get; protected set; }
        public IRoutedEventHandler OnMouseDragLeave { get; protected set; }
        public IRoutedEventHandler OnMouseDrop { get; protected set; }

    }

    public class DebugUIEventProcessor : UIEventProcessorBase
    {
        public static DebugUIEventProcessor Instance = new DebugUIEventProcessor();

        public DebugUIEventProcessor()
        {
            Priority = 0;
            OnMouseDown = DebugRoutedEventHandler.PrintSourceDC;
            //OnMouseMove = DebugRoutedEventHandler.PrintSourceDC;
            OnMouseDrag = DebugRoutedEventHandler.PrepareDrag;
            OnMouseUp = DebugRoutedEventHandler.PrintSourceDC;

            //OnMouseDragEnter = DebugRoutedEventHandler.PrintSourceDC;
            //OnMouseDragOver = DebugRoutedEventHandler.PrintSourceDC;
            //OnMouseDragLeave = DebugRoutedEventHandler.PrintSourceDC;
            OnMouseDrop = DebugRoutedEventHandler.PrintSourceDC;
        }
    }


}
