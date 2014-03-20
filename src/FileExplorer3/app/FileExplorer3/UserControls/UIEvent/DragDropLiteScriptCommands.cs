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
            if (DragLiteParameters.DragMode == DragMode.None)
            {

                //AttachedProperties.SetIsDragging(_ic, true);
                DragLiteParameters.DragInputType = pm.AsUIParameterDic().GetDragInputProcessor().StartInput.InputType;
                DragLiteParameters.DraggingItems = _isd.GetDraggables();
                DragLiteParameters.DragMode = DragMode.Lite;
                DragLiteParameters.Effects = _isd.QueryDrag(DragLiteParameters.DraggingItems);
                DragLiteParameters.DragSource = _isd;


                return CapturePointer.Release(null);
            }

            return ResultCommand.NoError;
        }
    }


    public class ContinueDragLite : ScriptCommandBase
    {
        private bool _enableDrop;
        private bool _enableDrag;
        public ContinueDragLite(bool enableDrag, bool enableDrop)
            : base("ContinueDragLite", "EventArgs")
        { _enableDrag = enableDrag; _enableDrop = enableDrop; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            //TODO: Find mouse over items control, and attach adorner.

            if (DragLiteParameters.DragMode == DragMode.Lite && (_enableDrag || _enableDrop))
            {

                //FrameworkElement directlyOverEle = null;
                //if (pd.Input.InputType == UIInputType.Touch)
                //    directlyOverEle = (pd.EventArgs as TouchEventArgs).TouchDevice.DirectlyOver as FrameworkElement;
                //else directlyOverEle = Mouse.DirectlyOver as FrameworkElement;


                //var isd = DataContextFinder.GetDataContext(ref directlyOverEle, DataContextFinder.SupportDrop);

                //Console.WriteLine(directlyOverEle);

                pd.EventArgs.Handled = true;
                //if (DragLiteParameters.DragInputType == pd.Input.InputType)
                {
                    pd.Input = new DragInput(pd.Input,
                        DragLiteParameters.DragSource.GetDataObject(DragLiteParameters.DraggingItems),
                        DragDropEffects.Copy, (eff) => { });

                    return new AttachAdorner(new UpdateAdorner(new UpdateAdornerText()));
                }
            }
            return ResultCommand.OK;
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

            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;
            //if (DragLiteParameters.DragInputType != pd.Input.InputType)
            //    return ResultCommand.NoError;

            //FrameworkElement directlyOverEle = null;
            //if (pd.Input.InputType == UIInputType.Touch)
            //    directlyOverEle = (pd.EventArgs as TouchEventArgs).TouchDevice.DirectlyOver as FrameworkElement;
            //else directlyOverEle = Mouse.DirectlyOver as FrameworkElement;





            DragLiteParameters.DragMode = DragMode.None;
            //var isd = DataContextFinder.GetDataContext(pd.Sender as FrameworkElement, DataContextFinder.SupportDrag);
            //if (isd != null && isd.Equals(DragLiteParameters.DragSource))
            //    return new DetachAdorner();

            //AttachedProperties.SetStartDraggingItem(ic, null);
            //AttachedProperties.SetIsDragging(ic, false);

            IDataObject da = DragLiteParameters.DragSource.GetDataObject(DragLiteParameters.DraggingItems);
            DragDropEffects queryEffs = DragLiteParameters.DragSource.QueryDrag(DragLiteParameters.DraggingItems);
            da.SetData(typeof(AttachedProperties.DragMethod), AttachedProperties.DragMethod.Menu);
            da.SetData(typeof(ISupportDrag), DragLiteParameters.DragSource);
            pd.Input = new DragInput(pd.Input, da, queryEffs, (eff) => { });

            return new BeginDrop();
            //return CapturePointer.Release(new DetachAdorner());
        }
    }

}
