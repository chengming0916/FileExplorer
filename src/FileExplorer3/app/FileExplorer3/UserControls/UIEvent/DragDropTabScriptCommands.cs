using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls.DragnDrop;
using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using FileExplorer.ViewModels.Helpers;
using System.Windows.Controls.Primitives;

namespace FileExplorer.BaseControls
{
    public static class DragTabParameters
    {
        public static DragMode DragMode = DragMode.None;
        public static IDraggable DraggingItem = null;
        public static UIInputType DragInputType = UIInputType.None;
    }

    public class BeginDragTab : ScriptCommandBase
    {
        public BeginDragTab() : base("BeginDragTab") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();

            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;
            var ic = pd.Sender as ItemsControl;
            var isr = DataContextFinder.GetDataContext(pm, DataContextFinder.SupportReorder);
            IDraggable sourceDraggable = isr == null ? null : isr.GetReorderable();

            if (sourceDraggable != null)
            {
                //Set it handled so it wont call multi-select.
                pd.EventArgs.Handled = true;
                pd.IsHandled = true;
                AttachedProperties.SetIsDragging(ic, true);

                return new DoDragDropTab(isr, sourceDraggable);
            }

            return ResultCommand.NoError;
        }
    }


    public class DoDragDropTab : ScriptCommandBase
    {
        private ISupportReorder _isr;
        private IDraggable _sourceDraggable;

        public DoDragDropTab(ISupportReorder isr, IDraggable sourceDraggable)
            : base("DoDragDropTab")
        { _isr = isr; _sourceDraggable = sourceDraggable; }


        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (DragLiteParameters.DragMode == DragMode.None)
            {
                DragTabParameters.DraggingItem = _sourceDraggable;
                DragTabParameters.DragMode = DragMode.Lite;

                //return new AttachAdorner(new UpdateAdorner(new UpdateAdornerText()));
            }

            return ResultCommand.NoError;
        }
    }


    public class EndDragTab : ScriptCommandBase
    {

        public EndDragTab()
            : base("EndDragTab")
        { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();

            if (pd.EventArgs.Handled)
                return ResultCommand.NoError;

            if (DragLiteParameters.DragMode != DragMode.Lite)
                return ResultCommand.NoError;

            TabItem destTabItem =
                UITools.FindAncestor<TabItem>(pd.EventArgs.OriginalSource as UIElement);
            IDraggable destDraggable = destTabItem == null ? null :
                dragOverTabItem.DataContext as IDraggable;
            if (tc != null && dragOverTabItem != null && dragOverTabItem != DragTabParameters.DraggingTab)
            {

            }

            return ResultCommand.NoError;
        }
    }
}
