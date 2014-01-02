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
        //IScriptCommand OnMouseDrag { get; }
        //IScriptCommand OnMouseDragOver { get; }
        //IScriptCommand OnMouseDragEnter { get; }
        //IScriptCommand OnMouseDragLeave { get; }
        //IScriptCommand OnMouseDrop { get; }
        //IScriptCommand OnMouseUp { get; }
        //IScriptCommand OnPreviewMouseDown { get; }
        //IScriptCommand OnMouseDown { get; }
        //IScriptCommand OnMouseMove { get; }

        //IScriptCommand OnKeyDown { get; }
    }

    public abstract class UIEventProcessorBase : Freezable, IUIEventProcessor
    {
        public int Priority { get; protected set; }
        public IEnumerable<RoutedEvent> ProcessEvents { get { return _processEvents; } }
        protected List<RoutedEvent> _processEvents = new List<RoutedEvent>();

        public UIEventProcessorBase()
        {
            //OnMouseDrag = ScriptCommands.NoCommand;
            //OnMouseDown = ScriptCommands.NoCommand;
            //OnPreviewMouseDown = ScriptCommands.NoCommand;
            //OnMouseUp = ScriptCommands.NoCommand;
            //OnMouseMove = ScriptCommands.NoCommand;

            //OnMouseDragEnter = ScriptCommands.NoCommand;
            //OnMouseDragOver = ScriptCommands.NoCommand;
            //OnMouseDragLeave = ScriptCommands.NoCommand;
            //OnMouseDrop = ScriptCommands.NoCommand;

            //OnKeyDown = ScriptCommands.NoCommand;
        }

        //public IScriptCommand OnMouseDrag { get; protected set; }
        //public IScriptCommand OnMouseUp { get; protected set; }
        //public IScriptCommand OnPreviewMouseDown { get; protected set; }
        //public IScriptCommand OnMouseDown { get; protected set; }
        //public IScriptCommand OnMouseMove { get; protected set; }

        //public IScriptCommand OnMouseDragEnter { get; protected set; }
        //public IScriptCommand OnMouseDragOver { get; protected set; }
        //public IScriptCommand OnMouseDragLeave { get; protected set; }
        //public IScriptCommand OnMouseDrop { get; protected set; }

        //public IScriptCommand OnKeyDown { get; protected set; }

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
            //OnPreviewMouseDown = ScriptCommands.PrintSourceDC;
            ////OnMouseMove = ScriptCommands.PrintSourceDC;
            //OnMouseDrag = ScriptCommands.PrepareDrag;
            //OnMouseUp = ScriptCommands.PrintSourceDC;

            ////OnMouseDragEnter = ScriptCommands.PrintSourceDC;
            ////OnMouseDragOver = ScriptCommands.PrintSourceDC;
            ////OnMouseDragLeave = ScriptCommands.PrintSourceDC;
            //OnMouseDrop = ScriptCommands.PrintSourceDC;
        }
    }


}
