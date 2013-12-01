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

        public static IScriptCommand StartDrag = new StartDrag();
        public static IScriptCommand EndDrag = new EndDrag();
    }

    #region Attach, Detach and Update Adorner.

    public class AttachAdorner : ScriptCommandBase
    {
        public AttachAdorner() : base("AttachAdorner") { }

        protected bool CanExecute(ParameterDic pm)
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

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);

            if (AttachedProperties.GetIsDragging(c))
            {
                var lastAdorner =
                    pm.ContainsKey("SelectionAdorner") ?
                        (SelectionAdorner)pm["SelectionAdorner"] :
                        AttachedProperties.GetSelectionAdorner(scp);

                if (pd.ContainsKey("IsSelecting") && pd["IsSelecting"] is bool)
                    lastAdorner.IsSelecting = (bool)pd["IsSelecting"];

                if (pd.ContainsKey("StartPosition") && pd["StartPosition"] is Point)
                    lastAdorner.StartPosition = (Point)pd["StartPosition"];
                else
                    lastAdorner.StartPosition = AttachedProperties.GetStartPlusStartScrollbarPosition(c);

                if (pd.ContainsKey("EndPosition") && pd["EndPosition"] is Point)
                    lastAdorner.EndPosition = (Point)pd["EndPosition"];
                else if (_setCurPositionAsEndPosition)
                    lastAdorner.EndPosition = Mouse.GetPosition(c);
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
            Debug.WriteLine("StartDrag");
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            var eventArgs = pd.EventArgs as MouseEventArgs;

            var result = MultiSelectScriptCommands.AttachAdorner.Execute(pm);
            if (result != ResultCommand.OK) return result;
            
            pm["IsSelecting"] = true;
            result = MultiSelectScriptCommands.UpdateAdornerPosition.Execute(pm);
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
            Debug.WriteLine("EndDrag");
            var pd = pm.AsUIParameterDic();
            var c = pd.Sender as Control;
            var scp = ControlUtils.GetScrollContentPresenter(c);
            var eventArgs = pd.EventArgs as MouseEventArgs;

            var result = MultiSelectScriptCommands.DetachAdorner.Execute(pm);
            if (result != ResultCommand.OK) return result;

            Mouse.Capture(null);
            return ResultCommand.OK;
        }
    }
}
