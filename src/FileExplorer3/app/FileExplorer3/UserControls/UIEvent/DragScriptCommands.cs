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
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;
using FileExplorer.ViewModels;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.BaseControls.DragnDrop
{
    public static class DataContextFinder
    {
        public static Func<object, ISupportDrag> SupportDrag =
            dc => (dc is ISupportDrag && (dc as ISupportDrag).HasDraggables) ? dc as ISupportDrag :
                (dc is ISupportDragHelper && (dc as ISupportDragHelper).DragHelper.HasDraggables) ? (dc as ISupportDragHelper).DragHelper :
                null;

        public static Func<object, ISupportDrop> SupportDrop =
            dc => (dc is ISupportDrop && (dc as ISupportDrop).IsDroppable) ? dc as ISupportDrop :
                (dc is ISupportDropHelper && (dc as ISupportDropHelper).DropHelper.IsDroppable) ? (dc as ISupportDropHelper).DropHelper :
                null;

        public static T GetDataContext<T>(ParameterDic pm, Func<object, T> filter = null)
        {
            FrameworkElement ele;
            return GetDataContext(pm, out ele, filter);
        }


        public static FrameworkElement GetDataContextOwner(FrameworkElement ele)
        {
            var tvItem = UITools.FindAncestor<TreeViewItem>(ele);
            if (tvItem != null)
                return tvItem;

            var lvItem = UITools.FindAncestor<ListViewItem>(ele);
            if (lvItem != null)
                return lvItem;

            var ic = UITools.FindAncestor<ItemsControl>(ele);
            if (ic != null)
                return ic;

            return ele;
        }

        public static T GetDataContext<T>(FrameworkElement ele, Func<object, T> filter = null)
        {
            FrameworkElement ele1 = ele;
            return GetDataContext<T>(ref ele1, filter);
        }

        public static T GetDataContext<T>(ref FrameworkElement ele, Func<object, T> filter = null)
        {
            object dataContext = ele.DataContext;
            var filterResult = filter(dataContext);
            if (filterResult != null)
            {
                ele = GetDataContextOwner(ele);
                return filterResult;
            }
            else
            {
                var ic = UITools.FindAncestor<ItemsControl>(ele);
                while (ic != null)
                {
                    filterResult = filter(ic.DataContext);
                    if (filterResult != null)
                    {
                        ele = GetDataContextOwner(ic);
                        return filterResult;
                    }
                    ic = UITools.FindAncestor<ItemsControl>(VisualTreeHelper.GetParent(ic));
                }

                return default(T);
            }

        }

        public static T GetDataContext<T>(ParameterDic pm, out FrameworkElement ele, Func<object, T> filter = null)
        {
            var pd = pm.AsUIParameterDic();
            var eventArgs = pd.EventArgs as RoutedEventArgs;
            var origSource = eventArgs.OriginalSource as FrameworkElement;
            ele = null;

            object dataContext = origSource.DataContext;
            var filterResult = filter(dataContext);
            if (filterResult != null)
            {
                ele = GetDataContextOwner(origSource);
                return filterResult;
            }
            else
            {
                var ic = UITools.FindAncestor<ItemsControl>(origSource);
                while (ic != null)
                {
                    filterResult = filter(ic.DataContext);
                    if (filterResult != null)
                    {
                        ele = GetDataContextOwner(ic);
                        return filterResult;
                    }
                    ic = UITools.FindAncestor<ItemsControl>(VisualTreeHelper.GetParent(ic));
                }

            }
            return default(T);
        }

    }
    /// <summary>
    /// Set StartSelectedItem when PreviewMouseDown.
    /// </summary>
    public class RecordStartSelectedItem : ScriptCommandBase
    {
        public RecordStartSelectedItem() : base("RecordStartSelectedItem", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);
            var eventArgs = pd.Input.EventArgs as InputEventArgs;
            var scp = ControlUtils.GetScrollContentPresenter(ic);

            if (UITools.FindAncestor<ToggleButton>(eventArgs.OriginalSource as DependencyObject) != null)
                return ResultCommand.NoError;

            if (isd != null)
            {
                UITools.SetItemUnderMouseToAttachedProperty(ic, pd.Input.PositionRelativeTo(scp),
                    AttachedProperties.StartDraggingItemProperty);

                return new IfItemUnderMouseSelected(new SetSelectedDraggables(), ResultCommand.NoError);
            }
            return ResultCommand.NoError;
        }
    }

    public class QueryDrag : ScriptCommandBase
    {
        private IScriptCommand _otherwiseCmd;
        private Func<ItemsControl, ISupportDrag, IScriptCommand> _succeedCmd;
        public QueryDrag(Func<ItemsControl, ISupportDrag, IScriptCommand> succeedCmd,
            IScriptCommand otherwiseCmd)
            : base("QueryDrag")
        {
            _succeedCmd = succeedCmd;
            _otherwiseCmd = otherwiseCmd;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);


            if (ic != null && isd != null)
                if (ic.GetValue(AttachedProperties.StartDraggingItemProperty) != null)
                {
                    var previousDraggables = AttachedProperties.GetSelectedDraggables(ic);
                    var currentDraggables = isd.GetDraggables().ToList();

                    if (currentDraggables.Any() && previousDraggables != null &&
                        currentDraggables.SequenceEqual(previousDraggables))
                    {
                        return _succeedCmd(ic, isd).Execute(pm);
                    }
                }

            return _otherwiseCmd;
        }
    }

    public class BeginDrag : ScriptCommandBase
    {
        public BeginDrag() : base("BeginDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;

            return new QueryDrag((ic, isd) =>
            {
                //Set it handled so it wont call multi-select.
                pd.EventArgs.Handled = true;
                pd.IsHandled = true;
                AttachedProperties.SetIsDragging(ic, true);
                return new DoDragDrop(ic, isd);
            }, ResultCommand.NoError);
        }
    }

    public class ContinueDrag : ScriptCommandBase
    {
        public ContinueDrag() : base("ContinueDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;


            if (AttachedProperties.GetIsDragging(ic))
                pd.EventArgs.Handled = true;

            return ResultCommand.OK;
        }
    }

    public class EndDrag : ScriptCommandBase
    {
        public EndDrag() : base("EndDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            //if (AttachedProperties.GetStartDraggingItem(ic) != null)
            //    pd.IsHandled = true;

            AttachedProperties.SetStartDraggingItem(ic, null);
            AttachedProperties.SetIsDragging(ic, false);

            switch (AttachedProperties.GetDragMethod(ic))
            {
                case AttachedProperties.DragMethod.Menu:
                    return ResultCommand.NoError; //Don't detach adorner.
                default:
                    return new DetachAdorner();
            }
        }
    }






    #region Update/CheckIsDragging

    //public class UpdateIsDragging : ScriptCommandBase
    //{
    //    private bool _toValue;
    //    public UpdateIsDragging(bool toValue, IScriptCommand nextCommand = null) : base("UpdateIsDragging", nextCommand) { _toValue = toValue; }

    //    public override IScriptCommand Execute(ParameterDic pm)
    //    {
    //        var pd = pm.AsUIParameterDic();
    //        var c = pd.Sender as Control;
    //        if (AttachedProperties.GetIsDragging(c) != _toValue)
    //        {
    //            AttachedProperties.SetIsDragging(c, _toValue);
    //            if (_nextCommand != null)
    //                return _nextCommand;
    //        }
    //        return ResultCommand.NoError;
    //    }
    //}

    public class IfIsDragging : IfScriptCommand
    {
        public IfIsDragging(IScriptCommand ifTrueCommand, IScriptCommand otherwiseCommand)
            : base((pm) =>
                {
                    var pd = pm.AsUIParameterDic();
                    var c = pd.Sender as Control;
                    return AttachedProperties.GetIsDragging(c);
                }, ifTrueCommand, otherwiseCommand) { }

    }
    #endregion

    public class IfItemUnderMouseSelected : IfScriptCommand
    {
        private static bool conditionFunc(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);

            if (isd != null)
            {
                var startSelectedItem = AttachedProperties.GetStartDraggingItem(ic);
                if (startSelectedItem != null)
                {
                    if (isd.GetDraggables().Contains(
                        startSelectedItem.DataContext))
                        return true;

                    if (startSelectedItem.GetValue(ListBoxItem.IsSelectedProperty).Equals(true))
                        return true;

                    if (startSelectedItem.GetValue(TreeViewItem.IsSelectedProperty).Equals(true))
                        return true;
                }
            }
            return false;
        }

        public IfItemUnderMouseSelected(IScriptCommand ifTrueCommand, IScriptCommand otherwiseCommand) :
            base(conditionFunc, ifTrueCommand, otherwiseCommand)
        { }
    }

    public class SetSelectedDraggables : ScriptCommandBase
    {
        #region Constructor

        public SetSelectedDraggables() :
            base("SetPreviousSelectedDraggables", "EventArgs")
        { }

        #endregion

        #region Methods

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrag);
            if (isd != null)
            {

                AttachedProperties.SetSelectedDraggables(ic,
                    isd.GetDraggables().ToList());
                pd.EventArgs.Handled = true;
            }

            return ResultCommand.NoError;
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion


    }

    public class DoDragDrop : ScriptCommandBase
    {
        private IDataObject _dataObj;
        private ItemsControl _ic;
        private ISupportDrag _isd;
        private AttachedProperties.DragMethod _dragMethod;

        public DoDragDrop(ItemsControl ic, ISupportDrag isd) : base("DoDragDrop") { _ic = ic; _isd = isd; }

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            //The control, like treeview or listview
            FrameworkElement control = sender as FrameworkElement;

            //ESC pressed
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                control.AllowDrop = true;
            }
            else
                if (e.KeyStates == DragDropKeyStates.None)
                {
                    _dataObj.SetData(ShellClipboardFormats.CFSTR_INDRAGLOOP, 0);
                    e.Action = DragAction.Drop;
                }
                else
                    e.Action = DragAction.Continue;

            _dataObj.SetData(typeof(DragDropKeyStates), e.KeyStates);

            e.Handled = true;
        }

        private void OnPreviewDrop(object sender, QueryContinueDragEventArgs e)
        {

        }

        private void setDragMethod()
        {
            _dragMethod = AttachedProperties.DragMethod.Normal;
            if (Mouse.RightButton == MouseButtonState.Pressed)
                _dragMethod = AttachedProperties.DragMethod.Menu;

            //This is for EndDrag to now show adorner.
            AttachedProperties.SetDragMethod(_ic, _dragMethod);

            _dataObj.SetData(typeof(AttachedProperties.DragMethod), _dragMethod);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            //Debug.WriteLine(String.Format("DoDragDrop"));
            var pd = pm.AsUIParameterDic();

            var draggables = _isd.GetDraggables().ToList();
            _dataObj = _isd.GetDataObject(draggables);

            if (_dataObj == null)
                return ResultCommand.NoError; //Nothing to drag.

            var effect = _isd.QueryDrag(draggables);

            System.Windows.DragDrop.AddQueryContinueDragHandler(_ic, new QueryContinueDragEventHandler(OnQueryContinueDrag));

            _dataObj.SetData(typeof(ISupportDrag), _isd);
            //Determine and set the desired drag method. (Normal, Menu)
            setDragMethod();

            //Start the DragDrop.
            DragDropEffects resultEffect = System.Windows.DragDrop.DoDragDrop(_ic, _dataObj, effect);

            System.Windows.DragDrop.RemoveQueryContinueDragHandler(_ic, new QueryContinueDragEventHandler(OnQueryContinueDrag));
            Debug.WriteLine(String.Format("NotifyDropCompleted {0}", resultEffect));
            var dataObj = _dataObj;
            _dataObj = null;
            return new NotifyDropCompleted(_isd, draggables, dataObj, resultEffect);
        }
    }

    public class NotifyDropCompleted : ScriptCommandBase
    {
        private IEnumerable<IDraggable> _draggables;
        private IDataObject _dataObj;
        private DragDropEffects _resultEffect;
        private ISupportDrag _isd;

        public NotifyDropCompleted(ISupportDrag isd, IEnumerable<IDraggable> draggables, IDataObject dataObj,
            DragDropEffects resultEffect)
            : base("NotifyDropCompleted")
        {
            _isd = isd;
            _draggables = new List<IDraggable>(draggables);
            _dataObj = dataObj; _resultEffect = resultEffect;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (_resultEffect != DragDropEffects.None)
                _isd.OnDragCompleted(_draggables, _dataObj, _resultEffect);

            return ResultCommand.NoError;
        }
    }
}
