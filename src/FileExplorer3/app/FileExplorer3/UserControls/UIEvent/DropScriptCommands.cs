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
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.BaseControls.DragnDrop
{
    

    public enum QueryDragDropEffectMode { Enter, Leave }
    public class QueryDragDropEffects : ScriptCommandBase
    {
        private QueryDragDropEffectMode _mode;
        public QueryDragDropEffects(QueryDragDropEffectMode mode) :
            base("QueryDragDropEffects", "EventArgs") { _mode = mode; }

        public override IScriptCommand Execute(ParameterDic pm)
        {

            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;
            FrameworkElement ele;
            var isd = DataContextFinder.GetDataContext(pm, out ele, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                if (_mode == QueryDragDropEffectMode.Enter)
                {
                    AttachedProperties.SetDraggingOverItem(ic, ele);
                    eventArgs.Effects = (eventArgs.AllowedEffects & isd.QueryDrop(eventArgs.Data));
                    eventArgs.Handled = true;

                    return new AttachAdorner();
                }
                else
                {
                    AttachedProperties.SetDraggingOverItem(Window.GetWindow(ic), null);
                    return new HideAdorner();
                }
            }
            return ResultCommand.NoError;
        }
    }


    public class ContinueDrop : ScriptCommandBase
    {        
        public ContinueDrop()
            : base("ContinueDrop", "EventArgs")
        {  }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var eventArgs = pd.EventArgs as DragEventArgs;
            var parentWindow = Window.GetWindow(ic);

            var dragAdorner = AttachedProperties.GetDragAdorner(parentWindow);

            if (eventArgs.Data.GetDataPresent(typeof(AttachedProperties.DragState)))
            {
                var dragState = (AttachedProperties.DragState)eventArgs.Data.GetData(typeof(AttachedProperties.DragState));                
                switch (dragState)
                {
                    case AttachedProperties.DragState.Drag:
                        //Tell Drag side it support DropState.
                        eventArgs.Data.SetData(typeof(AttachedProperties.DropState), AttachedProperties.DropState.None);
                        break;

                    case AttachedProperties.DragState.Menu :
                        if (dragAdorner.DragDropEffect == DragDropEffects.None)
                        {
                            eventArgs.Data.SetData(typeof(AttachedProperties.DropState), AttachedProperties.DropState.Menu);
                            dragAdorner.ContextMenu.StaysOpen = true;
                            dragAdorner.ContextMenu.IsOpen = true;
                        }
                        else
                        {
                            eventArgs.Data.SetData(typeof(AttachedProperties.DropState), AttachedProperties.DropState.Drop);
                        }
                        break;
                    
                }
            }

            return new UpdateAdorner(false);
        }
    }


    //public class UpdateDragStatus : ScriptCommandBase
    //{
    //    AttachedProperties.DropStatus _status;
    //    public UpdateDragStatus(AttachedProperties.DropStatus status)
    //        : base("UpdateDragStatus", "EventArgs")
    //    { _status = status; }

    //    public override IScriptCommand Execute(ParameterDic pm)
    //    {
    //        var pd = pm.AsUIParameterDic();
    //        var ic = pd.Sender as ItemsControl;
    //        var eventArgs = pd.EventArgs as DragEventArgs;

    //        //If initiate by this helper.
    //        if (eventArgs.Data.GetDataPresent(typeof(AttachedProperties.DragMethod)))
    //            if (((AttachedProperties.DragMethod)eventArgs.Data.GetData(typeof(AttachedProperties.DragMethod)))
    //                == AttachedProperties.DragMethod.Menu) //Need display Menu
    //                eventArgs.Data.SetData(typeof(AttachedProperties.DropStatus), _status);

    //        switch (_status)
    //        {
    //            case AttachedProperties.DropStatus.Drop: return new HideAdorner();
    //        }
    //    }
    //}

    public class BeginDrop : ScriptCommandBase
    {
        public BeginDrop() : base("BeginDrop", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            AttachedProperties.SetDraggingOverItem(ic, null);
            var eventArgs = pd.EventArgs as DragEventArgs;

            var isd = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportDrop);
            if (isd != null)
            {
                if (eventArgs.Data.GetDataPresent(typeof(AttachedProperties.DragMethod)))
                {
                    eventArgs.Data.SetData("A", "B");
                    Debug.WriteLine(eventArgs.Data.GetData("A"));
                }

                eventArgs.Effects = eventArgs.AllowedEffects & isd.QueryDrop(eventArgs.Data);


                if (eventArgs.Effects != DragDropEffects.None)
                {
                    IEnumerable<IDraggable> draggables = isd.QueryDropDraggables(eventArgs.Data);
                    isd.Drop(draggables, eventArgs.Data, eventArgs.AllowedEffects);
                    eventArgs.Handled = true;
                }
            }

            return ResultCommand.NoError;
        }
    }


   


}
