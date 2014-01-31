using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class CommandViewAware : ViewAware, IExportCommandBindings
    {
        private ScriptBindingScope _scope;
        #region Constructor

        protected CommandViewAware(ScriptBindingScope scope = ScriptBindingScope.Local)
        {
            _scope = scope;
        }

        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var uiEle = view as System.Windows.UIElement;
            this.RegisterCommand(uiEle, _scope);
        }

        protected virtual IEnumerable<IScriptCommandBinding> getExportedCommands()
        {
            List<IScriptCommandBinding> retList = new List<IScriptCommandBinding>();
            if (ToolbarCommands != null)
                retList.AddRange(ToolbarCommands.ExportedCommandBindings);
            if (ScriptCommands != null)
                retList.AddRange(ScriptCommands.ExportedCommandBindings);
            return retList;
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

        public IToolbarCommandsHelper ToolbarCommands { get; protected set; }
        public IScriptCommandContainer ScriptCommands { get; protected set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get { return getExportedCommands(); } }        
    }
}
