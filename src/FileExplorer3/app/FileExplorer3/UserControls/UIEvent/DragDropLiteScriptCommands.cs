using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Defines;
using FileExplorer.UserControls.InputProcesor;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileExplorer.BaseControls.DragnDrop
{
    public static class DragLiteParameters
    {
        public static DragMode DragMode = DragMode.None;
        public static IEnumerable<IDraggable> DraggingItems = null;
        public static UIInputType DragInputType = UIInputType.None;
        public static ISupportDrag DragSource = null;
        public static DragDropEffects Effects;
    }



    public class BeginDragLite : ScriptCommandBase
    {
        public BeginDragLite() : base("BeginDragLite") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;
            if (DragLiteParameters.DragMode == DragMode.Lite)
            {
                pd.EventArgs.Handled = true;
                return ResultCommand.OK;
            }

            return new QueryDrag((ic, isd) =>
            {
                //Set it handled so it wont call multi-select.
                pd.EventArgs.Handled = true;
                pd.IsHandled = true;
                AttachedProperties.SetIsDragging(ic, true);

                return new DoDragDropLite(ic, isd);
            }, ResultCommand.NoError);
        }
    }

    /// <summary>
    /// This does not call DoDragDrop, instead it set DragLiteParameters
    /// </summary>
    public class DoDragDropLite : ScriptCommandBase
    {
        private IDataObject _dataObj;
        private ItemsControl _ic;
        private ISupportDrag _isd;
        private AttachedProperties.DragMethod _dragMethod;

        public DoDragDropLite(ItemsControl ic, ISupportDrag isd) : base("DoDragDropLite") { _ic = ic; _isd = isd; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            Console.WriteLine("DoDragDropLite");
            //AttachedProperties.SetIsDragging(_ic, true);
            DragLiteParameters.DragInputType = pm.AsUIParameterDic().GetDragInputProcessor().StartInput.InputType;
            DragLiteParameters.DraggingItems = _isd.GetDraggables();
            DragLiteParameters.DragMode = DragMode.Lite;
            DragLiteParameters.Effects = _isd.QueryDrag(DragLiteParameters.DraggingItems);
            DragLiteParameters.DragSource = _isd;

            

            return ResultCommand.NoError;
        }
    }


    public class ContinueDragLite : ScriptCommandBase
    {
        public ContinueDragLite() : base("ContinueDragLite", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            //TODO: Find mouse over items control, and attach adorner.
            
            if (DragLiteParameters.DragMode == DragMode.Lite)
            {
                FrameworkElement directlyOverEle = null;
                if (pd.Input is TouchInput)
                    directlyOverEle = (pd.EventArgs as TouchEventArgs).TouchDevice.DirectlyOver as FrameworkElement;
                else directlyOverEle = Mouse.DirectlyOver as FrameworkElement;


                var isd = DataContextFinder.GetDataContext(ref directlyOverEle, DataContextFinder.SupportDrop);
             
                pd.EventArgs.Handled = true;
                return new AttachAdorner(
                    new UpdateAdorner(new UpdateAdornerTextLite(), 
                        pd1 => DragLiteParameters.DragSource.GetDataObject(DragLiteParameters.DraggingItems)));
            }
            return ResultCommand.OK;
        }
    }

    public class UpdateAdornerTextLite : ScriptCommandBase
    {
        public UpdateAdornerTextLite()
            : base("UpdateAdornerTextLite", "EventArgs")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            DragAdorner dragAdorner = pd["DragAdorner"] as DragAdorner;
            var ic = pd.Sender as ItemsControl;
            FrameworkElement ele;
            ISupportDrop isd = DataContextFinder.GetDataContext(pm, out ele, DataContextFinder.SupportDrop);
            if (isd != null && dragAdorner != null)
            {
                dragAdorner.Text = String.Format("{0} {1} items to {2}",
                  "Copy",
                    (int)pd["DraggingItemsCount"],
                    isd.DropTargetLabel);
            }

            return ResultCommand.NoError;
        }
    }

    public class EndDragLite : ScriptCommandBase
    {
        public EndDragLite() : base("EndDragLite", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;

            if (DragLiteParameters.DragMode != DragMode.Lite)
                return ResultCommand.NoError;

            if (DragLiteParameters.DragInputType != pd.Input.InputType)
                return ResultCommand.NoError;

            DragLiteParameters.DragMode = DragMode.None;
            //AttachedProperties.SetStartDraggingItem(ic, null);
            //AttachedProperties.SetIsDragging(ic, false);

            return CapturePointer.Release(new DetachAdorner());
        }
    }

}
