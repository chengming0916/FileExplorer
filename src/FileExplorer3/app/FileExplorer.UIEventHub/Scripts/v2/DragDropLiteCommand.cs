using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        public static IScriptCommand StartDragDropLite(string dragSourceVariable = "{ISupportDrag}",
            IScriptCommand nextCommand = null, IScriptCommand failCommand = null)
        {
            return new DragDropLiteCommand()
            {
                State = DragDropState.StartCanvas,
                DragSourceKey = dragSourceVariable,
                NextCommand = (ScriptCommandBase)nextCommand,
                FailCommand = (ScriptCommandBase)failCommand
            };
        }


        public static IScriptCommand EndDragDropCanvas(IScriptCommand nextCommand = null)
        {
            return new DragDropLiteCommand()
            {
                State = DragDropState.EndCanvas,
                NextCommand = (ScriptCommandBase)nextCommand,
            };
        }

        public static IScriptCommand CancelDragDropCanvas(IScriptCommand nextCommand = null)
        {
            return new DragDropLiteCommand()
            {
                State = DragDropState.CancelCanvas,
                NextCommand = (ScriptCommandBase)nextCommand,
            };
        }
    }

    public enum DragDropState { StartCanvas, EndCanvas, CancelCanvas }

    public class DragDropLiteCommand : UIScriptCommandBase<Control, RoutedEventArgs>
    {
        public DragDropState State { get; set; }

        /// <summary>
        /// Point to DataContext (ISupportDrag) to initialize the drag, default = "{ISupportDrag}".
        /// </summary>
        public string DragSourceKey { get; set; }

        /// <summary>
        /// Point to DataContext (ISupportDrop) to initialize the drag, default = "{ISupportDrop}".
        /// </summary>
        public string DropTargetKey { get; set; }


        /// <summary>
        /// Current position relative to sender, adjusted with scrollbar position.
        /// </summary>
        public string CurrentPositionAdjustedKey { get; set; }

        public static string DragDropModeKey { get; set; }
        public static string DragDropDraggingItemsKey { get; set; }
        public static string DragDropEffectsKey { get; set; }
        public static string DragDropDragSourceKey { get; set; }
        public static string DragDropStartPositionKey { get; set; }

        public ScriptCommandBase FailCommand { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<DragDropLiteCommand>();

        static DragDropLiteCommand()
        {
            DragDropModeKey = "{DragDrop.Mode}";
            DragDropDraggingItemsKey = "{DragDrop.DraggingItems}";
            DragDropEffectsKey = "{DragDrop.Effects}";
            DragDropDragSourceKey = "{DragDrop.DragSource}";
            DragDropStartPositionKey = "{DragDrop.StartPosition}";
        }

        public DragDropLiteCommand()
            : base("DragDropLiteCommand")
        {
            DragSourceKey = "{ISupportDrag}";
            DropTargetKey = "{ISupportDrop}";
            CurrentPositionAdjustedKey = "{CurrentPositionAdjusted}";
        }


        private bool dragStart(ParameterDic pm, IUIInput input, string mode)
        {
            ISupportDrag isd = pm.GetValue<ISupportDrag>(DragSourceKey);
            if (DragLiteParameters.DragMode == DragMode.None && isd != null)
            {


                IDataObject dataObj = DragLiteParameters.DragSource is ISupportShellDrag ?
                    (DragLiteParameters.DragSource as ISupportShellDrag).GetDataObject(DragLiteParameters.DraggingItems) : null;

                pm.SetValue(DragDropModeKey, mode);
                pm.SetValue(DragDropDraggingItemsKey, isd.GetDraggables());
                pm.SetValue(DragDropEffectsKey, isd.QueryDrag(DragLiteParameters.DraggingItems));
                pm.SetValue(DragDropDragSourceKey, isd);
                pm.SetValue(DragDropStartPositionKey, pm.GetValue<Point>(CurrentPositionAdjustedKey));
                pm.SetValue(InputKey, new DragInput(input, dataObj, DragDropEffects.Copy, (eff) => { }));

                return true;
            }

            return false;
        }

        private void dragEnd(ParameterDic pm)
        {
            pm.SetValue<object>(DragDropModeKey, null);
            pm.SetValue<object>(DragDropDraggingItemsKey, null);
            pm.SetValue<object>(DragDropEffectsKey, null);
            pm.SetValue<object>(DragDropDragSourceKey, null);
            pm.SetValue<object>(DragDropStartPositionKey, null);
        }

        protected override IScriptCommand executeInner(ParameterDic pm, Control sender,
            RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            if (!pm.HasValue<ParameterDic>("{DragDrop}"))
                ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false).Execute(pm);
            if (!pm.HasValue<Point>(CurrentPositionAdjustedKey))
                HubScriptCommands.ObtainPointerPosition().Execute(pm);

            logger.Debug(State.ToString());
            switch (State)
            {
                case DragDropState.StartCanvas:
                    if (dragStart(pm, input, "Canvas"))
                    {
                        foreach (var item in
                             pm.GetValue<IEnumerable<IDraggable>>(DragDropDraggingItemsKey)
                             .Where(i => i is IPositionAware))
                            item.IsDragging = true;
                        return NextCommand;
                    }
                    else return FailCommand;

                case DragDropState.EndCanvas:
                case DragDropState.CancelCanvas:
                    //foreach (var item in pm.GetValue<IEnumerable<IDraggable>>(DragDropDraggingItemsKey))
                    //    item.IsDragging = true;

                    if (pm.HasValue<Point>(DragDropStartPositionKey))
                    {
                        Point currentPosition = pm.GetValue<Point>(CurrentPositionAdjustedKey);
                        Point startPosition = pm.GetValue<Point>(DragDropStartPositionKey);
                        Vector movePt = currentPosition - startPosition;

                        var items = pm.GetValue<IEnumerable<IDraggable>>(DragDropDraggingItemsKey)
                            .Where(i => i is IPositionAware);
                        foreach (var item in items)
                            item.IsDragging = false;

                        if (State == DragDropState.EndCanvas)
                            foreach (var posAwareItem in items.Cast<IPositionAware>())
                                posAwareItem.Position = new Point(posAwareItem.Position.X + movePt.X,
                                    posAwareItem.Position.Y + movePt.Y);

                    }

                    dragEnd(pm);
                    return NextCommand;

                default: return ResultCommand.Error(new NotSupportedException(State.ToString()));
            }


        }
    }
}
