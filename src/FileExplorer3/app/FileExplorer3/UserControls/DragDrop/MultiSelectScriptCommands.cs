using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

namespace FileExplorer.UserControls
{
    public static class MultiSelectScriptCommands
    {
        public static IScriptCommand AttachAdorner = new AttachAdorner();
        public static IScriptCommand DetachAdorner = new DetachAdorner();
        public static IScriptCommand UpdateAdorner = new UpdateAdorner(false);
        public static IScriptCommand UpdateAdornerPosition = new UpdateAdorner(true);
        public static IScriptCommand ObtainPointerPosition = new ObtainPointerPosition();

        public static IScriptCommand StartDrag = new StartDrag();
        public static IScriptCommand EndDrag = new EndDrag();

        public static IScriptCommand HighlightDraggingItems = new UpdateSelection(true);
        public static IScriptCommand SelectDraggingItems = new UpdateSelection(false);

        public static IScriptCommand FindSelectedItems = new FindSelectedItems();
        public static IScriptCommand HighlightItems = new SelectItems(SelectMode.highlight);
        public static IScriptCommand SelectItems = new SelectItems(SelectMode.select);

    }

    #region Attach, Detach and Update Adorner.

    public class AttachAdorner : ScriptCommandBase
    {
        public AttachAdorner() : base("AttachAdorner") { }

        public override bool CanExecute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(c);
            if (adornerLayer == null)
                return false;

            if (scp == null || AttachedProperties.GetSelectionAdorner(scp) != null)
                return false;

