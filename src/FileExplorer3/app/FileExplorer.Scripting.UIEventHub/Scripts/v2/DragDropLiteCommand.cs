using FileExplorer.Script;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        public static IScriptCommand StartDragDropLite(string dragSourceVariable = "{ISupportDrag}", 
            IScriptCommand nextCommand = null, IScriptCommand failCommand = null)
        {
            return new DragDropLiteCommand()
            {
                State = DragDropState.Start,
                DragSourceKey = dragSourceVariable,
                NextCommand = (ScriptCommandBase)nextCommand,
                FailCommand = (ScriptCommandBase)failCommand
            };
        }
    }

    public enum DragDropState { Start, End }

    public class DragDropLiteCommand : UIScriptCommandBase<Control, RoutedEventArgs>
    {
        public DragDropState State { get; set; }

        /// <summary>
        /// Point to DataContext (ISupportDrag) to initialize the drag, default = "{ISupportDrag}".
        /// </summary>
        public string DragSourceKey { get; set; }

        public ScriptCommandBase FailCommand { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<DragDropLiteCommand>();

        public DragDropLiteCommand()
            : base("DragDropLiteCommand")
        {
            DragSourceKey = "{ISupportDrag}";
        }

        protected override Script.IScriptCommand executeInner(IParameterDic pm, Control sender,
            RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            if (State == DragDropState.Start)
            {
                ISupportDrag isd = pm.GetValue<ISupportDrag>(DragSourceKey);
                if (DragLiteParameters.DragMode == DragMode.None && isd != null)
                {
                    logger.Info(State.ToString());
                    DragLiteParameters.DraggingItems = isd.GetDraggables();
                    //DragLiteParameters.DragMode = DragMode.Lite;
                    DragLiteParameters.Effects = isd.QueryDrag(DragLiteParameters.DraggingItems);
                    DragLiteParameters.DragSource = isd;
                    IDataObject dataObj = DragLiteParameters.DragSource is ISupportShellDrag ?
                        (DragLiteParameters.DragSource as ISupportShellDrag).GetDataObject(DragLiteParameters.DraggingItems) : null;

                    pm.SetValue(InputKey, new DragInput(input, dataObj, DragDropEffects.Copy, (eff) => { }));

                    return NextCommand;
                }
            }
            return FailCommand;
        }
    }
}
