using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.UserControls;
using FileExplorer.Utils;
using FileExplorer.ViewModels;

namespace FileExplorer.BaseControls.MultiSelect
{
    /*           (PreviewMouseDown) --> ObtainPointerPosition --> SetHandledIfNotFocused --> NoError
     * 
     *           (MouseDrag)                (MouseMove)                         (MouseUp)
     *            BeginSelect               ContinueSelect                      EndSelect
     *               |                               |                           |    |
     *               |-False--->UpdateIsSelecting<---|---------True--------------| ObtainPointerPosition
     *                            |-(ToValue)        |                                |
     *                      OK-----    |       CheckIsSelecting------OK       ClearSelectionIfNoItemUnderCurrentPosition
     *             [If already value]  |             |          [If not selecting]
     *                                 V             V
     *                              ObtainPointerPosition
     *                                  |   
     *                                  V           
     *                       FindSelectedItems
     *                              |--> FindSelectedItemsUsingGridView or
     *                              |--> FindSelectedItemsUsingIChildInfo 
     *                              |--> FindSelectedItemsUsingHitTest
     *                   IsSelecting?      Y|                   N|
     *                                      V                    V
     *                            HighlightItems/ByUpdate     SelectItems/ByUpdate     
     *                                      V                        |
     *                                AttachAdorner                  |
     *                                      V                        V
     *                                UpdateAdorner            DetachAdorner
     *                                      |--------- |-------------|
     *                                                 V
     *                                             AutoScroll
     *                                                 V
     *                                                 OK
     * 
     */


    #region Entry Point - Begin/Continue/EndSelect


    public class SetHandledIfNotFocused : ScriptCommandBase
    {
        public SetHandledIfNotFocused()
            : base("SetHandledIfNotFocused", "EventArgs")
        {

        }
        public ICommand UnselectCommand { get; set; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);


            if (!ic.IsKeyboardFocusWithin)
            {
                var itemUnderMouse = UITools.GetItemUnderMouse(ic, pd.Input.PositionRelativeTo(scp));

                if ((itemUnderMouse is ListBoxItem && (itemUnderMouse as ListBoxItem).IsSelected) ||
                    (itemUnderMouse is TreeViewItem && (itemUnderMouse as TreeViewItem).IsSelected))
                {
                    ic.Focus();
                    pd.EventArgs.Handled = true;
                }
            }

