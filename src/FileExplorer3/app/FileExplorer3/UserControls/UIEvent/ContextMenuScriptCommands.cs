using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Defines;

namespace FileExplorer.BaseControls.Menu
{
    public class ShowContextMenu : ScriptCommandBase
    {
        private ContextMenu _contextMenu;
        public ShowContextMenu(ContextMenu contextMenu) : base("ShowContextMenu", "EventArgs") { _contextMenu = contextMenu; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var pd = pm.AsUIParameterDic();
            pd.IsHandled = true;
            object trigger = AttachedProperties.GetStartInput(pd.Sender as DependencyObject);
            if (trigger != null && trigger.Equals(MouseButton.Right))
            {
                _contextMenu.PlacementTarget = (pd.Sender as Control);
                _contextMenu.SetValue(ContextMenu.IsOpenProperty, true);
                pd.EventArgs.Handled = true;
            }

            return ResultCommand.NoError; //Set Handled to true.
        }


    }
}
