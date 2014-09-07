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

        public static IScriptCommand UpdateDragDropCanvas(IScriptCommand nextCommand = null)
        {
            return new DragDropLiteCommand()
            {
                State = DragDropState.UpdateCanvas,
                NextCommand = (ScriptCommandBase)nextCommand,
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
    }

    public enum DragDropState { StartCanvas, UpdateCanvas, EndCanvas }

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
                logger.Info(State.ToString());

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

        private void setIsDragging(ParameterDic pm, bool value, bool updateOrgPosition)
        {
            var items = pm.GetValue<IEnumerable<IDraggable>>(DragDropDraggingItemsKey);
            foreach (var item in items)
            {
                if (updateOrgPosition && item is IDraggablePositionAware)
                    (item as IDraggablePositionAware).OriginalPosition =
                        (item as IDraggablePositionAware).Position;

                item.IsDragging = value;
            }
        }

        private void canvasUpdatePosition(ParameterDic pm)
        {
            if (pm.HasValue<Point>(DragDropStartPositionKey))
            {
                Point currentPosition = pm.GetValue<Point>(CurrentPositionAdjustedKey);
                Point startPosition = pm.GetValue<Point>(DragDropStartPositionKey);
                Vector movePt = currentPosition - startPosition;

                var items = pm.GetValue<IEnumerable<IDraggable>>(DragDropDraggingItemsKey);
                foreach (var item in items)
                    if (item is IDraggablePositionAware)
                    {
                        IDraggablePositionAware posAwareItem = item as IDraggablePositionAware;
                        posAwareItem.Position = new Point(posAwareItem.OriginalPosition.X + movePt.X,
                            posAwareItem.OriginalPosition.Y + movePt.Y);
                    }
            }
        }

        private static SelectedItemsAdorner adorner = null;
        protected override IScriptCommand executeInner(ParameterDic pm, Control sender,
            RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            if (!pm.HasValue<ParameterDic>("{DragDrop}"))
                ScriptCommands.AssignGlobalParameterDic("{DragDrop}", false).Execute(pm);
            if (!pm.HasValue<Point>(CurrentPositionAdjustedKey))
                HubScriptCommands.ObtainPointerPosition().Execute(pm);

            switch (State)
            {
                case DragDropState.StartCanvas:
                    if (dragStart(pm, input, "Canvas"))
                    {
                        setIsDragging(pm, true, true);

                        //
                        var scp = ControlUtils.GetScrollContentPresenter(sender);
                        AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(sender);
                        if (adorner == null)
                        {
                            adorner = new SelectedItemsAdorner(scp);
                            adornerLayer.Add(adorner);
                        }
                        adorner.CurrentPosition = pm.GetValue<Point>(CurrentPositionAdjustedKey);
                        adorner.Items = (sender as ItemsControl).ItemsSource;
                        //

                        return NextCommand;
                    }
                    else return FailCommand;

                case DragDropState.UpdateCanvas:
                    //canvasUpdatePosition(pm);
                    if (adorner != null)
                    {
                        adorner.CurrentPosition = pm.GetValue<Point>(CurrentPositionAdjustedKey);
                    }

                    return NextCommand;

                case DragDropState.EndCanvas:
                    setIsDragging(pm, false, false);
                    canvasUpdatePosition(pm);
                    if (adorner != null)
                    {
                        var scp = ControlUtils.GetScrollContentPresenter(sender);
                        AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(sender);
                        adornerLayer.Remove(adorner);
                        adorner = null;                        
                    }

                    dragEnd(pm);
                    return NextCommand;

                default: return ResultCommand.Error(new NotSupportedException(State.ToString()));
            }


        }
    }
}
