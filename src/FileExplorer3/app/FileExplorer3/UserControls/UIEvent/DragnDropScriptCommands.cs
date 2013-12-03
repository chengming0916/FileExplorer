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

namespace FileExplorer.UserControls
{
    public static class DragnDropScriptCommands
    {
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
            return new SetStartSelectedItem(ic, SelectedItemTargetValue.ItemUnderMouse, ResultCommand.OK);
        }
    }

    public class BeginDrag : ScriptCommandBase
    {
        public BeginDrag() : base("BeginDrag", "EventArgs") { }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var ic = pd.Sender as ItemsControl;
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            var eventArgs = pd.EventArgs as MouseEventArgs;

            Mouse.Capture(scp);
            return new UpdateIsSelecting(true);
        }
    }
}
