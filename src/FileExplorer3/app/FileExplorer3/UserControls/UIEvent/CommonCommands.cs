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
    public enum SelectedItemTargetValue { ItemUnderMouse, Null }
    public class SetStartSelectedItem : ScriptCommandBase
    {
        public SetStartSelectedItem(ItemsControl ic, SelectedItemTargetValue targetValue, IScriptCommand nextCommand = null) :
            base("SelectedItemTargetValue", "EventArgs", "SelectionBounds", "SelectionBoundsAdjusted")
        { _ic = ic; _targetValue = targetValue; _nextCommand = nextCommand; }

        private IScriptCommand _nextCommand;
        private SelectedItemTargetValue _targetValue;
        private ItemsControl _ic;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            var scp = ControlUtils.GetScrollContentPresenter(_ic);
            var eventArgs = pd.EventArgs as MouseEventArgs;

            pm["StartSelectedItem"] = null;
            if (_targetValue == SelectedItemTargetValue.ItemUnderMouse)
            {
                var itemUnderMouse = UITools.GetSelectedListBoxItem(scp, eventArgs.GetPosition(scp));
                if (AttachedProperties.GetStartSelectedItem(_ic) == null)
                    AttachedProperties.SetStartSelectedItem(_ic, itemUnderMouse);
                pm["StartSelectedItem"] = itemUnderMouse;
            }
            else
            {
                AttachedProperties.SetStartSelectedItem(_ic, null);
            }

            return _nextCommand == null ? ResultCommand.NoError : _nextCommand;
        }
    }

    
    public class SetEventIsHandled : ScriptCommandBase
    {
        public SetEventIsHandled(bool toVal = true, IScriptCommand nextCommand = null) :
            base("SetIsHandled")
        { _nextCommand = nextCommand; _toVal = toVal; }

        private IScriptCommand _nextCommand;
        private bool _toVal;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            pd.EventArgs.Handled = _toVal;
            return _nextCommand == null ? ResultCommand.NoError : _nextCommand;
        }
    }

    public class SetIsHandled : ScriptCommandBase
    {
        public SetIsHandled(IScriptCommand nextCommand = null, bool toVal = true) :
            base("SetIsHandled")
        { _nextCommand = nextCommand; _toVal = toVal; }

        private IScriptCommand _nextCommand;
        private bool _toVal;

        public override IScriptCommand Execute(ParameterDic pm)
        {
            pm.IsHandled = _toVal;
            return _nextCommand == null ? ResultCommand.NoError : _nextCommand;
        }
    }

}