            return ResultCommand.NoError;
        }
    }


    /// <summary>
    /// When select started, AttachAdorner, SetStartPosition, Set StartSelectedItem 
    /// Then Mouse.Capture.
    /// </summary>
    public class BeginSelect : ScriptCommandBase
    {
        public BeginSelect()
            : base("BeginSelect", "EventArgs")
        {
            UnselectCommand = null;
        }
        public ICommand UnselectCommand { get; set; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            var eventArgs = pd.EventArgs;


            if (scp == null)
                return ResultCommand.NoError;


            if (Keyboard.Modifiers != ModifierKeys.None)
                return ResultCommand.NoError;

            if (eventArgs.Handled)
                return ResultCommand.NoError;

            eventArgs.Handled = true;

            pm["UnselectCommand"] = UnselectCommand;

            Mouse.Capture(scp);
            return new UpdateIsSelecting(true);
        }
    }

    public class ContinueSelect : ScriptCommandBase
    {
        public ContinueSelect() : base("ContinueSelect", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;

            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;

            return new CheckIsSelecting();
        }
    }

    public class EndSelect : ScriptCommandBase
    {
        public EndSelect() : base("EndSelect", "EventArgs") { UnselectCommand = null; IsCheckBoxEnabled = false; }

        public ICommand UnselectCommand { get; set; }
        public bool IsCheckBoxEnabled { get; set; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            var eventArgs = pd.EventArgs as InputEventArgs;

            if (eventArgs == null)
                return ResultCommand.Error(new ArgumentException("EventArgs not InputEventArgs"));

            if (eventArgs.OriginalSource is CheckBox)
                return ResultCommand.NoError;

            pm["UnselectCommand"] = UnselectCommand;

            if (eventArgs.Handled)
                return ResultCommand.NoError;

            Mouse.Capture(null);

          
            if (AttachedProperties.GetIsSelecting(ic))
                return new UpdateIsSelecting(false);
            else
            {
                if (!pd.IsHandled && pd.Input.InputType != UIInputType.MouseRight)
                    return new ObtainPointerPosition(new SimpleScriptCommand("ClearSelectionIfNoItemUnderCurrentPosition",
                        pd2 =>
                        {

                            //(If not mouse over item and is selecting (dragging), this will unselect all)
                            object itemUnderMouse = UITools.GetItemUnderMouse(ic, (Point)pd2["CurrentPosition"]);
                            if (itemUnderMouse == null)
                            {
                                pd2["SelectedList"] = new List<object>();
                                return new SelectItems(ItemSelectProcessor.SelectItemInSelectedList);
                            }
                            else return ResultCommand.NoError;
                        }));

            }
            return ResultCommand.NoError;
        }
    }

    #endregion

    #region Update/CheckIsSelecting

    public class UpdateIsSelecting : ScriptCommandBase
    {
        private bool _toValue;
        public UpdateIsSelecting(bool toValue) : base("UpdateIsSelecting") { _toValue = toValue; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            if (AttachedProperties.GetIsSelecting(c) != _toValue)
            {
                AttachedProperties.SetIsSelecting(c, _toValue);
                return new ObtainPointerPosition(new FindSelectedItems());
            }
            return ResultCommand.NoError;
        }
    }

    public class CheckIsSelecting : ScriptCommandBase
    {
        public CheckIsSelecting() : base("CheckIsSelecting") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            if (AttachedProperties.GetIsSelecting(c))
                return new ObtainPointerPosition(new FindSelectedItems());
            else return ResultCommand.NoError;
        }
    }



    #endregion




    #region ObtainPointerPosition and FindSelectedItems



    /// <summary>
    /// Obtain position of scrollbar and  mouse, and write it to ParameterDic.
    /// </summary>
    public class ObtainPointerPosition : ScriptCommandBase
    {
        public ObtainPointerPosition(IScriptCommand nextCommand)
            : base("ObtainPointerPosition", nextCommand)
        { }

        private Point add(params Point[] pts)
        {
            Point retVal = new Point(0, 0);
            foreach (var pt in pts)
                retVal = new Point(retVal.X + pt.X, retVal.Y + pt.Y);
            return retVal;
        }

        private Point adjustScrollBarPosition(Point pt, Point startScrollbarPosition, Point currentScrollbarPosition)
        {
            return new Point(pt.X - currentScrollbarPosition.X + startScrollbarPosition.X,
                pt.Y - currentScrollbarPosition.Y + startScrollbarPosition.Y);
        }

        public static Point AdjustHeaderPosition(Point point, ParameterDic pd, int offset = -1)
        {
            Point pt = new Point(point.X, point.Y);
            pt.Offset(0, offset * ((Size)pd["ContentBelowHeaderSize"]).Height);
            //Deduct Grid View Header from the position.
            pt.Offset(0, offset * ((Size)pd["GridViewHeaderSize"]).Height);
            return pt;
        }


        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            var contentBelowHeader = (c is ListViewEx) ? (c as ListViewEx).ContentBelowHeader as FrameworkElement : null;
            pd["ContentBelowHeaderSize"] =
               contentBelowHeader == null ? new Size(0, 0) :
                   new Size(contentBelowHeader.ActualWidth, contentBelowHeader.ActualHeight);

            var gvhrp = UITools.FindVisualChild<GridViewHeaderRowPresenter>(c);
            pd["GridViewHeaderSize"] =
                             gvhrp == null ? new Size(0, 0) :
                    new Size(gvhrp.ActualWidth, gvhrp.ActualHeight);



            if (!(pd.ContainsKey("StartPosition")))
                pd["StartPosition"] = AdjustHeaderPosition(pd.GetDragInputProcessor().StartInput.Position, pd);

            if (!(pd.ContainsKey("StartScrollbarPosition")))
                pd["StartScrollbarPosition"] = pd.GetDragInputProcessor().StartInput.ScrollBarPosition;

            if (!(pd.ContainsKey("CurrentScrollbarPosition")))
                pd["CurrentScrollbarPosition"] = pd.Input.ScrollBarPosition;


            if (!(pd.ContainsKey("CurrentPosition")))
                pd["CurrentPosition"] = AdjustHeaderPosition(pd.Input.Position, pd);


            if (!(pd.ContainsKey("CurrentRelativePosition"))) //To scp ScrollContentPresenter
                pd["CurrentRelativePosition"] = pd.Input.PositionRelativeTo(scp);

            //These adjusted position for used in visual only.
            if (!(pd.ContainsKey("StartAdjustedPosition")))
                pd["StartAdjustedPosition"] = adjustScrollBarPosition((Point)pd["StartPosition"],
                    (Point)pd["StartScrollbarPosition"], (Point)pd["CurrentScrollbarPosition"]);

            //SelectionBounds that used to calcuate selected items must take scroll bar position into account.                       
            if (!pm.ContainsKey("SelectionBoundsAdjusted") || !(pm["SelectionBoundsAdjusted"] is Rect))
                pd["SelectionBoundsAdjusted"] = new Rect(
                    add((Point)pd["StartPosition"], (Point)pd["StartScrollbarPosition"]),
                    add((Point)pd["CurrentPosition"], (Point)pd["CurrentScrollbarPosition"]));

            if (!pm.ContainsKey("SelectionBounds") || !(pm["SelectionBounds"] is Rect))
                pd["SelectionBounds"] = new Rect((Point)pd["StartPosition"], (Point)pd["CurrentPosition"]);



            return _nextCommand;
        }
    }


    public class FindSelectedItemsUsingIChildInfo : ScriptCommandBase
    {
        public FindSelectedItemsUsingIChildInfo(ItemsControl ic, IChildInfo icInfo) :
            base("FindSelectedItemsUsingIChildInfo", "EventArgs", "SelectionBounds", "SelectionBoundsAdjusted")
        { _icInfo = icInfo; _ic = ic; }

        private IChildInfo _icInfo;
        private ItemsControl _ic;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            List<object> selectedList; pm["SelectedList"] = selectedList = new List<object>();
            List<int> selectedIdList; pm["SelectedIdList"] = selectedIdList = new List<int>();

            Rect selectionBound = (Rect)pm["SelectionBoundsAdjusted"];
            for (int i = 0; i < _ic.Items.Count; i++)
                if (_icInfo.GetChildRect(i).IntersectsWith(selectionBound))
                {
                    selectedList.Add(_ic.Items[i]);
                    selectedIdList.Add(i);
                }

            if (AttachedProperties.GetIsSelecting(_ic))
                return new HighlightItems();
            else return new SelectItems(ItemSelectProcessor.SelectItemInSelectedList);
        }

    }


    public class FindSelectedItemsUsingGridView : ScriptCommandBase
    {
        public FindSelectedItemsUsingGridView(ItemsControl ic, GridView gview,
            ScrollContentPresenter scp) :
            base("FindSelectedItemsUsingGridView", "EventArgs", "SelectionBounds", "SelectionBoundsAdjusted")
        { _ic = ic; _gview = gview; _scp = scp; }

        private GridView _gview;
        private ItemsControl _ic;
        private ScrollContentPresenter _scp;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            List<object> selectedList; pm["SelectedList"] = selectedList = new List<object>();
            List<int> selectedIdList; pm["SelectedIdList"] = selectedIdList = new List<int>();

            Point posRelToScp = (Point)pm["CurrentRelativePosition"];
            var startSelected = AttachedProperties.GetStartSelectedItem(_ic);

            var currentSelected = UITools.GetSelectedListBoxItem(_scp, posRelToScp);
            if (startSelected != null && currentSelected != null)
            {
                int startIdx = _ic.ItemContainerGenerator.IndexFromContainer(startSelected);
                int endIdx = _ic.ItemContainerGenerator.IndexFromContainer(currentSelected);

                for (int i = Math.Min(startIdx, endIdx); i <= Math.Max(startIdx, endIdx); i++)
                {
                    selectedList.Add(_ic.Items[i]);
                    selectedIdList.Add(i);
                }
            }

            //UpdateStartSelectedItems, or clear it if no longer selecting.
            if (AttachedProperties.GetIsSelecting(_ic))
            {
                if (AttachedProperties.GetStartSelectedItem(_ic) == null)
                {
                    var itemUnderMouse = UITools.GetSelectedListBoxItem(_scp, posRelToScp);
                    AttachedProperties.SetStartSelectedItem(_ic, itemUnderMouse);
                }
            }
            else
                AttachedProperties.SetStartSelectedItem(_ic, null);

            IScriptCommand nextCommand =
                (AttachedProperties.GetIsSelecting(_ic)) ? (IScriptCommand)new HighlightItems() :
                new SelectItems(ItemSelectProcessor.SelectItemInSelectedList);


            if (AttachedProperties.GetIsSelecting(_ic))
            {
                if (AttachedProperties.GetStartSelectedItem(_ic) == null)
                    UITools.SetItemUnderMouseToAttachedProperty(_ic, posRelToScp,
                        AttachedProperties.StartSelectedItemProperty);
            }

            return nextCommand;
        }

    }

    public class FindSelectedItemsUsingHitTest : ScriptCommandBase
    {
        public FindSelectedItemsUsingHitTest(ItemsControl ic) :
            base("FindSelectedItemsUsingHitTest", "EventArgs", "SelectionBounds", "SelectionBoundsAdjusted")
        { _ic = ic; }

        private ItemsControl _ic;
        private static Rect _prevBound = new Rect(0, 0, 0, 0);


        private static HitTestResultBehavior selectResultCallback(HitTestResult result)
        {
            return HitTestResultBehavior.Continue;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            List<object> selectedList; pm["SelectedList"] = selectedList = new List<object>();
            List<int> selectedIdList; pm["SelectedIdList"] = selectedIdList = new List<int>();
            List<object> unselectedList; pm["UnselectedList"] = unselectedList = new List<object>();
            List<int> unselectedIdList; pm["UnselectedIdList"] = unselectedIdList = new List<int>();

            Func<HitTestResult, HitTestResultBehavior> cont = (result) => HitTestResultBehavior.Continue;
            HitTestFilterCallback selectFilter = (HitTestFilterCallback)((potentialHitTestTarget) =>
            {
                if (potentialHitTestTarget is ListViewItem || potentialHitTestTarget is TreeViewItem)
                {
                    selectedList.Add(potentialHitTestTarget);
                    int id = _ic.ItemContainerGenerator.IndexFromContainer(potentialHitTestTarget);
                    selectedIdList.Add(id);

                    if (unselectedList.Contains(potentialHitTestTarget)) unselectedList.Remove(potentialHitTestTarget);
                    if (unselectedIdList.Contains(id)) unselectedIdList.Remove(id);
                    return HitTestFilterBehavior.ContinueSkipChildren;
                }
                return HitTestFilterBehavior.Continue;
            });
            HitTestFilterCallback unselectFilter = (HitTestFilterCallback)((potentialHitTestTarget) =>
            {
                if (potentialHitTestTarget is ListViewItem || potentialHitTestTarget is TreeViewItem)
                {
                    unselectedList.Add(potentialHitTestTarget);
                    unselectedIdList.Add(_ic.ItemContainerGenerator.IndexFromContainer(potentialHitTestTarget));
                    return HitTestFilterBehavior.ContinueSkipChildren;
                }
                return HitTestFilterBehavior.Continue;
            });

            Rect selectionBound = (Rect)pm["SelectionBounds"];

            //Unselect all visible selected items (by using _lastPos) no matter it's current selected or not.
            VisualTreeHelper.HitTest(_ic, unselectFilter,
                new HitTestResultCallback(cont),
                new GeometryHitTestParameters(new RectangleGeometry(_prevBound)));

            //Select all visible items in select region.
            VisualTreeHelper.HitTest(_ic, selectFilter,
                new HitTestResultCallback(cont),
                new GeometryHitTestParameters(new RectangleGeometry(selectionBound)));

            _prevBound = selectionBound;

            if (AttachedProperties.GetIsSelecting(_ic))
                return new HighlightItemsByUpdate();
            else return new SelectItemsByUpdate();
        }

    }

    /// <summary>
    /// Call the appropriated FindSelectedItemsXYZ to poll a list of selected list (or selected + unselectedList)
    /// </summary>
    public class FindSelectedItems : ScriptCommandBase
    {
        public FindSelectedItems() : base("FindSelectedItems", "EventArgs", "SelectionBounds", "SelectionBoundsAdjusted") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);

            IChildInfo icInfo = UITools.FindVisualChild<Panel>(scp) as IChildInfo;
            if (icInfo != null)
                return new FindSelectedItemsUsingIChildInfo(ic, icInfo);
            else
                if (ic is ListView && (ic as ListView).View is GridView)
                {
                    var gview = (ic as ListView).View as GridView;
                    return new FindSelectedItemsUsingGridView(ic, gview, scp);
                }
                else return new FindSelectedItemsUsingHitTest(ic);
        }
    }




    #endregion



    #region Item selection update func - Highlight/SelectItems

    /// <summary>
    /// Update highlight of items (AttachedProperties.IsSelecting) using SelectedIdList.
    /// </summary>
    public class HighlightItems : ScriptCommandBase
    {
        public HighlightItems() : base("FindSelectedItems", "EventArgs", "SelectedIdList") { }


        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            var selectedIdList = pm.ContainsKey("SelectedIdList") ? pm["SelectedIdList"] as List<int>
                : new List<int>();

            for (int i = 0; i < ic.Items.Count; i++)
            {
                DependencyObject item = ic.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null)
                    AttachedProperties.SetIsSelecting(item, selectedIdList.Contains(i));
            }

            return new AttachAdorner();
        }
    }

    public class HighlightItemsByUpdate : ScriptCommandBase
    {
        public HighlightItemsByUpdate() : base("HighlightItemsByUpdate", "EventArgs", "UnselectedIdList", "UnselectedList") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            List<object> unselectedList = pm["UnselectedList"] as List<object>;
            List<int> unselectedIdList = pm["UnselectedIdList"] as List<int>;

            List<object> selectedList = pm["SelectedList"] as List<object>;
            List<int> selectedIdList = pm["SelectedIdList"] as List<int>;

            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            for (int i = 0; i < ic.Items.Count; i++)
            {
                DependencyObject item = ic.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null)
                {
                    bool isSelecting = AttachedProperties.GetIsSelecting(item);
                    if (isSelecting && unselectedList.Contains(item))
                        AttachedProperties.SetIsSelecting(item, false);
                    else if (!isSelecting && selectedList.Contains(item))
                        AttachedProperties.SetIsSelecting(item, true);
                }
            }

            return new AttachAdorner();
        }
    }

    public class SelectItemsByUpdate : ScriptCommandBase
    {
        public SelectItemsByUpdate() : base("SelectItemByUpdate", "EventArgs", "UnselectedIdList", "UnselectedList") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            List<object> unselectedList = pm["UnselectedList"] as List<object>;
            List<int> unselectedIdList = pm["UnselectedIdList"] as List<int>;

            List<object> selectedList = pm["SelectedList"] as List<object>;
            List<int> selectedIdList = pm["SelectedIdList"] as List<int>;

            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            for (int i = 0; i < ic.Items.Count; i++)
            {
                DependencyObject item = ic.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null)
                {
                    bool isSelecting = AttachedProperties.GetIsSelecting(item);
                    AttachedProperties.SetIsSelecting(item, false);

                    if (isSelecting && !unselectedList.Contains(item))
                        item.SetValue(ListBoxItem.IsSelectedProperty, true);
                    else if (selectedList.Contains(item))
                        item.SetValue(ListBoxItem.IsSelectedProperty, true);
                    else item.SetValue(ListBoxItem.IsSelectedProperty, false);
                }
            }

            return new DetachAdorner();
        }
    }

    public interface IItemSelectProcessor
    {
        void Select(ISelectable item, bool inSelectedList);
    }
    public class ItemSelectProcessor : IItemSelectProcessor
    {
        public static IItemSelectProcessor SelectItemInSelectedList =
            new ItemSelectProcessor((vm, inList) => { vm.IsSelected = inList; });
        public static IItemSelectProcessor AppendItemInSelectedList =
            new ItemSelectProcessor((vm, inList) => { vm.IsSelected = vm.IsSelected || inList; });
        public static IItemSelectProcessor ToggleItemInSelectedList =
            new ItemSelectProcessor((vm, inList) =>
            {
                if (inList)
                    vm.IsSelected = !vm.IsSelected;
            });

        private Action<ISelectable, bool> _selectFunc;
        protected ItemSelectProcessor(Action<ISelectable, bool> selectFunc)
        {
            _selectFunc = selectFunc;
        }
        public void Select(ISelectable item, bool inSelectedList)
        {
            _selectFunc(item, inSelectedList);
        }
    }


    /// <summary>
    /// Update actual selection of items (ListBoxItem.IsSelected) using SelectedIdList.
    /// </summary>
    public class SelectItems : ScriptCommandBase
    {
        private IItemSelectProcessor _processor;
        public SelectItems(IItemSelectProcessor processor)
            : base("SelectItems", "EventArgs", "SelectedList", "SelectedIdList")
        { _processor = processor; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            var selectedIdList = pm.ContainsKey("SelectedIdList") ? pm["SelectedIdList"] as List<int> : null;
            var selectedList = pm.ContainsKey("SelectedList") ? pm["SelectedList"] as List<object> : null;

            if (pm.ContainsKey("UnselectCommand"))
            {
                ICommand unselectCommand = pm["UnselectCommand"] as ICommand;
                if (unselectCommand != null && unselectCommand.CanExecute(ic))
                    unselectCommand.Execute(ic);
            }


            bool isISelectable = ic.Items.Count > 0 && ic.Items[0] is ISelectable;


            Action<int, object, ListBoxItem> updateSelected = null;
            Func<int, ListBoxItem, bool> returnSelected = (idx, item) => false;
            if (selectedIdList != null)
                returnSelected = (idx, item) => selectedIdList.Contains(idx);
            else if (selectedList != null)
                returnSelected = (idx, item) => selectedList.Contains(item);


            updateSelected = (idx, vm, item) => _processor.Select(
                ExtensionMethods.ToISelectable(vm, item), returnSelected(idx, item));

            for (int i = 0; i < ic.Items.Count; i++)
            {
                ListBoxItem item = ic.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

                if (item != null)
                    AttachedProperties.SetIsSelecting(item, false);
                updateSelected(i, ic.Items[i], item);

            }


            return new DetachAdorner();
        }
    }

    #endregion


    #region Adorner related funcs - Attach/Update/DetachAdorner

    /// <summary>
    /// Attach adorner to adorner layer, then call UpdateAdorner.
    /// </summary>
    public class AttachAdorner : ScriptCommandBase
    {
        public AttachAdorner() : base("AttachAdorner") { }

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
                if (AttachedProperties.GetSelectionAdorner(scp) == null)
                {

                    //Create and register adorner.
                    SelectionAdorner adorner = new SelectionAdorner(scp);
                    pm["SelectionAdorner"] = adorner;
                    AttachedProperties.SetSelectionAdorner(scp, adorner);
                    AttachedProperties.SetLastScrollContentPresenter(c, scp); //For used when detach.

                    adornerLayer.Add(adorner);
                }
                return new UpdateAdorner();
            }
            return ResultCommand.Error(new KeyNotFoundException("Scroll Content Presenter not found"));
        }
    }

    /// <summary>
    /// Update IsSelecting and position adorner if it's exists.
    /// </summary>
    public class UpdateAdorner : ScriptCommandBase
    {
        public UpdateAdorner()
            : base("UpdateAdorner",
            "IsSelecting", "StartPostion", "EndPosition")
        { }

        //private Point adjustScrollBarPosition(Control c, ScrollContentPresenter scp, Point pt)
        //{
        //    var currentScrollbarPosition = ControlUtils.GetScrollbarPosition(scp);
        //    var startScrollbarPosition = AttachedProperties.GetStartScrollbarPosition(c);
        //    return new Point(pt.X + startScrollbarPosition.X - currentScrollbarPosition.X,
        //        pt.Y + startScrollbarPosition.Y - currentScrollbarPosition.Y);
        //}

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            var lastAdorner = pm.ContainsKey("SelectionAdorner") ? (SelectionAdorner)pm["SelectionAdorner"] :
                    AttachedProperties.GetSelectionAdorner(scp);

            if (lastAdorner == null)
                return ResultCommand.Error(new Exception("Adorner not found."));

            lastAdorner.IsSelecting = AttachedProperties.GetIsSelecting(c);

            lastAdorner.StartPosition = (Point)pd["StartAdjustedPosition"];
            lastAdorner.EndPosition = (Point)pd["CurrentPosition"];

            return new AutoScroll();
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
    /// Remove adorner from adorner layer.
    /// </summary>
    public class DetachAdorner : ScriptCommandBase
    {
        public DetachAdorner() : base("DetachAdorner") { }


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

            return ResultCommand.NoError;

        }
    }

    #endregion

    public class AutoScroll : ScriptCommandBase
    {
        public AutoScroll()
            : base("AutoScroll")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            if (pd.Input != null)
            {
                Point posRelToScp = (Point)pm["CurrentRelativePosition"];
                IScrollInfo isInfo = UITools.FindVisualChild<Panel>(scp) as IScrollInfo;
                if (isInfo != null)
                {
                    if (isInfo.CanHorizontallyScroll)
                        if (posRelToScp.X < 0)
                            isInfo.LineLeft();
                        else if (posRelToScp.X > (isInfo as Panel).ActualWidth)
                            isInfo.LineRight();
                    if (isInfo.CanVerticallyScroll)
                        if (posRelToScp.Y < 0)
                            isInfo.LineUp();
                        else if (posRelToScp.Y > (isInfo as Panel).ActualHeight) //isInfo.ViewportHeight is bugged.
                            isInfo.LineDown();
                }
            }
            return ResultCommand.NoError;
        }
    }
}

