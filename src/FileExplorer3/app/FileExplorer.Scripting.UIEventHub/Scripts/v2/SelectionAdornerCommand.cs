using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
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
        public static IScriptCommand AttachSelectionAdorner(IScriptCommand nextCommand = null)
        {
            return new SelectionAdornerCommand()
            {
                AdornerMode = UIEventHub.AdornerMode.Attach,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand DettachSelectionAdorner(IScriptCommand nextCommand = null)
        {
            return new SelectionAdornerCommand()
            {
                AdornerMode = UIEventHub.AdornerMode.Detach,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand UpdateSelectionAdorner(
            string selectionBoundAdjustedVariable = "{SelectionBoundAdjusted}", IScriptCommand nextCommand = null)
        {
            return new SelectionAdornerCommand()
            {
                AdornerMode = UIEventHub.AdornerMode.Update,
                SelectionBoundAdjustedKey = selectionBoundAdjustedVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }


    public enum AdornerMode { Attach, Update, Detach }

    public class SelectionAdornerCommand : UIScriptCommandBase<ItemsControl, RoutedEventArgs>
    {
        public AdornerMode AdornerMode { get; set; }

        /// <summary>
        /// If attach, the selection adorner (of type SelectionAdorner) will be set to the key, 
        /// If update, 
        /// If detach, the SelectionAdorner will be point to null.
        /// Default = "{SelectionAdorner}".
        /// </summary>
        public string SelectionAdornerKey { get; set; }

        /// <summary>
        ///Required when update, assigned by ObtainPointerPosition command.
        ///SelectionBounds (Rect) that used to calcuate selected items, took scroll bar position into account.                       
        /// </summary>
        public string SelectionBoundAdjustedKey { get; set; }

        /// <summary>
        /// Start position relative to sender, adjusted with scrollbar position.
        /// </summary>
        public string StartPositionAdjustedKey { get; set; }

        /// <summary>
        /// Current position relative to sender, regardless the scrollbar.
        /// </summary>
        public string CurrentPositionKey { get; set; }


        public SelectionAdornerCommand()
            : base("SelectionAdornerCommand")
        {
            SelectionAdornerKey = "{SelectionAdorner}";
            SelectionBoundAdjustedKey = "{SelectionBoundAdjusted}";
            StartPositionAdjustedKey = "{StartPositionAdjusted}";
            CurrentPositionKey = "{CurrentPosition}";
        }

        protected override Script.IScriptCommand executeInner(ParameterDic pm, ItemsControl sender,
            RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            var scp = ControlUtils.GetScrollContentPresenter(sender);
            if (scp != null)
            {
                AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(sender);

                switch (AdornerMode)
                {
                    case UIEventHub.AdornerMode.Attach:
                        if (adornerLayer == null)
                            return ResultCommand.Error(new Exception("Adorner layer not found."));
                        if (AttachedProperties.GetSelectionAdorner(scp) == null)
                        {

                            //Create and register adorner.
                            SelectionAdorner adorner = new SelectionAdorner(scp);
                            pm.SetValue(SelectionAdornerKey, adorner);
                            AttachedProperties.SetSelectionAdorner(scp, adorner);
                            AttachedProperties.SetLastScrollContentPresenter(sender, scp); //For used when detach.

                            adornerLayer.Add(adorner);
                        }
                        break;

                    case UIEventHub.AdornerMode.Detach:
                        var lastScp = AttachedProperties.GetLastScrollContentPresenter(sender); //For used when detach.
                        var lastAdorner = AttachedProperties.GetSelectionAdorner(scp);
                        if (lastAdorner != null)
                            adornerLayer.Remove(lastAdorner);

                        AttachedProperties.SetLastScrollContentPresenter(sender, null);
                        AttachedProperties.SetSelectionAdorner(scp, null);
                        pm.SetValue<Object>(SelectionAdornerKey, null);
                        break;

                    case UIEventHub.AdornerMode.Update:
                        var updateAdorner = pm.GetValue<SelectionAdorner>(SelectionAdornerKey) ??
                            AttachedProperties.GetSelectionAdorner(scp);

                        if (updateAdorner == null)
                            return ResultCommand.Error(new Exception("Adorner not found."));

                        updateAdorner.IsSelecting = AttachedProperties.GetIsSelecting(sender);
                        Rect selectionBound = pm.GetValue<Rect>(SelectionBoundAdjustedKey);
                        Point startAdjusted = pm.GetValue<Point>("{StartPosition}");
                        Point current = pm.GetValue<Point>(CurrentPositionKey);
                        updateAdorner.SetValue(SelectionAdorner.StartPositionProperty, startAdjusted);
                        updateAdorner.SetValue(SelectionAdorner.EndPositionProperty, current);
                        break;

                }



            }
            return NextCommand;
        }

    }
}
