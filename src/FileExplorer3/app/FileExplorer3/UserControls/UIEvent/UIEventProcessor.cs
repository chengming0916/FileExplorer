using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core.Script;

namespace FileExplorer.BaseControls
{
   
    /// <summary>
    /// Allow one application (e.g. dragging) to handle events from an control.
    /// </summary>
    public interface IUIEventProcessor
    {
        int Priority { get; }
        IScriptCommand OnEvent(RoutedEvent eventId);
        IEnumerable<RoutedEvent> ProcessEvents { get; }
    }

    public abstract class UIEventProcessorBase : Freezable, IUIEventProcessor
    {
        public int Priority { get; protected set; }
        public IEnumerable<RoutedEvent> ProcessEvents { get { return _processEvents; } }
        protected List<RoutedEvent> _processEvents = new List<RoutedEvent>();

        public UIEventProcessorBase()
        {
     
        }

        public virtual IScriptCommand OnEvent(RoutedEvent eventId)
        {
            return ResultCommand.NoError;
        }

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
    }

    public class DebugUIEventProcessor : UIEventProcessorBase
    {
        public static DebugUIEventProcessor Instance = new DebugUIEventProcessor();

        public override IScriptCommand OnEvent(RoutedEvent eventId)
        {
            switch (eventId.Name)
            {
                case "OnPreviewMouseDown": return ScriptCommands.PrintSourceDC;
                case "OnMouseDrag": return ScriptCommands.PrepareDrag;
                case "OnMouseUp": return ScriptCommands.PrintSourceDC;
                case "OnMouseDrop": return ScriptCommands.PrintSourceDC;
            }

            return base.OnEvent(eventId);
        }

        public DebugUIEventProcessor()
        {
            Priority = 0;         
        }
    }


}