            return true;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            if (scp != null)
            {
                AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(c);
                if (adornerLayer == null)
                    return ResultCommand.Error(new Exception("Adorner layer not found."));
                if (AttachedProperties.GetSelectionAdorner(scp) != null)
                    return ResultCommand.Error(new Exception("SelectionAdorner already registered."));

                //Create and register adorner.
                SelectionAdorner adorner = new SelectionAdorner(scp);
                pm["SelectionAdorner"] = adorner;
                AttachedProperties.SetSelectionAdorner(scp, adorner);
                AttachedProperties.SetLastScrollContentPresenter(c, scp); //For used when detach.

                adornerLayer.Add(adorner);

                if (pm.ContainsKey("IsSelecting") && (bool)pm["IsSelecting"])
                    return MultiSelectScriptCommands.UpdateAdornerPosition;
            }
            return ResultCommand.OK;
        }
    }
    public class DetachAdorner : ScriptCommandBase
    {
        public DetachAdorner() : base("DetachAdorner") { }

        public override bool CanExecute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            if (scp == null || AttachedProperties.GetSelectionAdorner(scp) == null)
                return false;

            return true;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            if (scp != null)
            {
                AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(c);
                if (adornerLayer == null)
                    return ResultCommand.Error(new Exception("Adorner layer not found."));

                var lastScp = AttachedProperties.GetLastScrollContentPresenter(c); //For used when detach.
                var lastAdorner = AttachedProperties.GetSelectionAdorner(scp);

                if (lastAdorner != null)
                    adornerLayer.Remove(lastAdorner);

                AttachedProperties.SetLastScrollContentPresenter(c, null);
                AttachedProperties.SetSelectionAdorner(scp, null);
            }

            return ResultCommand.OK;

        }
    }
    public class UpdateAdorner : ScriptCommandBase
    {
        public UpdateAdorner(bool setCurPositionAsEndPosition)
            : base("UpdateAdorner",
            "IsSelecting", "StartPostion", "EndPosition")
        { _setCurPositionAsEndPosition = setCurPositionAsEndPosition; }

        private bool _setCurPositionAsEndPosition;

        private Point adjustScrollBarPosition(Control c, ScrollContentPresenter scp, Point pt)
        {
            var currentScrollbarPosition = ControlUtils.GetScrollbarPosition(scp);
            var startScrollbarPosition = AttachedProperties.GetStartScrollbarPosition(c);
            return new Point(pt.X + startScrollbarPosition.X - currentScrollbarPosition.X,
                pt.Y + startScrollbarPosition.Y - currentScrollbarPosition.Y);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            if (AttachedProperties.GetIsDragging(c))
            {
                var lastAdorner = pm.ContainsKey("SelectionAdorner") ? (SelectionAdorner)pm["SelectionAdorner"] :
                        AttachedProperties.GetSelectionAdorner(scp);

                var result = MultiSelectScriptCommands.ObtainPointerPosition.Execute(pm);
                if (result != ResultCommand.OK)
                    return result;

                if (pd.ContainsKey("IsSelecting") && pd["IsSelecting"] is bool)
                    lastAdorner.IsSelecting = (bool)pd["IsSelecting"];

                lastAdorner.StartPosition = (Point)pd["StartAdjustedPosition"];
                lastAdorner.EndPosition = (Point)pd["CurrentPosition"];
            }
            return ResultCommand.OK;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            return scp != null && AttachedProperties.GetSelectionAdorner(scp) != null;
        }
    }

    /// <summary>
    /// Obtain position of scrollbar and  mouse, and write it to ParameterDic.
    /// </summary>
    public class ObtainPointerPosition : ScriptCommandBase
    {
        public ObtainPointerPosition()
            : base("ObtainPointerPosition")
        { }

        private Point adjustScrollBarPosition(Point pt, Point startScrollbarPosition, Point currentScrollbarPosition)
        {
            return new Point(pt.X - currentScrollbarPosition.X + startScrollbarPosition.X,
                pt.Y - currentScrollbarPosition.Y + startScrollbarPosition.Y);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            if (!(pd.ContainsKey("StartPosition")))
                pd["StartPosition"] = AttachedProperties.GetStartPosition(c);

            if (!(pd.ContainsKey("StartScrollbarPosition")))
                pd["StartScrollbarPosition"] = AttachedProperties.GetStartScrollbarPosition(c);

            if (!(pd.ContainsKey("CurrentScrollbarPosition")))
                pd["CurrentScrollbarPosition"] = ControlUtils.GetScrollbarPosition(scp);

            if (!(pd.ContainsKey("CurrentPosition")))
                pd["CurrentPosition"] = Mouse.GetPosition(c);

            //These adjusted position for used in visual only.
            if (!(pd.ContainsKey("StartAdjustedPosition")))
                pd["StartAdjustedPosition"] = adjustScrollBarPosition((Point)pd["StartPosition"],
                    (Point)pd["StartScrollbarPosition"], (Point)pd["CurrentScrollbarPosition"]);

            return ResultCommand.OK;
        }
    }



    #endregion


    /// <summary>
    /// When drag started, AttachAdorner, SetStartPosition, Set StartSelectedItem 
    /// Then Mouse.Capture.
    /// </summary>
    public class StartDrag : ScriptCommandBase
    {
        public StartDrag() : base("StartDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            var eventArgs = pd.EventArgs as MouseEventArgs;

            var itemUnderMouse = UITools.GetSelectedListBoxItem(scp, eventArgs.GetPosition(scp));
            AttachedProperties.SetStartSelectedItem(c, itemUnderMouse);

            pm["IsSelecting"] = true;
            var result = MultiSelectScriptCommands.AttachAdorner.Execute(pm);
            if (result != ResultCommand.OK) return result;

            Mouse.Capture(scp);
            return ResultCommand.OK;
        }
    }

    public class EndDrag : ScriptCommandBase
    {
        public EndDrag() : base("EndDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {            
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            var eventArgs = pd.EventArgs as MouseEventArgs;

            AttachedProperties.SetStartSelectedItem(c, null);

            if (AttachedProperties.GetIsDragging(c))
            {
                new ScriptRunner().Run(new Queue<IScriptCommand>(
                    new[] {                    
                    MultiSelectScriptCommands.ObtainPointerPosition,
                    //Find selecting items and assigned to SelectedList and SelectedIdList
                    MultiSelectScriptCommands.FindSelectedItems, 
                      //Select items and remove all highlight.
                    MultiSelectScriptCommands.SelectItems,
                     //Remove adorner.
                    MultiSelectScriptCommands.DetachAdorner,
                }), pm);

                Mouse.Capture(null);
            }
            return ResultCommand.OK;
        }
    }

    public class UpdateSelection : ScriptCommandBase
    {
        public UpdateSelection(bool highlightOnly)
            : base("UpdateSelection", "EventArgs")
        {
            _hightlightOnly = highlightOnly;
        }

        private bool _hightlightOnly;

        public override bool CanExecute(ParameterDic pm)
        {
            return AttachedProperties.GetIsDragging(pm.AsUIParameterDic().Sender as Control);
        }



        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();

            new ScriptRunner().Run(new Queue<IScriptCommand>(
                new[] {                    
                    MultiSelectScriptCommands.ObtainPointerPosition,
                    //Find selecting items and assigned to SelectedList and SelectedIdList
                    MultiSelectScriptCommands.FindSelectedItems, 
                    MultiSelectScriptCommands.HighlightItems
                }), pm);

            pd.EventArgs.Handled = true;

            return MultiSelectScriptCommands.UpdateAdornerPosition;
        }
    }

    public enum SelectMode { highlight, select }
    public class SelectItems : ScriptCommandBase
    {
        public SelectItems(SelectMode selectMode) : base("FindSelectedItems", "EventArgs", "SelectedIdList") { _selectMode = selectMode; }

        SelectMode _selectMode;


        public override bool CanExecute(ParameterDic pm)
        {
            return true;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            var eventArgs = pd.EventArgs as MouseEventArgs;
            var selectedIdList = pm.ContainsKey("SelectedIdList") ? pm["SelectedIdList"] as List<int>
                : new List<int>();

            switch (_selectMode)
            {
                case SelectMode.highlight:
                    for (int i = 0; i < ic.Items.Count; i++)
                    {
                        DependencyObject item = ic.ItemContainerGenerator.ContainerFromIndex(i);
                        if (item != null)
                            AttachedProperties.SetIsSelecting(item, selectedIdList.Contains(i));
                    }
                    break;
                case SelectMode.select:
                    for (int i = 0; i < ic.Items.Count; i++)
                    {
                        DependencyObject item = ic.ItemContainerGenerator.ContainerFromIndex(i);
                        if (item != null)
                        {
                            AttachedProperties.SetIsSelecting(item, false);
                            item.SetValue(ListBoxItem.IsSelectedProperty, selectedIdList.Contains(i));
                        }
                    }
                    break;
            }



            return ResultCommand.OK;
        }
    }

    public class FindSelectedItems : ScriptCommandBase
    {
        public FindSelectedItems() : base("FindSelectedItems", "EventArgs", "SelectionBoundsAdjusted") { }

        public override bool CanExecute(ParameterDic pm)
        {
            return true;
        }

        private Point add(Point pt1, Point pt2)
        {
            return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {


            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            var eventArgs = pd.EventArgs as MouseEventArgs;
            if (AttachedProperties.GetIsDragging(ic))
            {
                //SelectionBounds that used to calcuate selected items must take scroll bar position into account.                       
                if (!pm.ContainsKey("SelectionBounds") || !(pm["SelectionBounds"] is Rect))
                    pd["SelectionBounds"] = new Rect(add((Point)pd["StartPosition"], (Point)pd["StartScrollbarPosition"]),
                        add((Point)pd["CurrentPosition"], (Point)pd["CurrentScrollbarPosition"]));

                List<object> selectedList; pm["SelectedList"] = selectedList = new List<object>();
                List<int> selectedIdList; pm["SelectedIdList"] = selectedIdList = new List<int>();

                IChildInfo icInfo = UITools.FindVisualChild<Panel>(scp) as IChildInfo;
                if (icInfo != null)
                {
                    Rect selectionBound = (Rect)pm["SelectionBounds"];
                    for (int i = 0; i < ic.Items.Count; i++)
                        if (icInfo.GetChildRect(i).IntersectsWith(selectionBound))
                        {
                            selectedList.Add(ic.Items[i]);
                            selectedIdList.Add(i);
                        }
                }
                else return ResultCommand.Error(new NotSupportedException());
            }
            return ResultCommand.OK;
        }
    }
}
