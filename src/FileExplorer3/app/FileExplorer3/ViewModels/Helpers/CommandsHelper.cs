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

    public interface IEntryModelCommandHelper : IScriptCommandContainer
    {
        IEntryModel[] AppliedModels { get; }
        EntriesHelper<ICommandViewModel> CommandModels { get; }        
    }

    public class EntryModelCommandHelper : NotifyPropertyChanged, IEntryModelCommandHelper
    {
        #region Constructor

        public EntryModelCommandHelper(IProfile[] rootProfiles, params ICommandModel[] extraCommands)
        {
            _extraCommands = extraCommands;
            _rootProfiles = rootProfiles;
            CommandModels = new EntriesHelper<ICommandViewModel>(loadCommandsTask);
            ParameterDicConverter = ParameterDicConverters.ConvertParameterOnly;         
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
        public EntriesHelper<ICommandViewModel> CommandModels { get; private set; }
        public IParameterDicConverter ParameterDicConverter { get; protected set; }

        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings
        {
            get { return _exportedCommandBindings; }
        }

        #endregion








       
    }
}
