using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels.Helpers
{
    /// <summary>
    /// Populate Commands (CommandViewModel, e.g. ToggleView), and notify the commands when AppliedModels changes.
    /// For use in toolbar and context menu.
    /// </summary>
    public interface IToolbarCommandsHelper : IExportCommandBindings
    {
        IEntryModel[] AppliedModels { get; }
       IEntriesHelper<ICommandViewModel> CommandModels { get; }        
    }

    public class ToolbarCommandsHelper : NotifyPropertyChanged, IToolbarCommandsHelper
    {
        #region Constructor

        public ToolbarCommandsHelper(IProfile[] rootProfiles, params ICommandModel[] extraCommands)
        {
            _extraCommands = extraCommands;
            _rootProfiles = rootProfiles;
            CommandModels = new EntriesHelper<ICommandViewModel>(loadCommandsTask);
        }

        #endregion

        #region Methods

        protected override void NotifyOfPropertyChanged(string propertyName)
        {
            base.NotifyOfPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "AppliedModels":
                    AsyncUtils.RunSync(() => CommandModels.LoadAsync(false));
                    foreach (var commandVM in CommandModels.AllNonBindable)
                        commandVM.CommandModel.NotifySelectionChanged(AppliedModels);
                    break;
            }
        }
        
        async Task<IEnumerable<ICommandViewModel>> loadCommandsTask()
        {
            List<ICommandModel> cmList = new List<ICommandModel>(_extraCommands) { };

            foreach (var cp in _rootProfiles.SelectMany(p => p.CommandProviders))
                cmList.AddRange(cp.GetCommandModels());

            return cmList.Select(cm => new CommandViewModel(cm)).ToArray();
        }


        #endregion

        #region Data

        IEntryModel[] _appliedModels = null;        
        IProfile[] _rootProfiles = null;
        private ICommandModel[] _extraCommands;
        protected List<IScriptCommandBinding> _exportedCommandBindings = new List<IScriptCommandBinding>();

        #endregion

        #region Public Properties

        public IEntryModel[] AppliedModels { get { return _appliedModels; } 
            set { _appliedModels = value; NotifyOfPropertyChanged(() => AppliedModels);  } }
        public IEntriesHelper<ICommandViewModel> CommandModels { get; private set; }

        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings
        {
            get { return _exportedCommandBindings; }
        }

        #endregion








       
    }
}
